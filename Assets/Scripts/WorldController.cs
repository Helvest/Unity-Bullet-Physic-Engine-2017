using System;
using System.Collections.Generic;
using BulletSharp;
using BulletSharp.SoftBody;
using BulletUnity;
using BulletUnity.Debugging;
using PhysicWorldEnums;
using UnityEngine;

[System.Serializable]
public class WorldController : IDisposable
{
	#region Variables

	public PhysicWorldParameters physicWorldParameters { get; private set; }
	public CollisionConfiguration collisionConf { get; private set; }
	public CollisionDispatcher dispatcher { get; private set; }
	public BroadphaseInterface broadphase { get; private set; }
	public ConstraintSolver constraintSolver { get; private set; }
	public GhostPairCallback ghostPairCallback { get; private set; }

	private CollisionWorld _world;
	public CollisionWorld World
	{
		get { return _world; }
		private set
		{
			_world = value;

			DWorld = _world is DynamicsWorld ? (DynamicsWorld)_world : null;

			DDWorld = _world is DiscreteDynamicsWorld ? (DiscreteDynamicsWorld)_world : null;

			SoftWorld = _world is SoftRigidDynamicsWorld ? (SoftRigidDynamicsWorld)_world : null;
		}
	}

	// convenience variable so we arn't typecasting all the time.
	public DynamicsWorld DWorld { get; private set; }
	public DiscreteDynamicsWorld DDWorld { get; private set; }
	public SoftRigidDynamicsWorld SoftWorld { get; private set; }

	#endregion Variables

	#region Init

	public WorldController()
	{
		physicWorldParameters = WorldsManager.PhysicWorldParameters;
		CreateWorld();
	}

	public WorldController(PhysicWorldParameters physicWorldParameters)
	{
		this.physicWorldParameters = physicWorldParameters;
		CreateWorld();
	}

	private void CreateWorld()
	{
		if (physicWorldParameters.worldType == WorldType.SoftBodyAndRigidBody && physicWorldParameters.collisionType != CollisionConfType.SoftBodyRigidBodyCollisionConf)
		{
			Debug.LogError("For World Type = SoftBodyAndRigidBody collisionType must be collisionType = SoftBodyRigidBodyCollisionConf.");
			return;
		}

		isDisposed = false;

		switch (physicWorldParameters.collisionType)
		{
			case CollisionConfType.DefaultDynamicsWorldCollisionConf:
				collisionConf = new DefaultCollisionConfiguration();
				break;

			case CollisionConfType.SoftBodyRigidBodyCollisionConf:
				collisionConf = new SoftBodyRigidBodyCollisionConfiguration();
				break;
		}

		dispatcher = new CollisionDispatcher(collisionConf);

		switch (physicWorldParameters.broadphaseType)
		{
			default:
			case BroadphaseType.DynamicAABBBroadphase:
				broadphase = new DbvtBroadphase();
				break;

			case BroadphaseType.Axis3SweepBroadphase:
				broadphase = new AxisSweep3(physicWorldParameters.axis3SweepBroadphaseMin, physicWorldParameters.axis3SweepBroadphaseMax, physicWorldParameters.axis3SweepMaxProxies);
				break;

			case BroadphaseType.Axis3SweepBroadphase_32bit:
				broadphase = new AxisSweep3_32Bit(physicWorldParameters.axis3SweepBroadphaseMin, physicWorldParameters.axis3SweepBroadphaseMax, physicWorldParameters.axis3SweepMaxProxies);
				break;
		}

		switch (physicWorldParameters.worldType)
		{
			case WorldType.CollisionOnly:
				World = new CollisionWorld(dispatcher, broadphase, collisionConf);
				break;

			case WorldType.RigidBodyDynamics:
				World = new DiscreteDynamicsWorld(dispatcher, broadphase, null, collisionConf);
				break;

			case WorldType.MultiBodyWorld:
				MultiBodyConstraintSolver mbConstraintSolver = new MultiBodyConstraintSolver();
				constraintSolver = mbConstraintSolver;
				World = new MultiBodyDynamicsWorld(dispatcher, broadphase, mbConstraintSolver, collisionConf);
				break;

			case WorldType.SoftBodyAndRigidBody:
				SequentialImpulseConstraintSolver siConstraintSolver = new SequentialImpulseConstraintSolver();
				constraintSolver = siConstraintSolver;
				siConstraintSolver.RandSeed = physicWorldParameters.sequentialImpulseConstraintSolverRandomSeed;

				World = new SoftRigidDynamicsWorld(dispatcher, broadphase, siConstraintSolver, collisionConf);
				_world.DispatchInfo.EnableSpu = true;

				SoftWorld.WorldInfo.SparseSdf.Initialize();
				SoftWorld.WorldInfo.SparseSdf.Reset();
				SoftWorld.WorldInfo.AirDensity = 1.2f;
				SoftWorld.WorldInfo.WaterDensity = 0;
				SoftWorld.WorldInfo.WaterOffset = 0;
				SoftWorld.WorldInfo.WaterNormal = BulletSharp.Math.Vector3.Zero;
				SoftWorld.WorldInfo.Gravity = physicWorldParameters.gravity;
				break;
		}

		if (_world is DiscreteDynamicsWorld)
		{
			((DiscreteDynamicsWorld)_world).Gravity = physicWorldParameters.gravity;
		}

		Debug.LogFormat("Physic World of type {0} Instanced", physicWorldParameters.worldType);

		if (physicWorldParameters.debug)
		{
			Debug.Log("Physic World Debug is active");

			DebugDrawUnity db = new DebugDrawUnity();
			db.DebugMode = physicWorldParameters.debugDrawMode;
			_world.DebugDrawer = db;
		}
	}

	#endregion Init

	#region StepSimulation

	public void StepSimulation(float timeStep)
	{
		if (!isDisposed && timeStep > 0f)
		{
			if (DWorld != null)
			{
				//StepSimulation proceeds the simulation over 'timeStep', units in preferably in seconds.
				//By default, Bullet will subdivide the timestep in constant substeps of each 'fixedTimeStep'.
				//In order to keep the simulation real-time, the maximum number of substeps can be clamped to 'maxSubSteps'.
				//You can disable subdividing the timestep/substepping by passing maxSubSteps=0 as second argument to stepSimulation, 
				//but in that case you have to keep the timeStep constant.
				DWorld.StepSimulation(timeStep, physicWorldParameters.maxSubsteps, physicWorldParameters.fixedTimeStep);
			}

			//Collisions
			OnPhysicsStep();
		}
	}

	#endregion StepSimulation

	#region CallbackListeners

	private HashSet<BCollisionObject.BICollisionCallbackEventHandler> collisionCallbackListeners = new HashSet<BCollisionObject.BICollisionCallbackEventHandler>();

	public void RegisterCollisionCallbackListener(BCollisionObject.BICollisionCallbackEventHandler toBeAdded)
	{
		collisionCallbackListeners.Add(toBeAdded);
	}

	public void DeregisterCollisionCallbackListener(BCollisionObject.BICollisionCallbackEventHandler toBeRemoved)
	{
		collisionCallbackListeners.Remove(toBeRemoved);
	}

	private static PersistentManifold contactManifold;
	private static CollisionObject a;
	private static CollisionObject b;
	private static int numManifolds;

	//Check Collisions
	private void OnPhysicsStep()
	{
		numManifolds = dispatcher.NumManifolds;

		for (int i = 0; i < numManifolds; i++)
		{
			contactManifold = dispatcher.GetManifoldByIndexInternal(i);

			a = contactManifold.Body0;
			b = contactManifold.Body1;

			if (a is CollisionObject && a.UserObject is BCollisionObject && ((BCollisionObject)a.UserObject).collisionCallbackEventHandler != null)
			{
				((BCollisionObject)a.UserObject).collisionCallbackEventHandler.OnVisitPersistentManifold(contactManifold);
			}

			if (b is CollisionObject && b.UserObject is BCollisionObject && ((BCollisionObject)b.UserObject).collisionCallbackEventHandler != null)
			{
				((BCollisionObject)b.UserObject).collisionCallbackEventHandler.OnVisitPersistentManifold(contactManifold);
			}
		}

		foreach (BCollisionObject.BICollisionCallbackEventHandler coeh in collisionCallbackListeners)
		{
			if (coeh != null)
			{
				coeh.OnFinishedVisitingManifolds();
			}
		}

		contactManifold = null;
		a = b = null;
	}

	#endregion CallbackListeners

	#region Add & Remove

	public bool AddAction(IAction action)
	{
		if (!isDisposed)
		{
			if (physicWorldParameters.worldType < WorldType.RigidBodyDynamics)
			{
				Debug.LogError("World type must not be collision only");
			}
			else
			{
				DWorld.AddAction(action);

				return true;
			}
		}

		return false;
	}

	public bool RemoveAction(IAction action)
	{
		if (!isDisposed)
		{
			if (physicWorldParameters.worldType < WorldType.RigidBodyDynamics)
			{
				Debug.LogError("World type must not be collision only");
			}
			else
			{
				DDWorld.RemoveAction(action);

				return true;
			}
		}

		return false;
	}

	public bool AddCollisionObject(BCollisionObject co)
	{
		if (!isDisposed)
		{
			if (co is BRigidBody)
			{
				return AddRigidBody((BRigidBody)co);
			}
			else if (co is BSoftBody)
			{
				return AddSoftBody((BSoftBody)co);
			}

			if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
			{
				Debug.LogFormat("Adding collision object {0} to world", co);
			}

			if (co._BuildCollisionObject())
			{
				_world.AddCollisionObject(co.GetCollisionObject(), co.groupsIBelongTo, co.collisionMask);
				co.isInWorld = true;

				if (ghostPairCallback == null && co is BGhostObject && DWorld != null)
				{
					ghostPairCallback = new GhostPairCallback();
					DWorld.PairCache.SetInternalGhostPairCallback(ghostPairCallback);
				}

				if (co is BCharacterController && DWorld != null)
				{
					AddAction(((BCharacterController)co).GetKinematicCharacterController());
				}
			}

			return true;
		}

		return false;
	}

	public bool RemoveCollisionObject(BCollisionObject co)
	{
		if (!isDisposed)
		{
			if (co is BRigidBody)
			{
				return RemoveRigidBody((RigidBody)co.GetCollisionObject());
			}
			else if (co is BSoftBody)
			{
				return RemoveSoftBody((SoftBody)co.GetCollisionObject());
			}

			if (co is BCharacterController && DWorld != null)
			{
				RemoveAction(((BCharacterController)co).GetKinematicCharacterController());
			}

			if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
			{
				Debug.LogFormat("Removing collisionObject {0} from world", co);
			}

			_world.RemoveCollisionObject(co.GetCollisionObject());
			co.isInWorld = false;

			return true;
		}

		return false;
	}

	public bool AddRigidBody(BRigidBody rb)
	{
		if (!isDisposed)
		{
			if (physicWorldParameters.worldType < WorldType.RigidBodyDynamics)
			{
				Debug.LogError("World type must not be collision only");
				return false;
			}

			if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
			{
				Debug.LogFormat("Adding rigidbody {0} to world", rb);
			}

			if (rb._BuildCollisionObject())
			{
				DWorld.AddRigidBody((RigidBody)rb.GetCollisionObject(), rb.groupsIBelongTo, rb.collisionMask);
				rb.isInWorld = true;
			}

			return true;
		}

		return false;
	}

	public bool RemoveRigidBody(RigidBody rb)
	{
		if (!isDisposed)
		{
			if (physicWorldParameters.worldType < WorldType.RigidBodyDynamics)
			{
				Debug.LogError("World type must not be collision only");
				return false;
			}

			if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
			{
				Debug.LogFormat("Removing rigidbody {0} from world", rb.UserObject);
			}

			DWorld.RemoveRigidBody(rb);

			if (rb.UserObject is BCollisionObject)
			{
				((BCollisionObject)rb.UserObject).isInWorld = false;
			}

			return true;
		}

		return false;
	}

	public bool AddConstraint(BTypedConstraint c)
	{
		if (!isDisposed)
		{
			if (physicWorldParameters.worldType < WorldType.RigidBodyDynamics)
			{
				Debug.LogError("World type must not be collision only");
				return false;
			}

			if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
			{
				Debug.LogFormat("Adding constraint {0} to world", c);
			}

			if (c._BuildConstraint())
			{
				DWorld.AddConstraint(c.GetConstraint(), c.disableCollisionsBetweenConstrainedBodies);
				c.m_isInWorld = true;
			}

			return true;
		}

		return false;
	}

	public bool RemoveConstraint(TypedConstraint c)
	{
		if (!isDisposed)
		{
			if (physicWorldParameters.worldType < WorldType.RigidBodyDynamics)
			{
				Debug.LogError("World type must not be collision only");
				return false;
			}

			if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
			{
				Debug.LogFormat("Removing constraint {0} from world", c.Userobject);
			}

			DWorld.RemoveConstraint(c);

			if (c.Userobject is BTypedConstraint)
			{
				((BTypedConstraint)c.Userobject).m_isInWorld = false;
			}

			return true;
		}

		return false;
	}

	public bool AddSoftBody(BSoftBody softBody)
	{
		if (!isDisposed)
		{
			if (physicWorldParameters.worldType == WorldType.SoftBodyAndRigidBody)
			{
				Debug.LogErrorFormat("The Physics World must be a BSoftBodyWorld for adding soft bodies");
				return false;
			}

			if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
			{
				Debug.LogFormat("Adding softbody {0} to world", softBody);
			}

			if (softBody._BuildCollisionObject())
			{
				SoftWorld.AddSoftBody((SoftBody)softBody.GetCollisionObject());
				softBody.isInWorld = true;
			}

			return true;
		}

		return false;
	}

	public bool RemoveSoftBody(SoftBody softBody)
	{
		if (!isDisposed)
		{
			if (physicWorldParameters.worldType == WorldType.SoftBodyAndRigidBody)
			{
				Debug.LogErrorFormat("The Physics World must be a BSoftBodyWorld for removing soft bodies");
				return false;
			}

			if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
			{
				Debug.LogFormat("Removing softbody {0} from world", softBody.UserObject);
			}

			SoftWorld.RemoveSoftBody(softBody);

			if (softBody.UserObject is BCollisionObject)
			{
				((BCollisionObject)softBody.UserObject).isInWorld = false;
			}

			return true;
		}

		return false;
	}

	#endregion Add & Remove

	#region Destroy & Dispose

	public bool isDisposed { get; private set; }

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
		{
			Debug.Log("BDynamicsWorld Disposing physics.");
		}

		if (_world != null)
		{
			//remove/dispose constraints
			int i;
			if (DDWorld != null)
			{
				if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
				{
					Debug.LogFormat("Removing Constraints {0}", DDWorld.NumConstraints);
				}

				for (i = DDWorld.NumConstraints - 1; i >= 0; i--)
				{
					TypedConstraint constraint = DDWorld.GetConstraint(i);
					DDWorld.RemoveConstraint(constraint);

					if (constraint.Userobject is BTypedConstraint)
					{
						((BTypedConstraint)constraint.Userobject).m_isInWorld = false;
					}

					if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
					{
						Debug.LogFormat("Removed Constaint {0}", constraint.Userobject);
					}

					constraint.Dispose();
				}
			}

			if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
			{
				Debug.LogFormat("Removing Collision Objects {0}", DDWorld.NumCollisionObjects);
			}

			//Remove the rigidbodies from the dynamics world and delete them
			for (i = _world.NumCollisionObjects - 1; i >= 0; i--)
			{
				CollisionObject obj = _world.CollisionObjectArray[i];
				RigidBody body = obj as RigidBody;

				if (body != null && body.MotionState != null)
				{
					Debug.Assert(body.NumConstraintRefs == 0, "Rigid body still had constraints");
					body.MotionState.Dispose();
				}

				_world.RemoveCollisionObject(obj);

				if (obj.UserObject is BCollisionObject)
				{
					((BCollisionObject)obj.UserObject).isInWorld = false;
				}

				if (physicWorldParameters.debugType >= BDebug.DebugType.Debug)
				{
					Debug.LogFormat("Removed CollisionObject {0}", obj.UserObject);
				}

				obj.Dispose();
			}

			if (_world.DebugDrawer != null && _world.DebugDrawer is IDisposable)
			{
				((IDisposable)_world.DebugDrawer).Dispose();
			}

			_world.Dispose();
			World = null;
		}

		if (broadphase != null)
		{
			broadphase.Dispose();
			broadphase = null;
		}

		if (dispatcher != null)
		{
			dispatcher.Dispose();
			dispatcher = null;
		}

		if (collisionConf != null)
		{
			collisionConf.Dispose();
			collisionConf = null;
		}

		if (constraintSolver != null)
		{
			constraintSolver.Dispose();
			constraintSolver = null;
		}

		if (ghostPairCallback != null)
		{
			ghostPairCallback.Dispose();
			ghostPairCallback = null;
		}

		isDisposed = true;
	}

	private void OnDestroy()
	{
		if (!isDisposed)
		{
			BDebug.Log(physicWorldParameters.debugType, "Destroying Physics World");
			Dispose(false);
		}
	}

	#endregion Destroy & Dispose
}
