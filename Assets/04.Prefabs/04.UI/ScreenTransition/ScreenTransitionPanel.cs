using UnityEngine;
using UnityEngine.Events;

// 부모 패널 영역 안에서 덮기와 걷기 애니메이션을 재생합니다.
[RequireComponent(typeof(RectTransform), typeof(Animator))]
public sealed class ScreenTransitionPanel : MonoBehaviour
{
    [SerializeField] private UnityEvent covered = new UnityEvent();
    [SerializeField] private UnityEvent revealed = new UnityEvent();

    private Animator animator;

    public Animator _animPet;
    [SerializeField] private string[] animationNames;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
    }

    // 비활성 상태에서도 호출할 수 있습니다.
    public void PlayCover()
    {
        gameObject.SetActive(true);
        animator.Play("Base Layer.Cover", 0, 0f);
        animator.Update(0f);
    }

    public void PlayReveal()
    {
        gameObject.SetActive(true);
        animator.Play("Base Layer.Reveal", 0, 0f);
        animator.Update(0f);
    }

    // Cover의 마지막 프레임: 부모 패널 영역이 완전히 가려졌을 때 호출됩니다.
    public void OnCoverComplete()
    {
        SpawnManager.Instance.RetreatStage();
        covered.Invoke();
    }

    // Reveal의 마지막 프레임: 클릭 차단 UI를 끈 다음 완료를 알립니다.
    public void OnRevealComplete()
    {
        gameObject.SetActive(false);
        revealed.Invoke();
    }

    public void PlayRandomAnimation()
    {
        if (animator == null)
            return;

        if (animationNames == null || animationNames.Length == 0)
            return;

        int randomIndex = Random.Range(0, animationNames.Length);

        string animName = animationNames[randomIndex];

        Debug.Log(randomIndex + ": " + animName);

        Debug.Log($"재생 애니메이션 : {animName}");

        animator.Play(animationNames[randomIndex], 0, 0f);
    }
}
