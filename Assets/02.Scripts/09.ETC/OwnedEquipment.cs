using System;
using System.Collections.Generic;

[Serializable]
public class OwnedEquipment
{
    public EquipmentSlotType SlotType;
    public EquipmentGrade Grade;
    public int EquipmentIndex;

    public EquipmentStatusType MainStatusType;
    public int MainStatusValue;

    public List<EquipmentOption> Options = new List<EquipmentOption>();

    public OwnedEquipment(
        EquipmentSlotType slotType,
        EquipmentGrade grade,
        int equipmentIndex,
        EquipmentStatusType mainStatusType,
        int mainStatusValue,
        List<EquipmentOption> options)
        {
            SlotType = slotType;
            Grade = grade;
            EquipmentIndex = equipmentIndex;
            MainStatusType = mainStatusType;
            MainStatusValue = mainStatusValue;

            if (options != null)
                Options = options;
        }
}