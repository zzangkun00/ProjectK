using ProjectN.Resource;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToggleScroll : PoolingMonoBehaviour
{
    [SerializeField]
    protected UIToggleGroup m_EventToggle = null;

    public AddEventToggle OnEventToggle { get { return m_EventToggle?.OnEventToggle; } }

    public UIEventToggleBase SelectData()
    {
        return m_EventToggle?.SelectData();
    }
    public Item_Toggle SelectItem()
    {
        return m_EventToggle?.SelectItem();
    }
    public void ClearData()
    {
        m_EventToggle?.ClearData();
    }
    public int GetItemCount()
    {
        if(m_EventToggle == null) { return 0; }

        return m_EventToggle.GetItemCount();
    }
    public UIEventToggleBase GetData(int nIDX)
    {
        return m_EventToggle?.GetData(nIDX);
    }
    public void SetData(string _menuID, bool isAlwaysOn, bool isInitSelect = true)
    {
        CommonFunction.SetValue(m_EventToggle, _menuID, isAlwaysOn, isInitSelect);

        SetContentSize();
    }
    public void SetData(List<UIEventToggleData> _DataList, bool isAlwaysOn, bool isInitSelect = true)
    {
        CommonFunction.SetValue(m_EventToggle, _DataList, isAlwaysOn, isInitSelect);

        SetContentSize();
    }
    public void SetData<T>(List<T> _DataList, bool isAlwaysOn, bool isInitSelect = true) where T : UIToggleData
    {
        if (m_EventToggle == null) { return; }

        m_EventToggle.SetData(_DataList, isAlwaysOn, isInitSelect);

        SetContentSize();
    }
    public void SetContentSize()
    {
        if (m_EventToggle == null) { return; }

        ScrollRect _scroll = GetComponent<ScrollRect>();
        if (_scroll == null) { return; }
        if (_scroll.content == null) { return; }
        if (_scroll.viewport == null) { return; }

        int nTabCount = m_EventToggle.GetItemCount();
        Vector2 size = m_EventToggle.GetItemSize();

        LayoutType _layoutType = m_EventToggle.GetLayoutType();

        float TotalContentSize = 0f;

        switch (_layoutType)
        {
            case LayoutType.Horizontal:
                {
                    if (m_EventToggle.GetComponent<HorizontalLayoutGroup>() is HorizontalLayoutGroup _hor)
                    {
                        TotalContentSize = (size.x * nTabCount) + (nTabCount * _hor.spacing) - _hor.spacing;
                    }

                    _scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _scroll.viewport.rect.height);
                    _scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TotalContentSize);

                    _scroll.movementType = _scroll.viewport.rect.width >= TotalContentSize ? ScrollRect.MovementType.Clamped : ScrollRect.MovementType.Elastic;

                }
                break;
            case LayoutType.Vertical:
                {
                    if (m_EventToggle.GetComponent<VerticalLayoutGroup>() is VerticalLayoutGroup _ver)
                    {
                        TotalContentSize = (size.y * nTabCount) + (nTabCount * _ver.spacing) - _ver.spacing;
                    }

                    _scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _scroll.viewport.rect.width);
                    _scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, TotalContentSize);

                    _scroll.movementType = _scroll.viewport.rect.height >= TotalContentSize ? ScrollRect.MovementType.Clamped : ScrollRect.MovementType.Elastic;
                }
                break;
        }
    }
    public void OnSelect(int nType)
    {
        if (m_EventToggle == null) { return; }

        m_EventToggle.OnSelect(nType);
    }
}