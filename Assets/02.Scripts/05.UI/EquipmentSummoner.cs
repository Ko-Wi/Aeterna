using LayerLab.ArtMakerUnity;
using System;
using UnityEngine;

public class EquipmentSummoner : MonoBehaviour
{
    MyObject myChar;
    GameManager gameManager;
    UiManager uiManager;
    ForgeManager forgeManager;
    public Animator animator;

    private bool isSummoning;

    private void Start()
    {
        myChar = MyObject.MyChar;
        gameManager = GameManager.Instance;
        uiManager = UiManager.Instance;
        forgeManager = ForgeManager.Instance;
    }

    public void OnClickSummonButton()
    {
        // 이미 소환 애니메이션이 진행 중이면 클릭 무시
        if (isSummoning) return;

        if (animator == null) return;

        isSummoning = true;

        animator.ResetTrigger("Click");
        animator.SetTrigger("Click");
    }
    /// <summary>
    /// 장비 소환 시 어떤 부위가 나올지 결정합니다.
    /// 무기를 착용하지 않은 상태라면 무기를 최우선으로 반환합니다.
    /// </summary>
    public EquipmentSlotType GetRandomSummonSlot()
    {
        // 무기를 착용하지 않았다면 무기 최우선
        if (myChar.EquippedWeapon.EquipmentIndex == -1)
        {
            return GetRandomWeaponType();
        }

        // 무기를 이미 착용 중이면 전체 장비 부위 중 랜덤
        EquipmentSlotType[] summonParts =
        {
            EquipmentSlotType.Wand,
            EquipmentSlotType.Staff,
            EquipmentSlotType.Helmet,
            EquipmentSlotType.Chest,
            EquipmentSlotType.Pants,
            EquipmentSlotType.Boots,
            EquipmentSlotType.Ring,
            EquipmentSlotType.Amulet,
            EquipmentSlotType.Belt,
            EquipmentSlotType.Shield
        };

        
        int randomIndex = UnityEngine.Random.Range(0, summonParts.Length);
        return summonParts[randomIndex];
    }
    //장비뽑기 이벤트 
    //public void SummonEquipment()
    //{
    //    EquipmentSlotType summonSlot = GetRandomSummonSlot();

    //    //등급 랜덤으로 뽑기
    //    EquipmentGrade grade = gameManager.GetRandomEquipmentGrade();

    //    int equipmentIndex = GetRandomIndexByGrade(summonSlot, grade);

    //    AddOwnedEquipment(summonSlot, grade, equipmentIndex);

    //    //Debug.Log($"뽑힌 장비 타입: {summonSlot} // {equipmentIndex} // {grade}");
    //    uiManager.SummonUiSet();
    //}
    public void SummonEquipment()
    {
        // 혹시 애니메이션 이벤트가 중복 호출되어도 한 번만 처리
        if (!isSummoning)
            return;

        EquipmentSlotType summonSlot = GetRandomSummonSlot();

        EquipmentGrade grade = gameManager.GetRandomEquipmentGrade();

        int equipmentIndex = GetRandomIndexByGrade(summonSlot, grade);

        AddOwnedEquipment(summonSlot, grade, equipmentIndex);

        uiManager.SummonUiSet();

        // 소환 처리가 끝났으므로 다시 클릭 허용
        isSummoning = false;
    }

    //장비 단조
    private void AddOwnedEquipment(EquipmentSlotType slotType, EquipmentGrade grade, int index)
    {
        forgeManager.ForgeEquipment();
    }

    /// <summary>
    /// 오른손 무기 타입 중 하나를 랜덤으로 반환합니다.
    /// </summary>
    private EquipmentSlotType GetRandomWeaponType()
    {
        EquipmentSlotType[] weaponTypes =
        {
            EquipmentSlotType.Wand,
            EquipmentSlotType.Staff
        };

        int randomIndex = UnityEngine.Random.Range(0, weaponTypes.Length);
        return weaponTypes[randomIndex];
    }

    private int GetRandomIndexByForgeLevel()
    {
        int forgeLevel = Mathf.Clamp(myChar.ForgeLevel, 1, 10);

        int minIndex = (forgeLevel - 1) * 3;
        int maxIndex = minIndex + 2;

        return UnityEngine.Random.Range(minIndex, maxIndex + 1);
    }
    private int GetRandomIndexByGrade(EquipmentSlotType slotType, EquipmentGrade grade)
    {
        int[] counts = gameManager.GetIndexCountsBySlot(slotType);
        int gradeIndex = (int)grade;

        int minIndex = 0;

        for (int i = 0; i < gradeIndex; i++)
        {
            minIndex += counts[i];
        }

        int maxIndex = minIndex + counts[gradeIndex];

        return UnityEngine.Random.Range(minIndex, maxIndex);
    }
}
