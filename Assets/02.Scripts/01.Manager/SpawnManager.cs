using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnData
    {
        public Transform spawnPoint;
        public PathRoute pathRoute;
        public int spawnPointIndex;
    }

    [SerializeField] private Transform monsterParent;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private SpawnData[] spawnDatas;

    private void Start()
    {
        SpawnAll();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SpawnAll();
        }
    }

    public void SpawnAll()
    {
        if (monsterPrefab == null || spawnDatas == null) return;

        for (int i = 0; i < spawnDatas.Length; i++)
        {
            if (spawnDatas[i].spawnPoint == null || spawnDatas[i].pathRoute == null)
                continue;

            GameObject monster = Instantiate(monsterPrefab, spawnDatas[i].spawnPoint.position, Quaternion.identity);
            monster.transform.parent = monsterParent;
            MonsterController controller = monster.GetComponent<MonsterController>();
            if (controller != null)
            {
                controller.Init(spawnDatas[i].pathRoute, spawnDatas[i].spawnPointIndex);
            }
        }
    }
}
