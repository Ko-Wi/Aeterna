using UnityEngine;

public class GameManager : MonoBehaviour
{
    /********************************** ½Ì ±Û Åæ *******************************************/

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

    private void Awake()
    {
        _instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myChar = MyObject.MyChar;
    }

    // Update is called once per frame
    void Update()
    {
        
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
