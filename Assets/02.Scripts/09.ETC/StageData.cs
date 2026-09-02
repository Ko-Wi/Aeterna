using System.Collections.Generic;
using UnityEngine;
public enum StageTier
{
    Normal,     // 일반 단계
    Heroic,     // 영웅 단계
    Demigod,    // 반신 단계
    Titan       // 타이탄 단계
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("스테이지 등급")]

    // 이 데이터가 Normal, Heroic, Demigod, Titan 중 어디에 사용되는지 설정
    [SerializeField] private StageTier stageTier;

    [Header("일반 몬스터")]

    // 해당 등급에서 순환해서 사용할 일반 몬스터 프리팹 목록
    [SerializeField] private List<GameObject> monsterList = new List<GameObject>();

    [Header("보스 몬스터")]

    // 해당 등급에서 순환해서 사용할 보스 프리팹 목록
    [SerializeField] private List<GameObject> bossList = new List<GameObject>();


    // SpawnManager에서 이 데이터의 스테이지 등급을 확인
    public StageTier Tier => stageTier;

    // SpawnManager에서 일반 몬스터 목록을 읽기 위해 사용
    public List<GameObject> MonsterList => monsterList;

    // SpawnManager에서 보스 목록을 읽기 위해 사용
    public List<GameObject> BossList => bossList;
}
