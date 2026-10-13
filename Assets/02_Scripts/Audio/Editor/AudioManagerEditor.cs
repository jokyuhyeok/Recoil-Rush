#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Audio Data Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Sort Audio Data"))
        {
            ModifyAudioData(false);
        }

        if (GUILayout.Button("Add Missing Enum Types"))
        {
            AddMissingAudioData();
        }

        if (GUILayout.Button("Validate Audio Data"))
        {
            ValidateAudioData();
        }
    }

    private void ModifyAudioData(bool unused)
    {
        serializedObject.Update();

        SerializedProperty bgmDatas = serializedObject.FindProperty("_bgmDatas");
        SerializedProperty sfxDatas = serializedObject.FindProperty("_sfxDatas");

        SortArray(bgmDatas);
        SortArray(sfxDatas);

        serializedObject.ApplyModifiedProperties();

        Debug.Log("Audio Data 정렬 완료");
    }

    private void AddMissingAudioData()
    {
        serializedObject.Update();

        SerializedProperty bgmDatas = serializedObject.FindProperty("_bgmDatas");
        SerializedProperty sfxDatas = serializedObject.FindProperty("_sfxDatas");

        int bgmAdded = AddMissingTypes(bgmDatas, typeof(BackgroundMusicType));
        int sfxAdded = AddMissingTypes(sfxDatas, typeof(SoundEffectType));

        serializedObject.ApplyModifiedProperties();

        Debug.Log($"Audio Data 추가 완료: BGM {bgmAdded}개, SFX {sfxAdded}개");
    }

    private void SortArray(SerializedProperty array)
    {
        for (int i = 1; i < array.arraySize; i++)
        {
            int j = i;

            while (j > 0)
            {
                int previousType = array.GetArrayElementAtIndex(j - 1)
                    .FindPropertyRelative("Type").enumValueIndex;

                int currentType = array.GetArrayElementAtIndex(j)
                    .FindPropertyRelative("Type").enumValueIndex;

                if (previousType <= currentType)
                    break;

                array.MoveArrayElement(j, j - 1);
                j--;
            }
        }
    }

    private int AddMissingTypes(SerializedProperty array, Type enumType)
    {
        HashSet<int> existingTypes = new HashSet<int>();

        for (int i = 0; i < array.arraySize; i++)
        {
            SerializedProperty type = array.GetArrayElementAtIndex(i).FindPropertyRelative("Type");
            existingTypes.Add(type.intValue);
        }

        int addedCount = 0;

        foreach (object value in Enum.GetValues(enumType))
        {
            int enumValue = Convert.ToInt32(value);

            if (existingTypes.Contains(enumValue))
                continue;

            int index = array.arraySize;
            array.arraySize++;

            SerializedProperty element = array.GetArrayElementAtIndex(index);

            element.FindPropertyRelative("Type").intValue = enumValue;
            element.FindPropertyRelative("Clip").objectReferenceValue = null;

            existingTypes.Add(enumValue);
            addedCount++;
        }

        return addedCount;
    }

    private void ValidateAudioData()
    {
        serializedObject.Update();

        StringBuilder report = new StringBuilder();
        int issueCount = 0;

        issueCount += ValidateArray(
            serializedObject.FindProperty("_bgmDatas"),
            typeof(BackgroundMusicType),
            "BGM",
            report
        );

        issueCount += ValidateArray(
            serializedObject.FindProperty("_sfxDatas"),
            typeof(SoundEffectType),
            "SFX",
            report
        );

        if (issueCount == 0)
        {
            Debug.Log("Audio Data 검사 완료: 문제 없음");
        }
        else
        {
            Debug.LogWarning($"Audio Data 검사 완료: {issueCount}개 문제 발견\n{report}");
        }
    }

    private int ValidateArray(SerializedProperty array, Type enumType, string label, StringBuilder report)
    {
        HashSet<int> existingTypes = new HashSet<int>();
        int issueCount = 0;

        for (int i = 0; i < array.arraySize; i++)
        {
            SerializedProperty element = array.GetArrayElementAtIndex(i);
            int enumValue = element.FindPropertyRelative("Type").intValue;

            string typeName = Enum.GetName(enumType, enumValue);

            if (typeName == null)
            {
                report.AppendLine($"[{label}] 잘못된 enum 값: {enumValue} (배열 Index {i})");
                issueCount++;
                continue;
            }

            if (!existingTypes.Add(enumValue))
            {
                report.AppendLine($"[{label} {enumValue}] 중복: {typeName}");
                issueCount++;
            }

            if (element.FindPropertyRelative("Clip").objectReferenceValue == null)
            {
                report.AppendLine($"[{label} {enumValue}] Clip 미할당: {typeName}");
                issueCount++;
            }
        }

        foreach (object value in Enum.GetValues(enumType))
        {
            int enumValue = Convert.ToInt32(value);

            if (!existingTypes.Contains(enumValue))
            {
                report.AppendLine($"[{label} {enumValue}] 미등록: {value}");
                issueCount++;
            }
        }

        return issueCount;
    }
}
#endif
