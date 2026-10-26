using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EquipmentLvTierTemplateMgr : MUIBase<EquipmentLvTierTemplate>
{
    string TextResource;
    bool flagDataLoad;

    public EquipmentLvTierTemplateMgr()
    {
        flagDataLoad = false;
    }

    static public string ReadFile(string FileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(FileName);

        if (textAsset == null)
            return "";

        string data = textAsset.text;

        Resources.UnloadAsset(textAsset);

        return data;
    }

    public void OnDataLoad(string _textResource)
    {
        if (TextResource == "" && _textResource == "") return;

        TextResource = _textResource;

        string Data = ReadFile(_textResource);

        if (Data != "")
        {
            string[] list = Data.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int nCnt = 1; nCnt < list.Length; nCnt++)
            {
                string[] listDetail = list[nCnt].Split('\t');
                int Length = (int)(listDetail.Length - 1);
                listDetail[Length] = listDetail[Length].Trim('\r');
                if (listDetail.Length > 0)
                {
                    EquipmentLvTierTemplate pTemplate = new EquipmentLvTierTemplate(listDetail);
                    base.AddTItem(pTemplate);
                }
            }
        }
        flagDataLoad = true;
    }

    public EquipmentLvTierTemplate GetTemplate(int _Index)
    {
        List<EquipmentLvTierTemplate> listTemplate = GetTList();
        for (int nCnt = 0; nCnt < listTemplate.Count; ++nCnt)
        {
            if (listTemplate[nCnt].Index != _Index)
                continue;

            return listTemplate[nCnt];
        }
        return null;
    }
}