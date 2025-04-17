using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Collectible))] 
public class CollectibleEditor : Editor
{
    SerializedProperty typeProp;
    SerializedProperty coinValueProp;
    SerializedProperty stopDurationProp;
    SerializedProperty resumeDelayProp;

    private void OnEnable()
    {
        typeProp = serializedObject.FindProperty("type");
        coinValueProp = serializedObject.FindProperty("coinValue");
        stopDurationProp = serializedObject.FindProperty("stopDuration");
        resumeDelayProp = serializedObject.FindProperty("resumeDelay");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(typeProp);

        Collectible.CollectibleType currentType = (Collectible.CollectibleType)typeProp.enumValueIndex;

        EditorGUILayout.Space();

        switch (currentType)
        {
            case Collectible.CollectibleType.Coin:
                EditorGUILayout.LabelField("Coin Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(coinValueProp, new GUIContent("Coin Value"));
                break;

            case Collectible.CollectibleType.StopTrigger:
                EditorGUILayout.LabelField("Stop Trigger Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(stopDurationProp, new GUIContent("Stop Duration (s)")); 
                EditorGUILayout.PropertyField(resumeDelayProp, new GUIContent("Resume Delay (s)"));  
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}