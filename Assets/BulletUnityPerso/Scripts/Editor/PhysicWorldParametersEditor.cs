using UnityEditor;

[CustomEditor(typeof(PhysicWorldParameters))]
public class PhysicWorldParametersEditor : Editor
{
	private float lastFixedTimeStep;
	private float lastFramerate;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		PhysicWorldParameters myScript = (PhysicWorldParameters)target;

		if(myScript.framerate != lastFramerate)
		{
			lastFramerate = myScript.framerate;

			lastFixedTimeStep = myScript.fixedTimeStep = 1f / lastFramerate;
		}
		else if(myScript.fixedTimeStep != lastFixedTimeStep)
		{
			lastFixedTimeStep = myScript.fixedTimeStep;

			lastFramerate = myScript.framerate = 1f / lastFixedTimeStep;
		}

		/*if(GUILayout.Button("Build Object"))
		{
			myScript.BuildObject();
		}*/
	}
}