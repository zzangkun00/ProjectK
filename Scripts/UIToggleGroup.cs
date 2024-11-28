using UnityEngine;
using UnityEngine.Events;
using ProjectN.Resource;
using System.Collections.Generic;
using Global;

public class AddEventToggle : UnityEvent<int>
{
    public AddEventToggle()
    {

    }
}

public class UIToggleGroup : MonoBehaviour
{
    [SerializeField]
    LayoutType mLayoutType = LayoutType.Max;
    [SerializeField]
    Item_Toggle m_EventToggle = null;

    private int m_Select = -1;

    private AddEventToggle onEventToggle = new AddEventToggle();
    public AddEventToggle OnEventToggle { get { return onEventToggle; } }
    
    private List<Item_Toggle> m_ItemList = new List<Item_Toggle>();

    public LayoutType GetLayoutType() { return mLayoutType; }

    public void SetData(string _menuID, bool isAlwaysOn, bool isInitSelect = true)
    {
        ClearData();

        if(m_EventToggle == null) { return; }

        if (string.IsNullOrEmpty(_menuID) == true) { return; }

        UIMenuRecord _record = UIMenuTable.Instance.Find(_menuID);
        if (_record == null) { return; }

        for (int i = 0; i < _record.ListSubMenu.Count; i++)
        {
            UIMenuRecord tableChildData = UIMenuTable.Instance.Find(_record.ListSubMenu[i]);
            if (tableChildData == null) { return; }

            UIEventToggleData _data = new UIEventToggleData();
            _data.mIDX = i;
            _data.m_Record = tableChildData;

            GameObject _gob = ResourceManager.Instance.PopPrefabObject(m_EventToggle.gameObject);
            if(_gob == null) { return; }

            Item_Toggle _item = _gob.GetComponent<Item_Toggle>();
            if(_item == null)
            {
                ResourceManager.Instance.PushPrefabObject(_item);
                return; 
            }

            _item.transform.SetParent(transform, false);
            
            _item.InitData(this, _data, isAlwaysOn);
            m_ItemList.Add(_item);            
        }

        if (isInitSelect && _record.ListSubMenu.Count > 0)
            OnSelect(0);
    }
    public void SetData(List<UIEventToggleData> _DataList, bool isAlwaysOn, bool isInitSelect = true)
    {
        ClearData();

        if (m_EventToggle == null) { return; }

        for (int i = 0; i < _DataList.Count; i++)
        {
            if (_DataList[i] == null) { continue; }

            GameObject _gob = ResourceManager.Instance.PopPrefabObject(m_EventToggle.gameObject);
            if (_gob == null) { return; }

            Item_Toggle _item = _gob.GetComponent<Item_Toggle>();
            if (_item == null)
            {
                ResourceManager.Instance.PushPrefabObject(_item);
                return;
            }

            _item.transform.SetParent(transform, false);

            _item.InitData(this, _DataList[i], isAlwaysOn);
            m_ItemList.Add(_item);

            if (isInitSelect && _DataList.Count > 0)
            {
                OnSelect(0);
            }
        }
    }
    public void SetData<T>(List<T> _DataList, bool isAlwaysOn, bool isInitSelect = true) where T : UIEventToggleBase
    {
        if (m_EventToggle == null) { return; }

        for (int i = 0; i < _DataList.Count; i++)
        {
            if (_DataList[i] == null) { continue; }

            GameObject _gob = ResourceManager.Instance.PopPrefabObject(m_EventToggle.gameObject);
            if (_gob == null) { return; }

            Item_Toggle _item = _gob.GetComponent<Item_Toggle>();
            if (_item == null)
            {
                ResourceManager.Instance.PushPrefabObject(_item);
                return;
            }

            _item.transform.SetParent(transform, false);

            _item.InitData(this, _DataList[i], isAlwaysOn);
            m_ItemList.Add(_item);

            if (isInitSelect && _DataList.Count > 0)
            {
                OnSelect(0);
            }
        }
    }
    public void ClearData()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform _transform = transform.GetChild(i);
            ResourceManager.Instance.PushPrefabObject(_transform);
        }

        m_ItemList.Clear();

        m_Select = -1;
    }
    public void ClearEventHaneler()
    {
        OnEventToggle.RemoveAllListeners();
    }
    public void OnSelect(int nType)
    {
        if (OnEventToggle != null)
        {
            m_Select = nType;
            OnEventToggle.Invoke(nType);
        }
    }
    public int GetItemCount()
    {
        return m_ItemList.Count;
    }
    public Vector2 GetItemSize()
    {
        if(m_EventToggle == null) { return Vector2.zero; }


        RectTransform _rt = m_EventToggle.GetComponent<RectTransform>();
        return _rt.sizeDelta;
    }
    public Item_Toggle SelectItem()
    {
        if (m_Select < 0 || m_Select >= m_ItemList.Count) { return null; }

        return m_ItemList[m_Select];        
    }
    public Item_Toggle GetItem(int nIDX)
    {
        if(nIDX < 0 || nIDX >= m_ItemList.Count) { return null; }

        return m_ItemList[nIDX];
    }
    public UIEventToggleBase SelectData()
    {
        Item_Toggle _selToggle = SelectItem();
        if(_selToggle == null) { return null; }

        return _selToggle.GetData();
    }
    public UIEventToggleBase GetData(int nIDX)
    {
        if (nIDX < 0 || nIDX >= m_ItemList.Count) { return null; }

        if(m_ItemList[nIDX] == null) { return null; }

        return m_ItemList[nIDX].GetData();
    }
    public void RefreshUI()
    {
        for(int i = 0; i < m_ItemList.Count; ++i)
        {
            if (m_ItemList[i] == null) { continue; }

            m_ItemList[i].RefreshUI();
        }        
    }
}
