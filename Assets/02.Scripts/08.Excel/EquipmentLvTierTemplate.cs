using System;
using UnityEngine;

public class EquipmentLvTierTemplate
{
    public int Index;

    public float Common, Magic, Rare, Heroic, Legendary, Unique, Mythic, Ancient, Abyssal, Genesis;

    public EquipmentLvTierTemplate() { }

    public EquipmentLvTierTemplate(string[] listValue)
    {
        SetUp(listValue);
    }

    public void SetUp(string[] listValue)
    {
        ushort wCount = 0;

        Index = Convert.ToInt32(listValue[wCount++]);

        Common = Convert.ToSingle(listValue[wCount++]);
        Magic = Convert.ToSingle(listValue[wCount++]);
        Rare = Convert.ToSingle(listValue[wCount++]);
        Heroic = Convert.ToSingle(listValue[wCount++]);
        Legendary = Convert.ToSingle(listValue[wCount++]);
        Unique = Convert.ToSingle(listValue[wCount++]);
        Mythic = Convert.ToSingle(listValue[wCount++]);
        Ancient = Convert.ToSingle(listValue[wCount++]);
        Abyssal = Convert.ToSingle(listValue[wCount++]);
        Genesis = Convert.ToSingle(listValue[wCount++]);
    }
}
