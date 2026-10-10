using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(TutorialManager))]
public class TutorialManagerEditor : Editor
{
    private ReorderableList _steps;

    private void OnEnable()
    {
        _steps = new ReorderableList(serializedObject, serializedObject.FindProperty("Steps"), true, true, true, true);
        _steps.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "순차적으로 실행");
        _steps.elementHeightCallback = index =>
            EditorGUI.GetPropertyHeight(_steps.serializedProperty.GetArrayElementAtIndex(index), true) + 8f;
        _steps.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty element = _steps.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 4f;
            rect.height = EditorGUI.GetPropertyHeight(element, true);
            string name = element.FindPropertyRelative("Name")?.stringValue;
            string type = element.managedReferenceValue != null ? Label(element.managedReferenceValue.GetType()) : "비어 있는 단계";
            EditorGUI.PropertyField(rect, element, new GUIContent($"{index + 1}. {(string.IsNullOrWhiteSpace(name) ? type : name)}"), true);
        };
        _steps.onAddDropdownCallback = (rect, list) =>
        {
            var menu = new GenericMenu();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<TutorialStep>())
            {
                if (type.IsAbstract || !type.IsSerializable || type.GetConstructor(Type.EmptyTypes) == null) continue;
                Type selected = type;
                menu.AddItem(new GUIContent(Label(type)), false, () => AddStep(selected));
            }
            menu.DropDown(rect);
        };
        _steps.onRemoveCallback = list =>
        {
            Undo.RecordObject(target, "튜토리얼 단계 삭제");
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            serializedObject.ApplyModifiedProperties();
        };
    }

    private void AddStep(Type type)
    {
        serializedObject.Update();
        Undo.RecordObject(target, "튜토리얼 단계 추가");
        SerializedProperty steps = serializedObject.FindProperty("Steps");
        int index = steps.arraySize;
        steps.arraySize++;
        SerializedProperty element = steps.GetArrayElementAtIndex(index);
        var step = (TutorialStep)Activator.CreateInstance(type);
        step.Name = Label(type);
        element.managedReferenceValue = step;
        element.isExpanded = true;
        serializedObject.ApplyModifiedProperties();
        _steps.index = index;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        using (new EditorGUI.DisabledScope(Application.isPlaying))
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Context"), new GUIContent("공통 참조"), true);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("치환 문구 목록:\n{키}, {시간}, {현재횟수}, {목표횟수}, {레벨}, {골드}, {캐슬HP}, {최대캐슬HP}", MessageType.Info);
            _steps.DoLayoutList();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CompletionLog"), new GUIContent("완료 디버그 문구"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnCompleted"));
        }
        if (Application.isPlaying)
        {
            int index = ((TutorialManager)target).CurrentStepIndex;
            EditorGUILayout.LabelField("현재 실행 단계", index >= 0 ? (index + 1).ToString() : "없음");
            Repaint();
        }
        serializedObject.ApplyModifiedProperties();
    }

    private static string Label(Type type)
    {
        if (type == typeof(TutorialPrepareStep)) return "시작 준비";
        if (type == typeof(TutorialMovementStep)) return "이동 연습";
        if (type == typeof(TutorialPracticeStep)) return "공격 / 스킬 연습";
        if (type == typeof(TutorialMessageStep)) return "설명 / UI 강조";
        if (type == typeof(TutorialWaveStep)) return "몬스터 웨이브";
        if (type == typeof(TutorialGoldStep)) return "골드 배달";
        if (type == typeof(TutorialBuildStep)) return "터렛 건설 / 조합";
        if (type == typeof(TutorialSellStep)) return "터렛 판매";
        if (type == typeof(TutorialExitBuildStep)) return "건설 모드 종료";
        if (type == typeof(TutorialWalletCheckStep)) return "잔액 확인";
        return type.Name;
    }
}
