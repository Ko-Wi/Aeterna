using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /********************************** 싱 글 톤 *******************************************/

    private static GameManager _instance = null;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GameManager Singleton == null");
            }
            return _instance;
        }
    }

    /*************************************************************************************/
    MyObject myChar;

    [Header("현재 단조 레벨 기준 장비 등급 확률")]
    public List<EquipmentGradeProbability> EquipmentGradeProbabilities = new List<EquipmentGradeProbability>();


    private void Awake()
    {
        _instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myChar = MyObject.MyChar;

        RefreshEquipmentGradeProbabilities();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// 현재 myChar.ForgeLevel에 맞는 장비 등급 확률을 리스트에 갱신합니다.
    /// Inspector에서 확률을 눈으로 확인하기 위한 용도입니다.
    /// </summary>
    public void RefreshEquipmentGradeProbabilities()
    {
        if (myChar == null)
            myChar = MyObject.MyChar;

        UpgradeTemplate template = myChar.UpgradeDataMgr.GetTemplate(myChar.ForgeLevel);
        if (template == null)
        {
            Debug.Log(template.Unique);
            return;
        }

        EquipmentGradeProbabilities.Clear();

        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Common, template.Common));
        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Magic, template.Magic));
        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Rare, template.Rare));
        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Heroic, template.Heroic));
        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Legendary, template.Legendary));
        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Unique, template.Unique));
        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Mythic, template.Mythic));
        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Ancient, template.Ancient));
        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Abyssal, template.Abyssal));
        EquipmentGradeProbabilities.Add(new EquipmentGradeProbability(EquipmentGrade.Genesis, template.Genesis));
    }

    /// <summary>
    /// 현재 EquipmentGradeProbabilities 리스트 기준으로 등급을 랜덤으로 뽑습니다.
    /// </summary>
    public EquipmentGrade GetRandomEquipmentGrade()
    {
        if (EquipmentGradeProbabilities == null || EquipmentGradeProbabilities.Count == 0)
        {
            RefreshEquipmentGradeProbabilities();
            Debug.Log(111);
        }

        float totalRate = 0f;

        for (int i = 0; i < EquipmentGradeProbabilities.Count; i++)
        {
            totalRate += EquipmentGradeProbabilities[i].Probability;
        }

        if (totalRate <= 0f)
        {
            Debug.LogWarning("장비 등급 확률 총합이 0 이하입니다. 기본 등급 Common 반환");
            return EquipmentGrade.Common;
        }

        float randomValue = UnityEngine.Random.Range(0f, totalRate);
        float cumulative = 0f;

        for (int i = 0; i < EquipmentGradeProbabilities.Count; i++)
        {
            cumulative += EquipmentGradeProbabilities[i].Probability;

            if (randomValue <= cumulative)
            {
                return EquipmentGradeProbabilities[i].Grade;
            }
        }

        return EquipmentGrade.Common;
    }
}
[Serializable]
public class EquipmentGradeProbability
{
    public EquipmentGrade Grade;
    public float Probability;

    public EquipmentGradeProbability(EquipmentGrade grade, float probability)
    {
        Grade = grade;
        Probability = probability;
    }
}

public enum EquipmentSlotType
{
    Wand,
    Staff,

    Helmet,
    Chest,
    Glove,
    Boots,
    Ring,
    Earring,
    Belt,
    Shield
}
public enum EquipmentGrade
{
    Common,     // 일반
    Magic,      // 마법
    Rare,       // 희귀
    Heroic,     // 영웅
    Legendary,  // 전설
    Unique,     // 유니크
    Mythic,     // 신화
    Ancient,    // 고대
    Abyssal,    // 심연
    Genesis     // 창세
}