using System;
using System.Collections.Generic;

[Serializable]
public class Equipment
{
    public EquipmentSlotType SlotType;          // 장비종류
    public EquipmentGrade Grade;                // 등급
    public int EquipmentIndex;                  // 뽑힌 장비 Index
    public int EquipmentLevel;                  // 장비 레벨

    public EquipmentStatusType MainStatusType;
    public int MainStatusValue;

    public List<EquipmentOption> Options = new List<EquipmentOption>();

    public Equipment(
        EquipmentSlotType slotType,
        EquipmentGrade grade,
        int equipmentIndex,
        int equipmentLevel,
        EquipmentStatusType mainStatusType,
        int mainStatusValue,
        List<EquipmentOption> options)
        {
            SlotType = slotType;
            Grade = grade;
            EquipmentIndex = equipmentIndex;
            EquipmentLevel = equipmentLevel;
            MainStatusType = mainStatusType;
            MainStatusValue = mainStatusValue;

            if (options != null)
                Options = options;
        }
}