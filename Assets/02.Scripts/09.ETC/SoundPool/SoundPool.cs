using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathologicalGames;

public class SoundPool : MonoBehaviour
{
    public Transform SoundPrefab;
    public string soundName = "soundPrefab";
    public int soundAmount = 50;
    public SpawnPool soundPool;

    private static SoundPool _instance = null;

    private SoundPool()
    { }

    public static SoundPool Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Singleton == null");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        soundPool = PoolManager.Pools.Create(soundName);

        soundPool.group.parent = transform;

        soundPool.group.localPosition = Vector3.zero;
        soundPool.group.localRotation = Quaternion.identity;

        PrefabPool soundPrefabPool = new PrefabPool(SoundPrefab);
        soundPrefabPool.preloadAmount = 50;
        soundPrefabPool.cullDespawned = true;
        soundPrefabPool.cullAbove = 50;
        soundPrefabPool.cullDelay = 3;
        soundPrefabPool.limitInstances = true;
        soundPrefabPool.limitAmount = 50;
        soundPrefabPool.limitFIFO = true;

        soundPool.CreatePrefabPool(soundPrefabPool);
    }
}
