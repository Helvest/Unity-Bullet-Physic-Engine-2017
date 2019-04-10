using System.Collections.Generic;
using BulletUnity;
using UnityEngine;

public class BallThrowTest : MonoBehaviour
{
	public BRigidBody ballRigidbody;
	public Vector3 ballThrowImpulse;
	public int numberOfSimulationSteps;
	public GameObject ballGhostPrefab;
	private bool simulationStarted = false;
	public int startFrame = 0;

	private List<Vector3> ballPositionsRealtime = new List<Vector3>();
	private List<Vector3> ballPositionsOfflineSim = new List<Vector3>();

	private void Awake()
	{
		Debug.Log("Awake " + Time.frameCount);
		startFrame = Time.frameCount;
		ballRigidbody.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (Time.frameCount - startFrame == 50 && !simulationStarted)
		{
			Debug.Log("Starting simulation.");
			ballRigidbody.gameObject.SetActive(true);
			WorldsManager.WorldController.AddRigidBody(ballRigidbody);
			simulationStarted = true;
			startFrame = (int)WorldsManager.WorldController.FrameCount;

			//first simulation ==============================
			ballPositionsOfflineSim = OfflineBallSimulation.SimulateBall(ballRigidbody, ballThrowImpulse, numberOfSimulationSteps, false);

			//Second simulation =====================
			ballRigidbody.AddImpulse(ballThrowImpulse);

			for (int i = 0; i < ballPositionsOfflineSim.Count; i++)
			{
				Instantiate(ballGhostPrefab, ballPositionsOfflineSim[i], Quaternion.identity);
			}
		}
		else if (simulationStarted && ballPositionsRealtime.Count < 500)
		{
			ballPositionsRealtime.Add(ballRigidbody.GetCollisionObject().WorldTransform.Origin.ToUnity());
		}

		if (ballPositionsRealtime.Count == 500)
		{
			//prevent this clause from executing again
			ballPositionsRealtime.Add(Vector3.zero);
		}
	}
}
