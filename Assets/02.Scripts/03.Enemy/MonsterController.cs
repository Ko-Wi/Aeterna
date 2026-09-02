using System;
using UnityEngine;
using UnityEngine.UI;

public interface IEnemy
{
    void AttackHit(double damage, bool Cri = false);
    void EnemyDestroy();
}
public enum EnemyCategory
{
    None,
    Nomal,
    Boss
}
public class MonsterController : MonoBehaviour, IEnemy
{
    MyObject myChar;
    GameManager gameManager;

    private Collider2D _collider2D;

    public EnemyCategory _enemyCategory;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arriveDistance = 0.05f;
    public Animator _anim;
    public Transform hpBarPose;

    public double maxHp;
    public double currentHp;

    public GameObject hpBar;
    [SerializeField] private Slider hpBarSlider;
    private RectTransform hpBarRect;
    private Camera mainCamera;


    private PathRoute pathRoute;
    private int currentPointIndex;
    private bool isInitialized;
    private bool isDead;
    public bool IsTargetable => isInitialized && !isDead && currentHp > 0;      //몬스터 사망체크

    [Header("이동 기준점")]
    [SerializeField] private Transform pathAnchor;
    private void Awake()
    {
        myChar = MyObject.MyChar;
        gameManager = GameManager.Instance;

        _anim = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        hpBarPose = transform.Find("hpBarPos");

        mainCamera = Camera.main;
    }
    public void Init(PathRoute route, int spawnPoint)
    {
        if (route == null || route.PointCount == 0)
            return;

        pathRoute = route;
        currentPointIndex = spawnPoint;

        currentHp = maxHp;
        isDead = false;
        isInitialized = true;

        if (_collider2D != null)
            _collider2D.enabled = true;

        if (_anim != null)
        {
            _anim.SetBool("Death", false);
            _anim.SetBool("Move", true);
        }


        CreateHpBar();      // 몬스터 생성 시 한 번
        UpdateHpBar();       // 데미지를 받을 때만
    }

    private void Update()
    {
        if (!isInitialized || isDead || pathRoute == null)
            return;

        MoveAlongPath();
    }
    private void LateUpdate()
    {
        if (!isInitialized || isDead)
            return;

        UpdateHpBarPosition();
    }
    private void CreateHpBar()
    {
        if (hpBar != null)
            return;

        hpBar = gameManager.GetHPBar();

        hpBarRect = hpBar.GetComponent<RectTransform>();
        hpBarSlider = hpBar.GetComponent<Slider>();

        UpdateHpBarPosition();
    }

    private void UpdateHpBarPosition()
    {
        if (hpBarRect == null || hpBarPose == null || mainCamera == null)
            return;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(hpBarPose.position);

        hpBarRect.position = screenPosition;
    }


    //몬스터 레인따라 움직이는 Path
    private void MoveAlongPath()
    {
        Transform target = pathRoute.GetPoint(currentPointIndex);
        if (target == null)
            return;

        // 지정하지 않으면 기존처럼 루트 Transform을 사용
        Vector3 currentPathPosition = pathAnchor != null ? pathAnchor.position : transform.position;

        Vector3 dir = target.position - currentPathPosition;

        if (dir.sqrMagnitude <= arriveDistance * arriveDistance)
        {
            // 루트가 아닌 PathAnchor가 정확히 WayPoint에 도착하도록 보정
            transform.position += target.position - currentPathPosition;

            currentPointIndex++;

            if (currentPointIndex >= pathRoute.PointCount)
                currentPointIndex = 0;

            return;
        }

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;

        //if (Mathf.Abs(dir.x) > 0.01f)
        //{
        //    Vector3 scale = transform.localScale;
        //    scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
        //    transform.localScale = scale;
        //}
    }
    public void AttackHit(double damage, bool Cri = false)
    {
        if (isDead)
            return;

        currentHp -= damage;

        if (currentHp < 0)
            currentHp = 0;

        UpdateHpBar();

        if (currentHp <= 0)
        {
            EnemyDestroy();
        }
    }


    public void EnemyDestroy()
    {
        if (isDead)
            return;

        isDead = true;
        isInitialized = false;

        myChar.RemoveMonsterCount();
        
        if (_collider2D != null)
            _collider2D.enabled = false;

        if (hpBar != null)
            hpBar.SetActive(false);

        if (_anim != null)
        {
            _anim.SetBool("Move", false);
            _anim.SetBool("Death", true);
        }
        else
        {
            OnDeathAnimationEnd();
        }
    }

    // Bear_Dead 애니메이션 마지막 프레임의 Animation Event에서 호출
    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }

    private void UpdateHpBar()
    {
        if (hpBarSlider == null)
            return;

        if (maxHp <= 0)
        {
            hpBarSlider.value = 0f;
            return;
        }

        hpBarSlider.value = (float)(currentHp / maxHp);
    }
}
