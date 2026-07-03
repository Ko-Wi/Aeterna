using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinDropEffect : MonoBehaviour
{
    [Header("코인 오브젝트들")]
    [SerializeField] private RectTransform[] coinRects;
    [SerializeField] private Image[] coinImages;

    [Header("위치")]
    [SerializeField] private RectTransform startPosition;
    [SerializeField] private RectTransform targetPosition;

    [Header("퍼짐 설정")]
    [SerializeField] private float dropRangeX = 140f;
    [SerializeField] private float groundOffsetY = 100f;

    [Header("튕김 설정")]
    [SerializeField] private float firstJumpPower = 140f;
    [SerializeField] private float secondJumpPower = 70f;
    [SerializeField] private float thirdJumpPower = 35f;

    [SerializeField] private float firstJumpDuration = 0.35f;
    [SerializeField] private float secondJumpDuration = 0.22f;
    [SerializeField] private float thirdJumpDuration = 0.16f;

    [SerializeField] private float fadeDuration = 0.25f;

    private Sequence[] sequences;

    private void Awake()
    {
        sequences = new Sequence[coinRects.Length];

        for (int i = 0; i < coinRects.Length; i++)
        {
            if (coinRects[i] != null)
                coinRects[i].gameObject.SetActive(false);
        }
    }

    public void SellCoinEffectSetup()
    {
        if (coinRects == null || coinRects.Length == 0)
            return;

        Vector2 start = startPosition != null
            ? startPosition.anchoredPosition
            : Vector2.zero;

        Vector2 target = targetPosition != null
            ? targetPosition.anchoredPosition
            : Vector2.zero;

        for (int i = 0; i < coinRects.Length; i++)
        {
            PlayCoin(i, start);
        }
    }

    private void PlayCoin(int index, Vector2 start)
    {
        RectTransform coinRect = coinRects[index];

        if (coinRect == null)
            return;

        sequences[index]?.Kill();

        coinRect.gameObject.SetActive(true);
        coinRect.anchoredPosition = start;
        coinRect.localScale = Vector3.one;
        coinRect.localRotation = Quaternion.identity;

        Image coinImage = null;

        if (coinImages != null && index < coinImages.Length)
            coinImage = coinImages[index];

        if (coinImage != null)
        {
            Color color = coinImage.color;
            color.a = 1f;
            coinImage.color = color;
        }

        float randomX = Random.Range(-dropRangeX, dropRangeX);
        float randomDelay = Random.Range(0f, 0.08f);
        float randomRotation = Random.Range(-360f, 360f);

        float groundY = start.y - groundOffsetY;
        float endX = start.x + randomX;

        Vector2 firstTop = new Vector2(
            start.x + randomX * 0.35f,
            groundY + firstJumpPower
        );

        Vector2 firstGround = new Vector2(
            start.x + randomX * 0.65f,
            groundY
        );

        Vector2 secondTop = new Vector2(
            start.x + randomX * 0.8f,
            groundY + secondJumpPower
        );

        Vector2 secondGround = new Vector2(
            start.x + randomX * 0.9f,
            groundY
        );

        Vector2 thirdTop = new Vector2(
            endX,
            groundY + thirdJumpPower
        );

        Vector2 finalGround = new Vector2(
            endX,
            groundY
        );

        Sequence sequence = DOTween.Sequence();
        sequences[index] = sequence;

        sequence.AppendInterval(0.01f);

        // 처음 위로 튐
        sequence.Append(
            coinRect.DOAnchorPos(firstTop, firstJumpDuration * 0.45f)
                .SetEase(Ease.OutQuad)
        );

        // 첫 착지
        sequence.Append(
            coinRect.DOAnchorPos(firstGround, firstJumpDuration * 0.55f)
                .SetEase(Ease.InQuad)
        );

        // 두 번째 작은 튐
        sequence.Append(
            coinRect.DOAnchorPos(secondTop, secondJumpDuration * 0.45f)
                .SetEase(Ease.OutQuad)
        );

        sequence.Append(
            coinRect.DOAnchorPos(secondGround, secondJumpDuration * 0.55f)
                .SetEase(Ease.InQuad)
        );

        // 세 번째 더 작은 튐
        sequence.Append(
            coinRect.DOAnchorPos(thirdTop, thirdJumpDuration * 0.45f)
                .SetEase(Ease.OutQuad)
        );

        sequence.Append(
            coinRect.DOAnchorPos(finalGround, thirdJumpDuration * 0.55f)
                .SetEase(Ease.InQuad)
        );

        sequence.Join(
            coinRect.DORotate(
                new Vector3(0f, 0f, randomRotation),
                firstJumpDuration + secondJumpDuration + thirdJumpDuration,
                RotateMode.FastBeyond360
            )
        );

        //sequence.AppendInterval(0.1f);

        if (coinImage != null)
        {
            sequence.Append(
                coinImage.DOFade(0f, fadeDuration)
            );
        }

        sequence.OnComplete(() =>
        {
            coinRect.gameObject.SetActive(false);
        });
    }
    private void OnDisable()
    {
        KillAllSequences();
    }

    private void OnDestroy()
    {
        KillAllSequences();
    }

    private void KillAllSequences()
    {
        if (sequences == null)
            return;

        for (int i = 0; i < sequences.Length; i++)
        {
            sequences[i]?.Kill();
            sequences[i] = null;
        }
    }
}
