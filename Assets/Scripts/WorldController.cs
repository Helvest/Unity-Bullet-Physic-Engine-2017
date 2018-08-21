using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BulletSharp;
using BulletSharp.Math;
using BulletSharp.SoftBody;
using BulletUnity;
using UnityEngine;
using Vector3B = BulletSharp.Math.Vector3;

public class WorldController
{
	// Physics
	public DynamicsWorld World { get; private set; }
	public SoftRigidDynamicsWorld SoftWorld
	{
		get { return World as SoftRigidDynamicsWorld; }
	}

	public List<CollisionShape> CollisionShapes { get; private set; }

	private CollisionConfiguration CollisionConf;
	private CollisionDispatcher Dispatcher;
	private BroadphaseInterface Broadphase;
	private ConstraintSolver Solver;

	private TypedConstraint pickConstraint;

	public bool isSoftWold { get; private set; }

	// Debug drawing
	private bool _isDebugDrawEnabled;
	private DebugDrawModes _debugDrawMode = DebugDrawModes.DrawWireframe;
	private IDebugDraw _debugDrawer;

	public DebugDrawModes DebugDrawMode
	{
		get
		{
			return _debugDrawMode;
		}
		set
		{
			_debugDrawMode = value;

			if (_debugDrawer != null)
			{
				_debugDrawer.DebugMode = value;
			}
		}
	}

	public bool IsDebugDrawEnabled
	{
		get
		{
			return _isDebugDrawEnabled;
		}

		set
		{
			if (value)
			{
				if (_debugDrawer == null)
				{
					_debugDrawer = new DebugDrawUnity();
					_debugDrawer.DebugMode = _debugDrawMode;
					if (World != null)
					{
						World.DebugDrawer = _debugDrawer;
					}
				}
			}
			else
			{
				if (_debugDrawer != null)
				{
					if (World != null)
					{
						World.DebugDrawer = null;
					}

					if (_debugDrawer is IDisposable)
					{
						(_debugDrawer as IDisposable).Dispose();
					}

					_debugDrawer = null;
				}
			}

			_isDebugDrawEnabled = value;
		}
	}

	public WorldController(bool softWold)
	{
		CollisionShapes = new List<CollisionShape>();

		Initialise(softWold);
	}

	public void Initialise(bool softWold)
	{
		if (World != null)
		{
			ExitPhysics();
		}

		if (softWold)
		{
			OnInitializeSoftPhysics();
		}
		else
		{
			OnInitializePhysics();
		}

		if (_isDebugDrawEnabled)
		{
			if (_debugDrawer == null)
			{
				_debugDrawer = new DebugDrawUnity
				{
					DebugMode = DebugDrawMode
				};
			}

			if (World != null)
			{
				World.DebugDrawer = _debugDrawer;
			}
		}
	}

	private void OnInitializePhysics()
	{
		//Collision configuration contains default setup for memory, collision setup
		CollisionConf = new DefaultCollisionConfiguration();
		Dispatcher = new CollisionDispatcher(CollisionConf);

		Broadphase = new DbvtBroadphase();

		World = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, CollisionConf)
		{
			Gravity = Physics.gravity.ToBullet()
		};
	}

	public SoftBodyWorldInfo softBodyWorldInfo { get; private set; }
	private const int maxProxies = 32766;
	private void OnInitializeSoftPhysics()
	{
		// collision configuration contains default setup for memory, collision setup
		CollisionConf = new SoftBodyRigidBodyCollisionConfiguration();
		Dispatcher = new CollisionDispatcher(CollisionConf);

		Broadphase = new AxisSweep3(new Vector3B(-1000, -1000, -1000), new Vector3B(1000, 1000, 1000), maxProxies);

		// the default constraint solver.
		Solver = new SequentialImpulseConstraintSolver();

		softBodyWorldInfo = new SoftBodyWorldInfo
		{
			AirDensity = 1.2f,
			WaterDensity = 0,
			WaterOffset = 0,
			WaterNormal = Vector3B.Zero,
			Gravity = Physics.gravity.ToBullet(),
			Dispatcher = Dispatcher,
			Broadphase = Broadphase
		};

		softBodyWorldInfo.SparseSdf.Initialize();

		World = new SoftRigidDynamicsWorld(Dispatcher, Broadphase, Solver, CollisionConf)
		{
			Gravity = Physics.gravity.ToBullet()
		};

		World.DispatchInfo.EnableSpu = true;
	}

	/*public void ClientResetScene()
	{
		RemovePickingConstraint();

		ExitPhysics();

		OnInitializePhysics();

		WorldsManager.Instance.PostOnInitializePhysics();

		if (World != null && _debugDrawer != null)
		{
			World.DebugDrawer = _debugDrawer;
		}
	}*/

	public void ExitPhysics()
	{
		if (World != null)
		{
			//remove/dispose constraints
			int i;
			for (i = World.NumConstraints - 1; i >= 0; i--)
			{
				TypedConstraint constraint = World.GetConstraint(i);
				World.RemoveConstraint(constraint);
				constraint.Dispose();
			}

			CollisionObject obj;
			RigidBody body;
			//remove the rigidbodies from the dynamics world and delete them
			for (i = World.NumCollisionObjects - 1; i >= 0; i--)
			{
				obj = World.CollisionObjectArray[i];
				body = obj as RigidBody;

				if (body != null && body.MotionState != null)
				{
					body.MotionState.Dispose();
				}

				World.RemoveCollisionObject(obj);

				obj.Dispose();
			}

			//delete collision shapes
			for (i = CollisionShapes.Count - 1; i >= 0; i--)
			{
				CollisionShapes[i].Dispose();
			}

			CollisionShapes.Clear();

			World.Dispose();
			Debug.Log("World: " + World);
		}

		if (Broadphase != null)
		{
			Broadphase.Dispose();
		}

		if (Dispatcher != null)
		{
			Dispatcher.Dispose();
		}

		if (CollisionConf != null)
		{
			CollisionConf.Dispose();
		}

		if (Solver != null)
		{
			Solver.Dispose();
		}
	}

	public void FixeUpdate(float deltaTime)
	{
		World.StepSimulation(deltaTime);
	}

	public void Dispose()
	{
		ExitPhysics();
		GC.SuppressFinalize(this);
	}

	private RigidBody pickedBody;
	private void RemovePickingConstraint()
	{
		if (pickConstraint != null && World != null)
		{
			World.RemoveConstraint(pickConstraint);
			pickConstraint.Dispose();
			pickConstraint = null;
			pickedBody.ForceActivationState(ActivationState.ActiveTag);
			pickedBody.DeactivationTime = 0;
			pickedBody = null;
		}
	}

	public virtual RigidBody LocalCreateRigidBody(float mass, Vector3B startPosition, CollisionShape shape, bool isKinematic = false)
	{
		return LocalCreateRigidBody(mass, Matrix.Translation(startPosition), shape, isKinematic);
	}

	public virtual RigidBody LocalCreateRigidBody(float mass, Matrix startPosition, CollisionShape shape, bool isKinematic = false)
	{
		CollisionShapes.Add(shape);

		Vector3B localInertia = Vector3B.Zero;

		//rigidbody is dynamic if and only if mass is non zero, otherwise static
		if (mass != 0.0f)
		{
			shape.CalculateLocalInertia(mass, out localInertia);
		}

		//using motionstate is recommended, it provides interpolation capabilities, and only synchronizes 'active' objects
		DefaultMotionState myMotionState = new DefaultMotionState(startPosition);

		RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
		RigidBody body = new RigidBody(rbInfo);

		if (isKinematic)
		{
			body.CollisionFlags = body.CollisionFlags | BulletSharp.CollisionFlags.KinematicObject;
			body.ActivationState = ActivationState.DisableDeactivation;
		}

		rbInfo.Dispose();

		World.AddRigidBody(body);

		return body;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool AllocConsole();
}
