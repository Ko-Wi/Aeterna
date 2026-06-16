using System;
using System.Collections.Generic;
using UnityEngine;

public class ForgeManager : MonoBehaviour
{
    /********************************** ½Ì ±Û Åæ *******************************************/
    private static ForgeManager _instance;

    public static ForgeManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("ForgeManager Singleton == null");

            return _instance;
        }
    }
    /*************************************************************************************/

    private MyObject myChar;
    private GameManager gameManager;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        myChar = MyObject.MyChar;
        gameManager = GameManager.Instance;
    }

    public OwnedEquipment ForgeEquipment()
    {
        EquipmentSlotType slotType = GetRandomSummonSlot();
        EquipmentGrade grade = gameManager.GetRandomEquipmentGrade();
        int equipmentIndex = GetRandomIndexByGrade(slotType, grade);

        EquipmentStatusType statusType = GetMainStatusType(slotType);
        int statusValue = GetMainStatusValue(grade);

        List<EquipmentOption> options = CreateRandomOptions(slotType, grade);

        OwnedEquipment equipment = new OwnedEquipment(
            slotType,
            grade,
            equipmentIndex,
            statusType,
            statusValue,
            options
        );

        myChar.OwnedEquipments.Add(equipment);

        Debug.Log($"´ÜÁ¶ ¿Ï·á: {slotType} / {grade} / Index: {equipmentIndex} / {statusType}: {statusValue} / Option Count: {options.Count}");

        return equipment;
    }

    private EquipmentSlotType GetRandomSummonSlot()
    {
        if (myChar.WeaponIndex < 0)
        {
            return GetRandomWeaponType();
        }

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

    private EquipmentStatusType GetMainStatusType(EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Wand:
            case EquipmentSlotType.Staff:
                return EquipmentStatusType.Attack;

            case EquipmentSlotType.Helmet:
            case EquipmentSlotType.Chest:
            case EquipmentSlotType.Pants:
            case EquipmentSlotType.Boots:
            case EquipmentSlotType.Shield:
                return EquipmentStatusType.Defense;

            case EquipmentSlotType.Ring:
            case EquipmentSlotType.Amulet:
            case EquipmentSlotType.Belt:
                return EquipmentStatusType.Attack;

            default:
                return EquipmentStatusType.Attack;
        }
    }

    private int GetMainStatusValue(EquipmentGrade grade)
    {
        int gradeIndex = (int)grade;

        int minValue = 10 + gradeIndex * 10;
        int maxValue = 20 + gradeIndex * 15;

        return UnityEngine.Random.Range(minValue, maxValue + 1);
    }

    private List<EquipmentOption> CreateRandomOptions(EquipmentSlotType slotType, EquipmentGrade grade)
    {
        List<EquipmentOption> options = new List<EquipmentOption>();

        int optionCount = GetOptionCountByGrade(grade);

        for (int i = 0; i < optionCount; i++)
        {
            EquipmentOptionType optionType = GetRandomOptionType(slotType);
            int value = GetRandomOptionValue(optionType, grade);

            options.Add(new EquipmentOption(optionType, value));
        }

        return options;
    }

    private int GetOptionCountByGrade(EquipmentGrade grade)
    {
        switch (grade)
        {
            case EquipmentGrade.Common:
            case EquipmentGrade.Magic:
                return 0;

            case EquipmentGrade.Rare:
            case EquipmentGrade.Heroic:
                return UnityEngine.Random.Range(0, 2); // 0~1°³

            case EquipmentGrade.Legendary:
            case EquipmentGrade.Unique:
            case EquipmentGrade.Mythic:
                return UnityEngine.Random.Range(1, 3); // 1~2°³

            case EquipmentGrade.Ancient:
            case EquipmentGrade.Abyssal:
            case EquipmentGrade.Genesis:
                return 2;

            default:
                return 0;
        }
    }

    private EquipmentOptionType GetRandomOptionType(EquipmentSlotType slotType)
    {
        EquipmentOptionType[] optionTypes =
        {
            EquipmentOptionType.Attack,
            EquipmentOptionType.Hp,
            EquipmentOptionType.Defense,
            EquipmentOptionType.CriticalRate,
            EquipmentOptionType.CriticalDamage,
            EquipmentOptionType.GoldGain,
            EquipmentOptionType.ExpGain
        };

        int randomIndex = UnityEngine.Random.Range(0, optionTypes.Length);
        return optionTypes[randomIndex];
    }

    private int GetRandomOptionValue(EquipmentOptionType optionType, EquipmentGrade grade)
    {
        int gradeIndex = (int)grade;

        switch (optionType)
        {
            case EquipmentOptionType.CriticalRate:
                return UnityEngine.Random.Range(1 + gradeIndex, 3 + gradeIndex);

            case EquipmentOptionType.CriticalDamage:
                return UnityEngine.Random.Range(5 + gradeIndex * 2, 10 + gradeIndex * 3);

            case EquipmentOptionType.GoldGain:
            case EquipmentOptionType.ExpGain:
                return UnityEngine.Random.Range(1 + gradeIndex, 5 + gradeIndex * 2);

            default:
                return UnityEngine.Random.Range(5 + gradeIndex * 5, 10 + gradeIndex * 10);
        }
    }
}

public enum EquipmentStatusType
{
    Attack,
    Defense,
    Hp
}

public enum EquipmentOptionType
{
    Attack,
    Hp,
    Defense,
    CriticalRate,
    CriticalDamage,
    GoldGain,
    ExpGain
}


[Serializable]
public class EquipmentOption
{
    public EquipmentOptionType OptionType;
    public int Value;

    public EquipmentOption(EquipmentOptionType optionType, int value)
    {
        OptionType = optionType;
        Value = value;
    }
}