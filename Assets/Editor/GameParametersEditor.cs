using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(GameParameters), true)]
public class GameParametersEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GameParameters parameters = (GameParameters)target;

        EditorGUILayout.LabelField($"Name: {parameters.GetParametersName()}", EditorStyles.boldLabel);

        FieldInfo[] fields = parameters.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                                .Where(field => field.IsFamily != true).ToArray();

        foreach (FieldInfo field in fields)
        {
            if (System.Attribute.IsDefined(field, typeof(HideInInspector), false))
                continue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(40f));

            bool hasHeader = System.Attribute.IsDefined(field, typeof(HeaderAttribute), false);

            if (hasHeader)
                GUILayout.FlexibleSpace();

            if (GUILayout.Button(parameters.IsShowsField(field.Name) ? "-" : "+",
                                 GUILayout.Width(20f),
                                 GUILayout.Height(20f)))
            {
                parameters.ToggleShowField(field.Name);
                EditorUtility.SetDirty(parameters);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(16);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name), true);
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
