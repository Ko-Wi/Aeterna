using System;
using UnityEngine;

public class EquipmentLvTierTemplate
{
    public int Index;

    public int Common, Magic, Rare, Heroic, Legendary, Unique, Mythic, Ancient, Abyssal, Genesis;

    public EquipmentLvTierTemplate() { }

    public EquipmentLvTierTemplate(string[] listValue)
    {
        SetUp(listValue);
    }

    public void SetUp(string[] listValue)
    {
        ushort wCount = 0;

        Index = Convert.ToInt32(listValue[wCount++]);

        Common = Convert.ToInt32(listValue[wCount++]);
        Magic = Convert.ToInt32(listValue[wCount++]);
        Rare = Convert.ToInt32(listValue[wCount++]);
        Heroic = Convert.ToInt32(listValue[wCount++]);
        Legendary = Convert.ToInt32(listValue[wCount++]);
        Unique = Convert.ToInt32(listValue[wCount++]);
        Mythic = Convert.ToInt32(listValue[wCount++]);
        Ancient = Convert.ToInt32(listValue[wCount++]);
        Abyssal = Convert.ToInt32(listValue[wCount++]);
        Genesis = Convert.ToInt32(listValue[wCount++]);
    }
}
