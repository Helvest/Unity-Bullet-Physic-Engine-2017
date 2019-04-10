using UnityEditor;

[CustomEditor(typeof(PhysicWorldParameters))]
public class PhysicWorldParametersEditor : Editor
{
	private float lastFixedTimeStep;
	private float lastFramerate;

	private float lastSubFixedTimeStep;
	private float lastSubFramerate;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		PhysicWorldParameters myScript = (PhysicWorldParameters)target;

		if (myScript.framerate != lastFramerate)
		{
			lastFramerate = myScript.framerate;

			lastFixedTimeStep = myScript.fixedDeltaTime = 1f / lastFramerate;
		}
		else if (myScript.fixedDeltaTime != lastFixedTimeStep)
		{
			lastFixedTimeStep = myScript.fixedDeltaTime;

			lastFramerate = myScript.framerate = 1f / lastFixedTimeStep;
		}

		if (myScript.subFramerate != lastSubFramerate)
		{
			lastSubFramerate = myScript.subFramerate;

			lastSubFixedTimeStep = myScript.subFixedTimeStep = 1f / lastSubFramerate;
		}
		else if (myScript.subFixedTimeStep != lastSubFixedTimeStep)
		{
			lastSubFixedTimeStep = myScript.subFixedTimeStep;

			lastSubFramerate = myScript.subFramerate = 1f / lastSubFixedTimeStep;
		}

	}
}