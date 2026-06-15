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


    void Awake()
    {
        _anim = transform.GetComponentInChildren<Animator>();

        if (attackCenter == null)
            attackCenter = transform;
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

        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    ApplyEquipFromMyChar();
        //}
        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    partsManager.ToggleParts(partType, boolPart);
        //}
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    var item = equipmentPresetData.GetItem(partIndex);

        //    partsManager.ApplyPresetItem(item);

        //}

        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    UnequipAllParts();
        //}


        //// 테스트용: R 키를 누를 때마다 무기 1회 랜덤 뽑기
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    DrawRandomWeapon();
        //}

        // 무기 장착 중이면 자동 공격 체크
        TryAutoAttack();

    }

    public void UnequipAllParts()
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

    private void ApplyEquipFromMyChar()
    {
        if (myChar.HairIndex >= 0)
            partsManager.EquipParts(PartsType.Hair, myChar.HairIndex);

        if (myChar.BeardIndex >= 0)
            partsManager.EquipParts(PartsType.Beard, myChar.BeardIndex);

        if (myChar.HelmetIndex >= 0)
            partsManager.EquipParts(PartsType.Helmet, myChar.HelmetIndex);

        if (myChar.WeaponIndex >= 0)
            EquipGroupExclusive(myChar.WeaponType, myChar.WeaponIndex);

        if (myChar.LeftItemIndex >= 0)
            EquipGroupExclusive(myChar.LeftItemType, myChar.LeftItemIndex);
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
        if (isAttacking) return;

        // WeaponIndex가 -1이면 무기 미착용 상태
        if (myChar.WeaponIndex < 0) return;

        Collider2D target = Physics2D.OverlapCircle(
            attackCenter.position,
            attackRange,
            monsterLayer
        );

        if (target == null) return;
        Debug.Log("자동공격 시작!");
        Attack(target);
    }

    private void Attack(Collider2D target)
    {
        if (isAttacking) return;

        isAttacking = true;
        Debug.Log("공격모션 시작!!");
        _anim.Play("Attack", 0, 0f);

        MonsterController monster = target.GetComponent<MonsterController>();
    }

    private void EndAttack()
    {
        isAttacking = false;
        Debug.Log("공격모션 끝 아이들전환!!!");
        _anim.Play("Idle");
    }
    private void EnemyHit()
    {
        Debug.Log(11111);
    }
    private void OnDrawGizmosSelected()
    {
        Transform center = attackCenter != null ? attackCenter : transform;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, attackRange);
    }
}
