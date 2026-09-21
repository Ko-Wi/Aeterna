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

        _anim.Play("Attack", 0, 0f);
    }

    public void EnemyHit()
    {
        if (!isAttacking) return;

        List<MonsterController> targets = FindAttackTargets(maxProjectileTargetCount);

        if (targets.Count == 0)
        {
            EndAttack();
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            FireProjectile(targets[i]);
        }
    }
    //private List<MonsterController> FindAttackTargets(int maxCount)
    //{
    //    List<MonsterController> targets = new List<MonsterController>();

    //    Collider2D[] colliders = Physics2D.OverlapCircleAll(
    //        attackCenter.position,
    //        attackRange,
    //        monsterLayer
    //    );

    //    if (colliders == null || colliders.Length == 0)
    //        return targets;

    //    List<MonsterController> tempTargets = new List<MonsterController>();

    //    for (int i = 0; i < colliders.Length; i++)
    //    {
    //        MonsterController monster = colliders[i].GetComponent<MonsterController>();

    //        if (monster == null) continue;
    //        if (!monster.gameObject.activeInHierarchy) continue;

    //        tempTargets.Add(monster);
    //    }

    //    // 가까운 몬스터 우선 정렬
    //    tempTargets.Sort((a, b) =>
    //    {
    //        float distanceA = Vector2.Distance(attackCenter.position, a.transform.position);
    //        float distanceB = Vector2.Distance(attackCenter.position, b.transform.position);

    //        return distanceA.CompareTo(distanceB);
    //    });

    //    for (int i = 0; i < tempTargets.Count; i++)
    //    {
    //        if (targets.Count >= maxCount)
    //            break;

    //        targets.Add(tempTargets[i]);
    //    }

    //    return targets;
    //}

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

        if (projectileList == null || projectileList.Count == 0)
        {
            Debug.LogWarning("projectileList에 Projectile 프리팹이 없습니다.");
            return;
        }

        if (projectileIndex < 0 || projectileIndex >= projectileList.Count)
        {
            Debug.LogWarning("projectileIndex가 projectileList 범위를 벗어났습니다.");
            return;
        }

        GameObject projectilePrefab = projectileList[projectileIndex];

        if (projectilePrefab == null)
        {
            Debug.LogWarning("선택된 Projectile 프리팹이 비어있습니다.");
            return;
        }

        Vector3 spawnPos = attackPos != null
            ? attackPos.position
            : transform.position;

        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        ProjectileController projectile = projectileObj.GetComponent<ProjectileController>();

        if (projectile == null)
        {
            Debug.LogWarning("Projectile 프리팹에 ProjectileController가 없습니다.");
            Destroy(projectileObj);
            return;
        }

        projectile.Init(target, attackDamage);
    }

    public void EndAttack()
    {
        isAttacking = false;

        _anim.Play("Idle");
    }

    private void OnDrawGizmosSelected()
    {
        Transform center = attackCenter != null ? attackCenter : transform;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, attackRange);
    }
}
