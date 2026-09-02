using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    MyObject myChar;
    public AudioClip Title_BGM;
    public AudioClip[] BasicMap_BGM;
    public AudioClip GoldDungeon_BGM;
    public AudioClip DiaDungeon_BGM;
    public AudioClip PropertyDungeon_BGM;
    public AudioClip BossDungeon_BGM;
    //public AudioClip[] BGM;
    public AudioClip[] SFX;

    public int BGM_Num;

    public AudioSource audioSource;
    SoundPool soundPool;

    private static SoundManager _instance = null;

    public static SoundManager Instance
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
        myChar = MyObject.MyChar;
        _instance = this;

        soundPool = GetComponent<SoundPool>();
        audioSource = GetComponent<AudioSource>();

    }

    void Update()
    {
        //audioSource.pitch = Time.timeScale;
        //if (!myChar.BGM)
        //{
        //    if (!audioSource.isPlaying)
        //    {
        //        audioSource.enabled = true;
        //        if (SceneManager.GetActiveScene().name == "TitleScene")
        //        {
        //            audioSource.clip = Title_BGM;
        //            audioSource.Play();
        //        }
        //        else
        //        {
        //            PlayBGM();

        //        }
        //    }
        //    if (SceneManager.GetActiveScene().name != "TitleScene")
        //    {
        //        BGMCheck();
        //    }

        //}
        //else if (myChar.BGM)
        //{
        //    MuteBGM();
        //}
    }
    public void PlayBGM()
    {
        //if (!myChar.isDungeon)
        //{
        //    audioSource.clip = BasicMap_BGM[0];
        //    //audioSource.Play();
        //}
        //else
        //{
        //    switch (myChar.CurrentDungeonType)
        //    {
        //        case DungeonType.None:
        //            break;
        //        case DungeonType.Gold:
        //            audioSource.clip = GoldDungeon_BGM;
        //            //audioSource.Play();
        //            break;
        //        case DungeonType.Dia:
        //            audioSource.clip = DiaDungeon_BGM;
        //            //audioSource.Play();
        //            break;
        //        case DungeonType.Property:
        //            audioSource.clip = PropertyDungeon_BGM;
        //            //audioSource.Play();
        //            break;
        //        case DungeonType.InfinitDamage:
        //            audioSource.clip = BossDungeon_BGM;
        //            //audioSource.Play();
        //            break;
        //    }
        //}
        audioSource.Play();
    }
    private void BGMCheck()
    {
        //if (!myChar.isDungeon)
        //{
        //    if (audioSource.clip != BasicMap_BGM[0])
        //    {
        //        audioSource.enabled = false;
        //    }
        //}
        //else
        //{
        //    switch (myChar.CurrentDungeonType)
        //    {
        //        case DungeonType.None:
        //            break;
        //        case DungeonType.Gold:
        //            if (audioSource.clip != GoldDungeon_BGM)
        //            {
        //                audioSource.enabled = false;
        //            }
        //            break;
        //        case DungeonType.Dia:
        //            if (audioSource.clip != DiaDungeon_BGM)
        //            {
        //                audioSource.enabled = false;
        //            }
        //            break;
        //        case DungeonType.Property:
        //            if (audioSource.clip != PropertyDungeon_BGM)
        //            {
        //                audioSource.enabled = false;
        //            }
        //            break;
        //        case DungeonType.InfinitDamage:
        //            if (audioSource.clip != BossDungeon_BGM)
        //            {
        //                audioSource.enabled = false;
        //            }
        //            break;
        //    }
        //}
    }
    public void MuteBGM()
    {
        audioSource.Stop();
    }

    public void PlaySfx(int _sfx, float _volume = 1)
    {
        // int count = SoundPool.Instance.soundAmount;
        if (!myChar.EffectSound)
        {
            Transform tempSfx = SoundPool.Instance.soundPool.Spawn(SoundPool.Instance.SoundPrefab);
            SFX sfx = tempSfx.GetComponent<SFX>();
            sfx.Play(SFX[_sfx], _volume);
        }

        // count--;
    }

    public void SoundSfx(AudioClip _audio, float _volume = 1)
    {
        if (!myChar.EffectSound)
        {
            Transform tempSfx = SoundPool.Instance.soundPool.Spawn(SoundPool.Instance.SoundPrefab);
            SFX sfx = tempSfx.GetComponent<SFX>();
            sfx.Play(_audio, _volume);
        }
    }
}
