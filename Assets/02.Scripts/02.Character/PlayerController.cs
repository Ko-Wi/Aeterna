using LayerLab.ArtMakerUnity;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    MyObject myChar;

    [SerializeField] private PartsManager partsManager;

    public Animator _anim;

    [SerializeField] private PresetData equipmentPresetData;
    public PartsType partType;
    public int partIndex;
    public bool boolPart;
    public PartsManager PartsManager => partsManager;

    [Header("공격 설정")]
    [SerializeField] private bool isAttacking;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private int attackDamage = 10;
    // 문자열 대신 캐시한 파라미터 번호로 Animator 값을 변경합니다.
    private static readonly int attackSpeedHash = Animator.StringToHash("Speed");
    [SerializeField] private LayerMask monsterLayer;
    [SerializeField] private Transform attackCenter;

    [Header("Projectile")]
    [SerializeField] private List<GameObject> projectileList;
    [SerializeField] private Transform attackPos;
    [SerializeField] private int projectileIndex = 0;

    [Header("멀티샷")]
    [SerializeField] private int maxProjectileTargetCount = 1;

    // 현재 공격 중인 타겟 목록
    private readonly List<MonsterController> attackTargets = new List<MonsterController>();
    
    // 풀에서 재사용된 개체를 이전 타겟과 구분하기 위한 소환 번호
    private readonly Dictionary<MonsterController, uint> targetVersions = new Dictionary<MonsterController, uint>();

    // 마지막으로 발사를 처리한 공격 애니메이션 회차
    private int lastAttackLoop = -1;

    // 같은 프레임에 투사체 발사가 중복 처리되는 것을 방지
    private int lastFireFrame = -1;

    void Awake()
    {
        _anim = transform.GetComponentInChildren<Animator>();

        if (attackCenter == null)
            attackCenter = transform;

        if (attackPos == null)
            attackPos = attackCenter;
    }
    void Start()
    {
        Init();
    }
    public void Init()
    {
        myChar = MyObject.MyChar;

        if (partsManager != null)
        {
            partsManager.Init();
            _anim.Play("Idle");
        }

        //UnequipAllParts();    
    }
    // Update is called once per frame
    void Update()
    {
        // 무기 장착 중이면 자동 공격 체크
        TryAutoAttack();

        // 장비 교체나 Inspector 테스트로 변경된 공격속도를 반영합니다.
        RefreshAttackSpeed();
    }

    //모든파츠 벗기
    public void UnEquipAllParts()
    {
        if (partsManager == null) return;

        var allTypes = partsManager.GetAllPartsTypes();

        for (int i = 0; i < allTypes.Length; i++)
        {
            var type = allTypes[i];

            // Arrow, HelmetHair는 자동 동기화용이라 직접 빼도 되고 안 빼도 되는데
            // 굳이 건너뛰고 싶으면 continue 처리 가능
            partsManager.UnequipParts(type);
        }
        if (myChar.EyeIndex >= 0)
            partsManager.EquipParts(PartsType.Eye, myChar.EyeIndex);

        //색상 초기화
        partsManager.SetColor(ColorTargetType.Skin, Color.white);
        partsManager.SetColor(ColorTargetType.Hair, Color.white);
        partsManager.SetColor(ColorTargetType.Eye, Color.white);
        partsManager.SetColor(ColorTargetType.Beard, Color.white);
    }

    //장비 착용 해주는 코드
    public void ApplyEquipFromMyChar()
    {
        var partType = PartsType.Wand;

        if(myChar.EquippedWeapon.SlotType == EquipmentSlotType.Wand)
        {
            partType = PartsType.Wand;
        }
        else if (myChar.EquippedWeapon.SlotType == EquipmentSlotType.Staff)
        {
            partType = PartsType.Staff;
        }
        if (myChar.HairIndex >= 0)
            partsManager.EquipParts(PartsType.Hair, myChar.HairIndex);

        if (myChar.BeardIndex >= 0)
            partsManager.EquipParts(PartsType.Beard, myChar.BeardIndex);

        if (myChar.EquippedWeapon.EquipmentIndex >= 0)
            EquipGroupExclusive(partType, myChar.EquippedWeapon.EquipmentIndex);

        if (myChar.EquippedChest.EquipmentIndex >= 0)
            EquipGroupExclusive(PartsType.Chest, myChar.EquippedChest.EquipmentIndex);

        if (myChar.EquippedHelmet.EquipmentIndex >= 0)
            partsManager.EquipParts(PartsType.Helmet, myChar.EquippedHelmet.EquipmentIndex);

        if (myChar.EquippedShield.EquipmentIndex >= 0)
            EquipGroupExclusive(PartsType.Shield, myChar.EquippedShield.EquipmentIndex);
    }

    /// <summary>
    /// 같은 그룹(예: 오른손 무기, 왼손 장비) 안에서는
    /// 하나만 보이도록 처리한 뒤 장착합니다.
    /// </summary>
    private void EquipGroupExclusive(PartsType targetType, int targetIndex)
    {
        var groupTypes = GetGroupSubTypes(targetType);

        // 그룹 파츠라면 같은 그룹의 다른 파츠들을 먼저 끔
        if (groupTypes != null)
        {
            for (int i = 0; i < groupTypes.Length; i++)
            {
                PartsType type = groupTypes[i];

                if (type == targetType)
                    continue;

                if (partsManager.CanToggle(type))
                    partsManager.ToggleParts(type, false);
            }
        }

        // 내가 선택한 파츠만 켜고 장착
        if (partsManager.CanToggle(targetType))
            partsManager.ToggleParts(targetType, true);

        partsManager.EquipParts(targetType, targetIndex);
    }

    /// <summary>
    /// 전달받은 PartsType이 속한 그룹의 모든 서브타입을 반환합니다.
    /// 그룹이 아니면 null 반환.
    /// </summary>
    private PartsType[] GetGroupSubTypes(PartsType targetType)
    {
        foreach (UICategory uiCategory in System.Enum.GetValues(typeof(UICategory)))
        {
            if (!UICategoryConfig.IsGroup(uiCategory))
                continue;

            var subTypes = UICategoryConfig.GetSubTypes(uiCategory);

            for (int i = 0; i < subTypes.Length; i++)
            {
                if (subTypes[i] == targetType)
                    return subTypes;
            }
        }

        return null;
    }

    /// <summary>
    /// 무기 장착 상태이고, 범위 안에 몬스터가 있으면 자동 공격합니다.
    /// </summary>

    private void TryAutoAttack()
    {
        if (myChar == null) return;

        // 무기 미착용이면 공격하지 않음
        if (myChar.EquippedWeapon.EquipmentIndex < 0)
        {
            if (isAttacking)
                EndAttack();

            return;
        }

        // 공격 중일 때 몬스터가 없으면 공격 종료
        if (isAttacking)
        {
            if (!HasAttackTarget())
                EndAttack();

            return;
        }

        // 공격 중이 아닐 때 몬스터가 없으면 아무것도 안 함
        if (!HasAttackTarget()) return;

        Attack();
    }
    private bool HasAttackTarget()
    {
        Collider2D target = Physics2D.OverlapCircle(
            attackCenter.position,
            attackRange,
            monsterLayer
        );

        if (target == null) return false;

        MonsterController monster = target.GetComponent<MonsterController>();

        if (monster == null) return false;
        if (!monster.gameObject.activeInHierarchy) return false;

        return true;
    }
    
    private void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;

        // 새 공격이므로 애니메이션 회차 기록을 초기화합니다.
        lastAttackLoop = -1;

        _anim.Play("Attack", 0, 0f);

        //if (isAttacking) return;

        //isAttacking = true;

        //_anim.Play("Attack", 0, 0f);
    }

    public void EnemyHit()
    {
        if (!isAttacking) return;

        AnimatorStateInfo state = _anim.GetCurrentAnimatorStateInfo(0);

        if (!state.IsName("Attack"))
            return;

        // normalizedTime: 0~1은 첫 바퀴, 1~2는 두 번째 바퀴입니다.
        int attackLoop = Mathf.FloorToInt(state.normalizedTime);

        // 이미 처리한 공격 회차의 이벤트는 무시합니다.
        if (attackLoop <= lastAttackLoop)
            return;

        lastAttackLoop = attackLoop;

        // 재개 시 여러 회차의 이벤트가 같은 프레임에 들어와도
        // 한 번만 발사하고 나머지는 건너뜁니다.
        if (lastFireFrame == Time.frameCount)
            return;

        List<MonsterController> targets =
            FindAttackTargets(maxProjectileTargetCount);

        if (targets.Count == 0)
        {
            EndAttack();
            return;
        }

        lastFireFrame = Time.frameCount;

        // 멀티 타겟 기능은 유지합니다.
        // 이번 공격에서 선택된 타겟마다 한 발씩 발사합니다.
        for (int i = 0; i < targets.Count; i++)
        {
            FireProjectile(targets[i]);
        }
    }

    private List<MonsterController> FindAttackTargets(int maxCount)
    {
        maxCount = Mathf.Max(0, maxCount);

        Vector2 center = attackCenter.position;
        float rangeSquared = attackRange * attackRange;

        // 기존 타겟 중 더 이상 공격할 수 없는 대상만 제거
        for (int i = attackTargets.Count - 1; i >= 0; i--)
        {
            MonsterController target = attackTargets[i];

            bool isValid = target != null && target.IsTargetable &&
                targetVersions.TryGetValue(target, out uint version) && target.SpawnVersion == version &&
                ((Vector2)target.transform.position - center).sqrMagnitude <= rangeSquared;

            if (!isValid)
            {
                // 파괴된 Unity 오브젝트도 Dictionary에서 제거
                if (!ReferenceEquals(target, null))
                    targetVersions.Remove(target);

                attackTargets.RemoveAt(i);
            }
        }

        // 최대 타겟 수가 감소했다면 뒤쪽 타겟부터 해제
        while (attackTargets.Count > maxCount)
        {
            int lastIndex = attackTargets.Count - 1;

            targetVersions.Remove(attackTargets[lastIndex]);
            attackTargets.RemoveAt(lastIndex);
        }

        // 현재 타겟이 충분하면 새로 탐색하지 않음
        if (attackTargets.Count >= maxCount)
            return attackTargets;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(center, attackRange, monsterLayer);

        List<MonsterController> candidates = new List<MonsterController>();

        foreach (Collider2D hit in colliders)
        {
            MonsterController monster = hit.GetComponentInParent<MonsterController>();

            if (monster == null || !monster.IsTargetable)
                continue;

            // 이미 선택한 적이나 중복 Collider는 제외
            if (attackTargets.Contains(monster) || candidates.Contains(monster))
            {
                continue;
            }

            // 기존 타겟 유지 조건과 동일하게 중심 거리로 검사
            if (((Vector2)monster.transform.position - center).sqrMagnitude
                > rangeSquared)
            {
                continue;
            }

            candidates.Add(monster);
        }

        // 빈 타겟 자리를 채울 때만 가까운 적부터 선택
        candidates.Sort((a, b) =>
        {
            float distanceA = ((Vector2)a.transform.position - center).sqrMagnitude;

            float distanceB = ((Vector2)b.transform.position - center).sqrMagnitude;

            return distanceA.CompareTo(distanceB);
        });

        foreach (MonsterController monster in candidates)
        {
            if (attackTargets.Count >= maxCount)
                break;

            attackTargets.Add(monster);
            targetVersions[monster] = monster.SpawnVersion;
        }

        return attackTargets;
    }
    private void FireProjectile(MonsterController target)
    {
        if (target == null) return;

        GameObject projectilePrefab = projectileList[projectileIndex];

        Vector3 spawnPos = attackPos != null ? attackPos.position : transform.position;

        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        ProjectileController projectile = projectileObj.GetComponent<ProjectileController>();

        if (projectile == null)
        {
            Debug.LogWarning("Projectile 프리팹에 ProjectileController가 없습니다.");
            Destroy(projectileObj);
            return;
        }

        projectile.Init(target, myChar.GetTotalAttack());
        // 투사체마다 크리티컬 여부를 한 번 판정합니다.
        bool critical = myChar.IsCriticalHit();
        // 착용 장비의 기본 공격력을 합산합니다.
        double damage = myChar.TotalAttack;

        // 크리티컬이면 지정한 배율을 적용합니다.
        if (critical)
            damage *= myChar.CriticalDamageMultiplier;

        // 발사 시점의 최종 데미지와 판정 결과를 함께 전달합니다.
        projectile.Init(target, damage, critical);
    }

    public void EndAttack()
    {
        isAttacking = false;

        _anim.Play("Idle");
    }

    // 저장된 최종 공격속도가 바뀌었을 때만 Animator에 반영합니다.
    // 장비 옵션을 여기서 다시 합산하지 않습니다.
    private void RefreshAttackSpeed()
    {
        if (myChar == null || _anim == null)
            return;

        float speed = Mathf.Max(0.01f, myChar.TotalAttackSpeed);

        _anim.SetFloat(attackSpeedHash, speed);
    }
    private void OnDrawGizmosSelected()
    {
        Transform center = attackCenter != null ? attackCenter : transform;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, attackRange);
    }
}
