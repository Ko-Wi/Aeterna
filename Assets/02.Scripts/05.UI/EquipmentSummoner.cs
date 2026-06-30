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

    private void Start()
    {
        myChar = MyObject.MyChar;
        gameManager = GameManager.Instance;
        uiManager = UiManager.Instance;
        forgeManager = ForgeManager.Instance;
    }

    public void OnClickSummonButton()
    {
        // 클릭 애니메이션 트리거 실행
        if (animator != null)
            animator.SetTrigger("Click");
    }
    /// <summary>
    /// 장비 소환 시 어떤 부위가 나올지 결정합니다.
    /// 무기를 착용하지 않은 상태라면 무기를 최우선으로 반환합니다.
    /// </summary>
    public EquipmentSlotType GetRandomSummonSlot()
    {
        // 무기를 착용하지 않았다면 무기 최우선
        if (myChar.WeaponIndex < 0)
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
    public void SummonEquipment()
    {
        EquipmentSlotType summonSlot = GetRandomSummonSlot();

        //등급 랜덤으로 뽑기
        EquipmentGrade grade = gameManager.GetRandomEquipmentGrade();

        int equipmentIndex = GetRandomIndexByGrade(summonSlot, grade);

        AddOwnedEquipment(summonSlot, grade, equipmentIndex);

        //ApplySummonedEquipment(summonSlot, EquipmentIndex);

        Debug.Log($"뽑힌 장비 타입: {summonSlot} // {equipmentIndex} // {grade}");

        uiManager.SummonUiSet();

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
    private void ApplySummonedEquipment(EquipmentSlotType slotType, int index)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Wand:
                myChar.WeaponType = PartsType.Wand;
                myChar.WeaponIndex = index;
                break;

            case EquipmentSlotType.Staff:
                myChar.WeaponType = PartsType.Staff;
                myChar.WeaponIndex = index;
                break;

            case EquipmentSlotType.Helmet:
                myChar.HelmetIndex = index;
                break;

            case EquipmentSlotType.Chest:
                myChar.ChestIndex = index;
                break;

            case EquipmentSlotType.Pants:
                myChar.PantsIndex = index;
                break;

            case EquipmentSlotType.Boots:
                myChar.BootsIndex = index;
                break;

            case EquipmentSlotType.Ring:
                myChar.RingIndex = index;
                break;

            case EquipmentSlotType.Amulet:
                myChar.AmuletIndex = index;
                break;

            case EquipmentSlotType.Belt:
                myChar.BeltIndex = index;
                break;

            case EquipmentSlotType.Shield:
                myChar.LeftItemType = PartsType.Shield;
                myChar.LeftItemIndex = index;
                break;
        }
    }
}
