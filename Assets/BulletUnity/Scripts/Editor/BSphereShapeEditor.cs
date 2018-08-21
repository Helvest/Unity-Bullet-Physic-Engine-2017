using BulletUnity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(BSphereShape))]
public class BSphereShapeEditor : Editor
{
	BSphereShape script;

	void OnEnable()
	{
		script = (BSphereShape)target;
	}

	public override void OnInspectorGUI()
	{
		if (script.transform.localScale != Vector3.one)
		{
			EditorGUILayout.HelpBox("This shape doesn't support scale of the object.\nThe scale must be one", MessageType.Warning);
		}

		script.Radius = EditorGUILayout.FloatField("Radius", script.Radius);
		script.LocalScaling = EditorGUILayout.Vector3Field("Local Scaling", script.LocalScaling);
		if (GUI.changed)
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(script);
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			Repaint();
		}
	}
}