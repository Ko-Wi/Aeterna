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
        if(Input.GetKeyDown(KeyCode.R))
        {
            RefreshEquipmentGradeProbabilities();
        }
    }
    /// <summary>
    /// 현재 myChar.ForgeLevel에 맞는 장비 등급 확률을 리스트에 갱신합니다.
    /// Inspector에서 확률을 눈으로 확인하기 위한 용도입니다.
    /// </summary>
    public void RefreshEquipmentGradeProbabilities()
    {
        if (myChar == null)
            myChar = MyObject.MyChar;

        var template = myChar.UpgradeDataMgr.GetTemplate(myChar.ForgeLevel);
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
        float randomValue = UnityEngine.Random.Range(0f, 100f);
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

    public int[] GetIndexCountsBySlot(EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Wand:
                return new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };

            case EquipmentSlotType.Staff:
                return new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };

            case EquipmentSlotType.Chest:
                return new int[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };

            case EquipmentSlotType.Helmet:
                return new int[] { 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };

            case EquipmentSlotType.Pants:
                return new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };

            case EquipmentSlotType.Amulet:
                return new int[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };

            case EquipmentSlotType.Ring:
                return new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };

            case EquipmentSlotType.Boots:
                return new int[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };

            case EquipmentSlotType.Belt:
                return new int[] { 3, 3, 3, 3, 2, 3, 3, 3, 3, 3 };

            case EquipmentSlotType.Shield:
                return new int[] { 1, 1, 1, 2, 2, 2, 2, 2, 2, 2 };

            default:
                Debug.LogWarning($"정의되지 않은 장비 슬롯입니다: {slotType}");
                return new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }
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

    Chest,
    Helmet,
    Pants,
    Amulet,
    Ring,
    Boots,
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