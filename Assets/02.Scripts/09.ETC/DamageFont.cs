using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageFont : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;

    [SerializeField] private float moveHeight = 60f;
    [SerializeField, Min(0.01f)] private float duration = 0.7f;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = Color.yellow;

    private RectTransform rect;
    private RectTransform parentRect;
    private Canvas canvas;
    // 크리티컬일 때만 표시할 배경 이미지
    [SerializeField] private Image criticalBackground;

    private Camera mainCamera;
    private Camera uiCamera;

    private GameManager gameManager;

    private Vector3 hitPosition;
    private Color textColor;
    private float elapsed;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;

        canvas = GetComponentInParent<Canvas>().rootCanvas;
        mainCamera = Camera.main;

        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (damageText == null)
            damageText = GetComponentInChildren<TMP_Text>(true);

        damageText.raycastTarget = false;
    }

    public void Init(GameManager manager, double damage, Vector3 position, bool critical)
    {
        // 이전 표시에서 투명해진 배경을 다시 보이게 합니다.
        Color backgroundColor = criticalBackground.color;
        backgroundColor.a = 1f;
        criticalBackground.color = backgroundColor;

        gameManager = manager;
        hitPosition = position;
        elapsed = 0f;

        // 크리티컬 여부에 맞춰 활성 상태를 설정합니다.
        criticalBackground.gameObject.SetActive(critical);

        damageText.text = damage.ToString("N0");

        textColor = critical ? criticalColor : normalColor;

        damageText.color = textColor;

        transform.SetAsLastSibling();

        UpdatePosition(0f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        float progress = Mathf.Clamp01(elapsed / duration);

        textColor.a = 1f - progress;
        damageText.color = textColor;

        // 텍스트와 같은 비율로 크리티컬 배경도 사라지게 합니다.
        Color backgroundColor = criticalBackground.color;
        backgroundColor.a = 1f - progress;
        criticalBackground.color = backgroundColor;

        if (progress >= 1f)
        {
            gameManager.ReturnDamageFont(gameObject);
        }
    }

    private void LateUpdate()
    {
        float progress = Mathf.Clamp01(elapsed / duration);
        UpdatePosition(progress);
    }

    private void UpdatePosition(float progress)
    {
        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(hitPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPosition,
            uiCamera,
            out Vector2 localPosition);

        // 부모 피벗 기준 좌표이므로 중앙 앵커 영향 없이 배치
        rect.anchorMin = parentRect.pivot;
        rect.anchorMax = parentRect.pivot;

        rect.anchoredPosition =
            localPosition + Vector2.up * moveHeight * progress;

        Vector3 position = rect.localPosition;
        position.z = 0f;
        rect.localPosition = position;
    }
}