using System;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    //여기는 기본맵 관련 변수들
    [Header("맵별 필드 오브젝트")]
    [SerializeField] private GameObject[] Field;

    [Header("맵별 Road_Map 이미지")]
    [SerializeField] private Sprite[] Road_Map_Sprite;

    [Header("맵별 장식 오브젝트 묶음")]
    [SerializeField] private GameObject[] Map_Objects;

    [Header("이미지를 변경할 Road_Map")]
    [SerializeField] private SpriteRenderer roadMapRenderer;

    [Header("몇 스테이지마다 맵을 변경할지")]
    [SerializeField, Min(1)] private int stagesPerMap = 5;

    private MyObject myChar;

    private int lastStage = -1;
    private int currentMapIndex = -1;

    public int CurrentMapIndex => currentMapIndex;

    //던전 및 일반맵전체를 변경하기위한 함수
    [Header("일반 맵 부모 - MainField")]
    [SerializeField] private GameObject mainFieldRoot;

    [Header("던전 맵 부모 - Dungeon_Field")]
    [SerializeField] private GameObject dungeonFieldRoot;

    [Header("이동시킬 플레이어")]
    [SerializeField] private Transform player;

    [Header("던전 목록")]
    [SerializeField] private DungeonMap[] dungeonMaps;

    private bool isInDungeon;
    private int currentDungeonIndex = -1;

    private Vector3 returnPosition;
    private Quaternion returnRotation;
    private Sprite returnRoadSprite;

    public bool IsInDungeon => isInDungeon;
    public int CurrentDungeonIndex => currentDungeonIndex;
    private void Start()
    {
        myChar = MyObject.MyChar;
        RefreshStageMap();
    }

    private void Update()
    {
        RefreshStageMap();
    }

    private void RefreshStageMap()
    {
        if (myChar == null)
            return;

        int stage = Mathf.Max(1, myChar.CurrentStage);

        if (lastStage == stage)
            return;

        if (ApplyStage(stage))
            lastStage = stage;
    }

    public bool ApplyStage(int stage)
    {
        if (Field == null || Field.Length == 0)
            return false;

        int stageIndex = Mathf.Max(1, stage) - 1;

        // 모든 맵을 사용하면 첫 맵부터 반복
        int mapIndex =
            (stageIndex / Mathf.Max(1, stagesPerMap)) % Field.Length;

        return ApplyMap(mapIndex);
    }

    public bool ApplyMap(int mapIndex)
    {
        if (!ValidateMap(mapIndex))
            return false;

        if (currentMapIndex == mapIndex)
            return true;

        // 선택한 필드와 장식 묶음만 활성화
        SetActiveMap(Field, mapIndex);
        SetActiveMap(Map_Objects, mapIndex);

        // Road_Map 자체의 이미지만 변경
        roadMapRenderer.sprite = Road_Map_Sprite[mapIndex];

        currentMapIndex = mapIndex;
        return true;
    }

    private bool ValidateMap(int mapIndex)
    {
        if (Field == null || Field.Length == 0 ||
            Road_Map_Sprite == null || Map_Objects == null)
        {
            return false;
        }

        if (Field.Length != Road_Map_Sprite.Length ||
            Field.Length != Map_Objects.Length)
        {
            return false;
        }

        if (mapIndex < 0 || mapIndex >= Field.Length)
            return false;

        if (roadMapRenderer == null ||
            Field[mapIndex] == null ||
            Road_Map_Sprite[mapIndex] == null ||
            Map_Objects[mapIndex] == null)
        {
            return false;
        }

        return true;
    }

    private static void SetActiveMap(
        GameObject[] objects,
        int selectedIndex)
    {
        GameObject selectedObject = objects[selectedIndex];

        foreach (GameObject obj in objects)
        {
            if (obj == null)
                continue;

            bool active = obj == selectedObject;

            if (obj.activeSelf != active)
                obj.SetActive(active);
        }
    }
}

[System.Serializable]
public class DungeonMap
{
    public string dungeonName;

    [Header("Dungeon_Field 아래 해당 던전 오브젝트")]
    public GameObject mapRoot;

    [Header("플레이어 진입 위치")]
    public Transform entryPoint;

    [Header("던전에서 사용할 Road_Map 이미지")]
    public Sprite roadSprite;
}