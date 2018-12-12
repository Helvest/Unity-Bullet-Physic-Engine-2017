using System.Collections.Generic;
using PhysicWorldEnums;
using UnityEngine;

//is a singleton
public class WorldsManager : MonoBehaviour
{
	public static WorldsManager Instance { get; private set; }

	public static int ActualWorldID = 0;

	[SerializeField]
	private PhysicWorldParameters _physicWorldParameters;
	public static PhysicWorldParameters PhysicWorldParameters
	{
		get { return Instance._physicWorldParameters; }
	}

	private Dictionary<int, WorldController> worldControllerList = new Dictionary<int, WorldController>();
	public WorldController WorldController
	{
		get
		{
			if(worldControllerList.Count > 0)
			{
				return worldControllerList[ActualWorldID];
			}

			return null;
		}
	}

	public WorldController GetWorldController(int ID)
	{
		return worldControllerList[ID];
	}

	public WorldController GetOrCreateWorldController(int ID)
	{
		if(!worldControllerList.ContainsKey(ID))
		{
			worldControllerList.Add(ID, new WorldController());
		}

		return worldControllerList[ID];
	}

	public void OnDrawGizmos()
	{
		if(WorldController != null && WorldController.physicWorldParameters.debug)
		{
			WorldController.World.DebugDrawWorld();
		}
	}

	private void Awake()
	{
		Instance = this;

		if(_physicWorldParameters.worldType == WorldType.SoftBodyAndRigidBody
			&& _physicWorldParameters.collisionType == CollisionConfType.DefaultDynamicsWorldCollisionConf)
		{
			Debug.LogError("For World Type = SoftBodyAndRigidBody collisionType must be collisionType=SoftBodyRigidBodyCollisionConf. Switching");
			//m_collisionType = PhysicWorldEnums.CollisionConfType.SoftBodyRigidBodyCollisionConf;
			return;
		}

		Time.fixedDeltaTime = _physicWorldParameters.fixedTimeStep;

		//Create default Physic World
		ActualWorldID = 0;
		worldControllerList.Add(ActualWorldID, new WorldController());
	}

	private void OnDestroy()
	{
		for(int i = 0; i < worldControllerList.Count; i++)
		{
			worldControllerList[i].Dispose();
		}

		worldControllerList.Clear();
	}

	private void FixedUpdate()
	{
		//worldControllerList[0].StepSimulation(_physicWorldParameters.fixedTimeStep);
	}
}
