using Global;
using ProjectN.Resource;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class UIManager : Singleton<UIManager>
{
    private Dictionary<EUIID, UIBase> m_dicOpenUIList = new Dictionary<EUIID, UIBase>();
    private List<EUIID> m_OpenUITypeList = new List<EUIID>();

    public override void OnUnInstall()
    {
        AllCloseUI();

        base.OnUnInstall();
    }
    public override void OnSceneChange()
    {
        CloseUIAll(EUIID.UIMainHud, EUIID.UILoading, EUIID.UIFadeInOut);
        CloseObjectAll();
        CloseMessageBoxAll();

        base.OnSceneChange();
    }
    public void AllCloseUI()
    {
        CloseUIAll();
        CloseObjectAll();
        CloseMessageBoxAll();        
    }
    public void CloseUI(EUIID _type)
    {
        UIBase uiPanel = GetOpenUI(_type);
        if (uiPanel == null) { return; }

        if (m_dicOpenUIList.TryGetValue(_type, out UIBase _ui) == true)
        {
            _ui.RemoveHandlerEvent();
            _ui.Hide();
            m_dicOpenUIList.Remove(_type);
            m_OpenUITypeList.Remove(_type);
        }

        ResourceManager.SafeDelete(uiPanel.gameObject);

        return;
    }
    
    public T GetOrCreateOpenUI<T>(EUIID _type, Action<T> onAction = null) where T : UIBase
    {
        UIBase panelBase = GetOpenUI<UIBase>(_type);

        if (panelBase != null)
        {
            return panelBase as T;
        }

        return OpenUI(_type, onAction) as T;
    }   
    public UIBase OpenUI(EUIID _type, Action<UIBase> onAction = null)
    {
        return OpenUI<UIBase>(_type, onAction);
    }
    public T OpenUI<T>(EUIID _type, Action<T> onAction = null) where T : UIBase
    {
        if (IsOpenUI(_type)) { return null; }

        UIBase uiPanelBase = GetOpenUI(_type);

        if (uiPanelBase == null)
        {
            // PoolManager Check
            uiPanelBase = CreateUI(_type);
        }

        if (uiPanelBase == null)
        {
            CommonLogger.Error($"Fail OpenUI type : {_type}");
            return uiPanelBase as T;
        }

        if(uiPanelBase.m_Record == null) { return null; }

        CheckClearCanvas(uiPanelBase.m_Record);

        UIRootManager.Instance.RefreshSortOrder(uiPanelBase.m_RootType);

        m_dicOpenUIList.Add(_type, uiPanelBase);
        m_OpenUITypeList.Add(_type);

        uiPanelBase.ShowTopbar();
        uiPanelBase.ShowPlayInfo();

        uiPanelBase.SetHandlerEvent();

        if(onAction != null)
        {
            onAction(uiPanelBase as T);
        }

        uiPanelBase.Show();

        return uiPanelBase as T;

    }    
    public UIBase GetOpenUI(EUIID _type)
    {
        if (m_dicOpenUIList.TryGetValue(_type, out UIBase _panelBase) == true)
        {
            return _panelBase;
        }

        return null;
    }
    public T GetOpenUI<T>(EUIID _type) where T : UIBase
    {
        if (m_dicOpenUIList.TryGetValue(_type, out UIBase _panelBase) == true)
        {
            return (T)(object)_panelBase;
        }

        return default(T);
    }
    private UIBase CreateUI(EUIID _type)
    {
        UIRecord _record = UITable.Instance.Find(_type);
        if (_record == null) { return null; }

        UICanvasProperty transformAttatch = UIRootManager.Instance.GetTransformAttachRoot(_record.AttachRoot);
        if (transformAttatch == null)
            return null;

        UIBase uiPanelBase = ResourceManager.Instance.PopPrefabObjectComponent<UIBase>(_record.Path);
        if (uiPanelBase == null) { return null; }
        uiPanelBase.m_UIID = _type;
        uiPanelBase.m_RootType = _record.AttachRoot;
        uiPanelBase.SetTableData(_record);

        RectTransform uiRectTransform = uiPanelBase.GetComponent<RectTransform>();
        if (uiRectTransform == null) { return null; }

        Transform uiRoot = null;
        if(_record.SubAttachIndex >= 0)
        {
            if(GetOpenUI(_record.SubAttachID)is UIBase _uiParent)
            {
                uiRoot = _uiParent.GetSubRoot(_record.SubAttachIndex);
            }
        }

        if(transformAttatch.m_Canvas != null)
        {
            uiPanelBase.SetLayerInfo(transformAttatch.m_Canvas.sortingLayerID, 1);
        }

        if (uiRoot == null)
            uiRoot = transformAttatch.m_Root;

        uiRectTransform.SetParent(uiRoot, false);
        uiRectTransform.localPosition = Vector3.zero;
        uiRectTransform.localScale = Vector3.one;
        uiRectTransform.anchoredPosition = Vector2.zero;
        uiRectTransform.sizeDelta = Vector2.zero;

        return uiPanelBase;
    }
    public T CreateUI<T>(string strID) where T : UIBase
    {
        if (Enum.TryParse<EUIID>(strID, out EUIID _EUIID) == false) { return null; }

        UIRecord _record = UITable.Instance.Find(_EUIID);
        if (_record == null) { return null; }

        T _ui = ResourceManager.Instance.PopPrefabObjectComponent<T>(_record.Path);
        if (_ui == null) { return null; }

        RectTransform uiRectTransform = _ui.GetComponent<RectTransform>();
        if (uiRectTransform == null) { return null; }

        UICanvasProperty transformAttatch = UIRootManager.Instance.GetTransformAttachRoot(_record.AttachRoot);
        if (transformAttatch != null)
        {
            uiRectTransform.SetParent(transformAttatch.m_Root, false);
        }

        uiRectTransform.localPosition = Vector3.zero;
        uiRectTransform.localScale = Vector3.one;
        uiRectTransform.anchoredPosition = Vector2.zero;
        uiRectTransform.sizeDelta = Vector2.zero;

        return _ui;
    }
    public bool IsOpenUI(EUIID _type)
    {
        if (m_dicOpenUIList.TryGetValue(_type, out UIBase _panelBase) == true)
        {
            return true;
        }

        return false;
    }
#if UNITY_EDITOR
    public void Update()
    {
        
    }
#endif
    private void CheckClearCanvas(UIRecord _record)
    {
        if (_record == null) { return; }
        if (_record.ClearCavas == false) { return; }

        List<EUIID> _removeUIList = new List<EUIID>();

        foreach (var it in m_dicOpenUIList)
        {
            if (it.Value == null || it.Value.m_Record == null || it.Value.m_Record.CloseCanvas == false)
            {
                continue;
            }

            _removeUIList.Add(it.Key);
        }

        for (int i = 0; i < _removeUIList.Count; ++i)
        {
            CloseUI(_removeUIList[i]);
        }
    }
    public void CloseUIAll(params EUIID[] _uiID)
    {
        List<UIBase> listPopupBase = new List<UIBase>();

        foreach (var ui in m_dicOpenUIList.Values)
        {
            if (ui is UIBase)
            {
                listPopupBase.Add(ui);
            }
            else
            {
                Debug.LogWarning($"Attempted to add object of type {ui.GetType()} to List<UIBase>");
            }
        }

        for(int i = 0; i < _uiID.Length; ++i)
        {
            UIBase _ui = listPopupBase.Find(x => x != null && x.m_UIID == _uiID[i]);
            if(_ui != null)
            {
                listPopupBase.Remove(_ui);
            }
        }

        for (int i = 0; i < listPopupBase.Count; ++i)
        {
            if (listPopupBase[i] == null)
                continue;

            if (listPopupBase[i].IsBackkeyClose() == false)
                continue;

            listPopupBase[i].UIPopupClose();
        }
    }        
    public void SetAttchParent(UIBase _uiBase, ERootType _type)
    {
        if (_uiBase == null) { return; }

        UICanvasProperty transformAttatch = UIRootManager.Instance.GetTransformAttachRoot(_type);
        if (transformAttatch == null) { return; }

        RectTransform uiRectTransform = _uiBase.GetComponent<RectTransform>();
        if (uiRectTransform == null) { return; }

        uiRectTransform.SetParent(transformAttatch.m_Root, false);
        uiRectTransform.localPosition = Vector3.zero;
        uiRectTransform.localScale = Vector3.one;
        uiRectTransform.anchoredPosition = Vector2.zero;
        uiRectTransform.sizeDelta = Vector2.zero;
    }
    public void RefreshRedDot()
    {
        foreach (var it in m_dicOpenUIList)
        {
            if (it.Value == null) { continue; }

            it.Value.RefreshRedDot();
        }
    }
    #region Change Language 
    public void ChangeLanguage()
    {
        foreach (var it in m_dicOpenUIList)
        {
            if (it.Value == null) { continue; }

            it.Value.RefreshUI();
        }

        for(int i = 0; i < UIRootManager.Instance.ListCanvers.Count; ++i)
        {
            UICanvasProperty _canvas = UIRootManager.Instance.ListCanvers[i];
            if (_canvas == null)
                continue;

            UITableText[] arrayTableText = _canvas.GetComponentsInChildren<UITableText>(false);
            if (arrayTableText != null)
            {
                for (int j = 0; j < arrayTableText.Length; ++j)
                {
                    UITableText tableText = arrayTableText[j];
                    if (tableText == null)
                        continue;

                    tableText.UpdateTableString();
                }
            }
        }
    }
    #endregion
}