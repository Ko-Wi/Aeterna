using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class MUIBase<T>
{
    //uint m_dwID = 0;
    List<T> m_BaseTlist = new List<T>();
    public MUIBase()
    {
        Clear();
        InitModule();
    }
    #region 내부 사용 함수 모음
    /// <summary>
    /// 여기는 내부에서만 사용되는 함수들만 모아둔다.
    /// </summary>
    void InitModule()
    {
        m_BaseTlist.Clear();
    }
    #endregion

    public virtual List<T> GetTList()
    {
        return m_BaseTlist;
    }
    public virtual void AddTItem(T obj)
    {
        //m_dwID += 1;
        m_BaseTlist.Add(obj);
    }
    //public virtual T GetTItem()
    //{
    //    T _TItem = default(T);
    //    return _TItem;
    //}
    public virtual void DelTItem(T item)
    {
        m_BaseTlist.Remove(item);
    }
    public virtual int GetCount()
    {
        return m_BaseTlist.Count;
    }
    public virtual void Clear()
    {
        m_BaseTlist.Clear();
    }
    //public virtual void OnUIEventResult(GameObject pGameObject)
    //{
    //    //Debug.Log("UIEvent Result Name:" + pGameObject.name);
    //}
}
public class MUIBase<T1, T2>
{
    //uint m_dwID = 0;
    Dictionary<T1, T2> m_mapBase;
    public MUIBase()
    {
        InitModule();
    }
    #region 내부 사용 함수 모음
    /// <summary>
    /// 여기는 내부에서만 사용되는 함수들만 모아둔다.
    /// </summary>
    void InitModule()
    {
        m_mapBase = new Dictionary<T1, T2>();
        m_mapBase.Clear();
    }
    #endregion

    public virtual Dictionary<T1, T2> GetTList()
    {
        return m_mapBase;
    }
    public virtual List<T2> DumpTList()
    {
        List<T2> listData = new List<T2>();
        foreach (KeyValuePair<T1, T2> listItem in m_mapBase)
        {
            listData.Add(listItem.Value);
        }
        return listData;
    }
    public virtual void AddTItem(T1 key, T2 Value)
    {
        //m_dwID += 1;
        if (m_mapBase.ContainsKey(key) == true)
        {
            m_mapBase.Remove(key);
        }
        m_mapBase.Add(key, Value);
    }
    public virtual T2 GetTItem(T1 key)
    {
        if (!m_mapBase.ContainsKey(key))
            return default(T2);

        foreach (KeyValuePair<T1, T2> listItem in m_mapBase)
        {
            if (listItem.Key.ToString() == key.ToString())
                return listItem.Value;
        }
        return default(T2);
    }
    public virtual void DelTItem(T1 key)
    {
        m_mapBase.Remove(key);
    }
    public virtual int GetCount()
    {
        return m_mapBase.Count;
    }
    public virtual void Clear()
    {
        m_mapBase.Clear();
    }
    public virtual bool IsTItem(T1 Key)
    {
        if (m_mapBase.ContainsKey(Key))
            return true;
        else
            return false;
    }
}
