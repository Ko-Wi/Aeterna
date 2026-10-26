using System;
using System.Globalization;
using UnityEngine;

public class EquipmentValueTemplate
{
    public string Type;

    public double[] Values;

    public EquipmentValueTemplate() { }

    public EquipmentValueTemplate(string[] listValue)
    {
        SetUp(listValue);
    }

    public void SetUp(string[] listValue)
    {
        Type = listValue[0];

        Values = new double[listValue.Length - 1];

        for (int i = 0; i < Values.Length; i++)
        {
            Values[i] = Convert.ToDouble(listValue[i + 1]);
        }
    }
}
