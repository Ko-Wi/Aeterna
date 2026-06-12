using LayerLab.ArtMakerUnity;
using System;
using UnityEngine;

public class EquipmentSummoner : MonoBehaviour
{
    MyObject myChar;
    GameManager gameManager;
    public Animator animator;

    private void Start()
    {
        myChar = MyObject.MyChar;
        gameManager = GameManager.Instance;
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
            EquipmentSlotType.Glove,
            EquipmentSlotType.Boots,
            EquipmentSlotType.Ring,
            EquipmentSlotType.Earring,
            EquipmentSlotType.Belt,
            EquipmentSlotType.Shield
        };

        
        int randomIndex = UnityEngine.Random.Range(0, summonParts.Length);
        return summonParts[randomIndex];
    }
    public void SummonEquipment()
    {
        EquipmentSlotType summonSlot = GetRandomSummonSlot();

        EquipmentGrade grade = gameManager.GetRandomEquipmentGrade();

        int equipmentIndex = GetRandomIndexByForgeLevel();

        AddOwnedEquipment(summonSlot, grade, equipmentIndex);

        //ApplySummonedEquipment(summonSlot, EquipmentIndex);

        Debug.Log($"뽑힌 장비 타입: {summonSlot} // {equipmentIndex} // {grade}");
    }
    private void AddOwnedEquipment(EquipmentSlotType slotType, EquipmentGrade grade, int index)
    {
        OwnedEquipment equipment = new OwnedEquipment(slotType, grade, index);
        myChar.OwnedEquipments.Add(equipment);

        Debug.Log($"보유 장비 추가: {slotType} / Index: {index} / 총 보유 수: {myChar.OwnedEquipments.Count}");
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

            case EquipmentSlotType.Glove:
                myChar.GloveIndex = index;
                break;

            case EquipmentSlotType.Boots:
                myChar.BootsIndex = index;
                break;

            case EquipmentSlotType.Ring:
                myChar.RingIndex = index;
                break;

            case EquipmentSlotType.Earring:
                myChar.EarringIndex = index;
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
