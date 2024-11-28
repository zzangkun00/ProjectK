#if !UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Unity.VisualScripting;

[CustomEditor(typeof(UIToggleGroup), true)]
[CanEditMultipleObjects]
public class UIToggleGroupEditor : Editor
{
    SerializedProperty m_LayoutType;

    LayoutType mLayoutType = LayoutType.Max;
    protected void OnEnable()
    {
        m_LayoutType = serializedObject.FindProperty("mLayoutType");

        mLayoutType = (LayoutType)m_LayoutType.enumValueFlag;
    }

    public override void DrawPreview(Rect previewArea)
    {
        base.DrawPreview(previewArea);
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck(); // 변경 감지 시작

        //EditorGUILayout.PropertyField(m_LayoutType);

        if((LayoutType)m_LayoutType.enumValueFlag != mLayoutType)
        {
            mLayoutType = (LayoutType)m_LayoutType.enumValueFlag;
            ChangeLayout();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        base.OnInspectorGUI();
    }

    private void ChangeLayout()
    {
        var _verComponent = target.GetComponent<VerticalLayoutGroup>();
        if (_verComponent != null)
        {
            DestroyImmediate(_verComponent);
        }

        var _horComponent = target.GetComponent<HorizontalLayoutGroup>();
        if (_horComponent != null)
        {
            DestroyImmediate(_horComponent);
        }

        switch (mLayoutType)
        {
            case LayoutType.Horizontal:
                {
                    target.AddComponent<HorizontalLayoutGroup>();
                }
                break;
            case LayoutType.Vertical:
                {
                    target.AddComponent<VerticalLayoutGroup>();
                }
                break;
        }        
    }
}
#endif