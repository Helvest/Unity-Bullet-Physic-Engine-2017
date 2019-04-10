using System.Collections.Generic;
using PhysicWorldEnums;
using UnityEngine;

//is a singleton
public class WorldsManager : MonoBehaviour
{
	public static WorldsManager Instance { get; private set; }

	public static int ActualWorldID = 0;

	public bool autoCreateWorld = true;

	[SerializeField]
	private PhysicWorldParameters _physicWorldParameters;
	public static PhysicWorldParameters PhysicWorldParameters
	{
		get { return Instance._physicWorldParameters; }
	}

	private Dictionary<int, WorldController> worldControllerList = new Dictionary<int, WorldController>();
	public static WorldController WorldController
	{
		get
		{
			if (Instance.worldControllerList.Count > 0)
			{
				return Instance.worldControllerList[ActualWorldID];
			}

			return null;
		}
	}

	public static WorldController GetWorldController(int ID)
	{
		if (Instance.worldControllerList.ContainsKey(ID))
		{
			return Instance.worldControllerList[ID];
		}

		return null;
	}

	public WorldController GetOrCreateWorldController(int ID)
	{
		if (!worldControllerList.ContainsKey(ID))
		{
			worldControllerList.Add(ID, new WorldController());
		}

		return worldControllerList[ID];
	}

	public void OnDrawGizmos()
	{
		if (Instance && WorldController != null && WorldController.physicWorldParameters.debug)
		{
			WorldController.World.DebugDrawWorld();
		}
	}

	private void Awake()
	{
		Instance = this;

		if (_physicWorldParameters.worldType == WorldType.SoftBodyAndRigidBody
			&& _physicWorldParameters.collisionType == CollisionConfType.DefaultDynamicsWorldCollisionConf)
		{
			Debug.LogError("For World Type = SoftBodyAndRigidBody collisionType must be collisionType=SoftBodyRigidBodyCollisionConf. Switching");
			//m_collisionType = PhysicWorldEnums.CollisionConfType.SoftBodyRigidBodyCollisionConf;
			return;
		}

		Time.fixedDeltaTime = _physicWorldParameters.fixedDeltaTime;

		if (autoCreateWorld)
		{
			//Create default Physic World
			ActualWorldID = 0;
			worldControllerList.Add(ActualWorldID, new WorldController());
		}
	}

	private void OnDestroy()
	{
		foreach (WorldController world in worldControllerList.Values)
		{
			world.Dispose();
		}

		worldControllerList.Clear();
	}

	private void FixedUpdate()
	{
		foreach (WorldController world in worldControllerList.Values)
		{
			world.StepSimulation();
		}
	}
}
