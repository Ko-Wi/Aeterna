using System;
using System.Collections.Generic;

[Serializable]
public class Equipment
{
    public EquipmentSlotType SlotType = EquipmentSlotType.None;          // 장비종류
    public EquipmentGrade Grade;                // 등급
    public int EquipmentIndex = -1;                  // 뽑힌 장비 Index
    public int EquipmentLevel = 0;                  // 장비 레벨

    public EquipmentStatusType MainStatusType;
    public double MainStatusValue;

    public List<EquipmentOption> Options = new List<EquipmentOption>();

    public Equipment()
    {
        SlotType = EquipmentSlotType.None;
        EquipmentIndex = -1;
        EquipmentLevel = 0;
    }

    public Equipment(EquipmentSlotType slotType, EquipmentGrade grade, int equipmentIndex,
        int equipmentLevel, EquipmentStatusType mainStatusType, double mainStatusValue, List<EquipmentOption> options)
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
    public bool IsValid()
    {
        // 장비 부위가 있고, 이미지 인덱스가 지정되어 있으며, 장비 레벨이 1 이상이면 유효한 장비로 판단합니다.
        return SlotType != EquipmentSlotType.None && EquipmentIndex >= 0 && EquipmentLevel > 0;
    }
}