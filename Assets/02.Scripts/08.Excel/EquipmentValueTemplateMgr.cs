using System.Collections.Generic;
using System.Text;
using UnityEngine;
public class EquipmentValueTemplateMgr : MUIBase<EquipmentValueTemplate>
{
    string TextResource;
    bool flagDataLoad;

    public EquipmentValueTemplateMgr()
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
        if (TextResource == _textResource && flagDataLoad) return;

        TextResource = _textResource;

        string Data = ReadFile(_textResource);

        if (Data == "") return;

        Clear();

        string[] list = Data.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int nCnt = 1; nCnt < list.Length; nCnt++)
        {
            string[] listDetail = list[nCnt].Split('\t');
            int Length = listDetail.Length - 1;
            listDetail[Length] = listDetail[Length].Trim('\r');

            EquipmentValueTemplate pTemplate = new EquipmentValueTemplate(listDetail);

            base.AddTItem(pTemplate);
        }

        flagDataLoad = true;
    }

    public EquipmentValueTemplate GetTemplate(string _Type)
    {
        List<EquipmentValueTemplate> listTemplate = GetTList();

        for (int nCnt = 0; nCnt < listTemplate.Count; nCnt++)
        {
            if (listTemplate[nCnt].Type != _Type)
                continue;

            return listTemplate[nCnt];
        }

        return null;
    }
}
