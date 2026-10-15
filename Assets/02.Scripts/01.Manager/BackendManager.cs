using System;
using TMPro;
using System.Collections.Generic;
using BackEnd;
using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using System.Text;
#endif

public class BackendManager : MonoBehaviour
{
    public enum AccountType
    {
        Guest = 0,
        Google = 1,
        Apple = 2
    }

    MyObject myChar;
 
    public static BackendManager Instance { get; private set; }

    private const string LoginTypeKey = "Account.LoginType";

    // Android에서 Apple 웹 로그인을 사용할 때 필요한 Service ID.
    // 이전 게임 값이 아니라 현재 게임에 등록한 값을 입력하세요.
    [SerializeField] private string appleServiceId;

    public AccountType CurrentAccountType { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public bool IsBusy { get; private set; }

    // UI에서 성공·실패 안내에 사용할 수 있는 이벤트
    public event Action OnAccountSuccess;
    public event Action<string> OnAccountFailed;

    // 플랫폼 SDK의 콜백을 Unity 메인 스레드에서 처리
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();

#if UNITY_IOS && !UNITY_EDITOR
    private IAppleAuthManager appleAuthManager;
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            appleAuthManager =
                new AppleAuthManager(new PayloadDeserializer());
        }
#endif

        // 뒤끝 초기화
        // 비동기 콜백은 SDK에서 자동으로 메인 스레드에서 실행합니다.
        var result = Backend.Initialize();

        if (!result.IsSuccess())
        {
            Fail($"뒤끝 초기화 실패: {result.GetStatusCode()}");
            return;
        }

        StartLogin();
    }

    private void Update()
    {
        if (Instance != this)
            return;

#if UNITY_IOS && !UNITY_EDITOR
        appleAuthManager?.Update();
#endif

        while (true)
        {
            Action action;

            lock (mainThreadActions)
            {
                if (mainThreadActions.Count == 0)
                    break;

                action = mainThreadActions.Dequeue();
            }

            action.Invoke();
        }
    }

    // 시작 시 실행. 로그인 실패 후 재시도 버튼에도 연결 가능
    public void StartLogin()
    {
        if (!Backend.IsInitialized || IsBusy || IsLoggedIn)
            return;

        int savedType = PlayerPrefs.GetInt(
            LoginTypeKey,
            (int)AccountType.Guest
        );

        if (!Enum.IsDefined(typeof(AccountType), savedType))
        {
            Fail("저장된 로그인 방식이 올바르지 않습니다.");
            return;
        }

        AccountType accountType = (AccountType)savedType;
        IsBusy = true;

        if (accountType == AccountType.Guest)
        {
            // 기존 게스트 정보가 있으면 로그인,
            // 없으면 새로운 게스트 생성 후 로그인
            Backend.BMember.GuestLogin("Aeterna Guest", result =>
            {
                if (!result.IsSuccess())
                {
                    Fail($"게스트 로그인 실패: {result.GetStatusCode()}");
                    return;
                }

                Complete(AccountType.Guest);
            });
        }
        else
        {
            // 저장된 Google 또는 Apple 방식으로 인증 시작
            RequestPlatformLogin(accountType, false);
        }
    }

    // 설정의 Google 연동 버튼에 연결
    public void LinkWithGoogle()
    {
        BeginLink(AccountType.Google);
    }

    // 설정의 Apple 연동 버튼에 연결
    public void LinkWithApple()
    {
        BeginLink(AccountType.Apple);
    }

    private void BeginLink(AccountType targetType)
    {
        if (IsBusy)
            return;

        if (!IsLoggedIn)
        {
            Fail("로그인 완료 후 계정을 연결하세요.");
            return;
        }

        if (CurrentAccountType == targetType)
        {
            Debug.Log("이미 연결된 로그인 방식입니다.");
            return;
        }

        IsBusy = true;
        RequestPlatformLogin(targetType, true);
    }

    // linking: true는 계정 전환, false는 기존 계정 로그인
    private void RequestPlatformLogin(
        AccountType targetType,
        bool linking)
    {
        // SDK에서 중복 콜백이 와도 한 번만 처리
        int completed = 0;

        Action<bool, string> finish = (success, token) =>
        {
            if (System.Threading.Interlocked.Exchange(
                    ref completed, 1) != 0)
                return;

            lock (mainThreadActions)
            {
                mainThreadActions.Enqueue(() =>
                {
                    if (!success || string.IsNullOrWhiteSpace(token))
                    {
                        Fail("계정 인증이 취소되었거나 실패했습니다.");
                        return;
                    }

                    HandlePlatformToken(targetType, token, linking);
                });
            }
        };

        try
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (targetType == AccountType.Google)
            {
                TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(
                    true,
                    (success, error, token) =>
                    {
                        finish(success, token);
                    }
                );
            }
            else
            {
                if (string.IsNullOrWhiteSpace(appleServiceId))
                {
                    Fail("현재 게임의 Apple Service ID를 입력하세요.");
                    return;
                }

                // 인증창을 닫았을 때 요청 상태 해제
                TheBackend.ToolKit.AppleLogin.Android.OnCloseButtonClick =
                    () => finish(false, null);

                TheBackend.ToolKit.AppleLogin.Android.AppleLogin(
                    appleServiceId,
                    out var error,
                    true,
                    token => finish(true, token)
                );

                if (!string.IsNullOrEmpty(error))
                    finish(false, null);
            }

#elif UNITY_IOS && !UNITY_EDITOR
            if (targetType == AccountType.Google)
            {
                TheBackend.ToolKit.GoogleLogin.iOS.GoogleLogin(
                    (success, error, token) =>
                    {
                        finish(success, token);
                    }
                );
            }
            else
            {
                if (appleAuthManager == null)
                {
                    Fail("Apple 로그인 초기화가 필요합니다.");
                    return;
                }

                var args = new AppleAuthLoginArgs(
                    LoginOptions.IncludeEmail |
                    LoginOptions.IncludeFullName
                );

                appleAuthManager.LoginWithAppleId(
                    args,
                    credential =>
                    {
                        var appleCredential =
                            credential as IAppleIDCredential;

                        if (appleCredential?.IdentityToken == null)
                        {
                            finish(false, null);
                            return;
                        }

                        string token = Encoding.UTF8.GetString(
                            appleCredential.IdentityToken
                        );

                        finish(true, token);
                    },
                    error => finish(false, null)
                );
            }
#else
            Fail("Google·Apple 인증은 Android/iOS 실기기에서 실행하세요.");
#endif
        }
        catch (Exception exception)
        {
            finish(false, null);

            // 인증 토큰은 로그에 출력하지 않음
            Debug.LogWarning(
                $"플랫폼 인증 요청 오류: {exception.GetType().Name}"
            );
        }
    }

    private void HandlePlatformToken(
        AccountType targetType,
        string token,
        bool linking)
    {
        FederationType federationType =
            targetType == AccountType.Google
                ? FederationType.Google
                : FederationType.Apple;

        if (!linking)
        {
            // 기존 연동 계정으로 로그인
            Backend.BMember.AuthorizeFederation(
                token,
                federationType,
                result =>
                {
                    if (!result.IsSuccess())
                    {
                        Fail($"계정 로그인 실패: {result.GetStatusCode()}");
                        return;
                    }

                    Complete(targetType);
                }
            );

            return;
        }

        if (CurrentAccountType == AccountType.Guest)
        {
            // 게스트 → Google 또는 Apple
            Backend.BMember.ChangeCustomToFederation(
                token,
                federationType,
                result => FinishLink(result, targetType)
            );
        }
        else
        {
            // Google → Apple 또는 Apple → Google
            // 기존 게임 계정은 유지하고 로그인 수단을 교체
            Backend.BMember.ChangeToFederation(
                token,
                federationType,
                result => FinishLink(result, targetType)
            );
        }
    }

    private void FinishLink(
        BackendReturnObject result,
        AccountType targetType)
    {
        if (!result.IsSuccess())
        {
            if (result.GetStatusCode() == "409")
            {
                Fail("이미 다른 게임 계정에 연결된 계정입니다.");
            }
            else
            {
                Fail($"계정 연결 실패: {result.GetStatusCode()}");
            }

            // 실패하면 기존 계정과 로그인 방식을 유지
            return;
        }

        Complete(targetType);
    }

    private void Complete(AccountType accountType)
    {
        bool firstLogin = !IsLoggedIn;
        // 서버에서 로그인/전환 성공을 받은 뒤에만 저장
        CurrentAccountType = accountType;

        PlayerPrefs.SetInt(LoginTypeKey, (int)accountType);
        PlayerPrefs.Save();

        IsLoggedIn = true;
        IsBusy = false;

        if (firstLogin)
        {
            MyObject myChar = MyObject.MyChar;

            // 서버 시간 동기화 → 방치 데이터 로딩
            myChar.InitializeServerTime(myChar.LoadIdleStartTimeUtc);
        }

        Debug.Log($"로그인/계정 연결 완료: {accountType}");

        OnAccountSuccess?.Invoke();
    }

    private void Fail(string message)
    {
        IsBusy = false;

        // 연동 실패 시 기존 로그인 상태와 PlayerPrefs는 유지
        Debug.LogWarning(message);
        OnAccountFailed?.Invoke(message);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
