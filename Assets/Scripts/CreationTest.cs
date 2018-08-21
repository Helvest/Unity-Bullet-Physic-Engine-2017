using System.Collections.Generic;
using BulletSharp;
using BulletSharp.Math;
using BulletSharp.SoftBody;
using BulletUnity;
using UnityEngine;
using Vector3B = BulletSharp.Math.Vector3;

public class CreationTest : MonoBehaviour
{
	[SerializeField]
	private GameObject prefabSphere;

	private float defaultMass = 10f;

	private void Start()
	{
		CreateGround();
		WorldsManager.Instance.PostOnInitializePhysics();
		//CreateCubeOfBox();
		CreatePheres();
		//CreateSoftBodyMesh();
	}

	private void CreatePheres()
	{
		//Creation de la forme sans body
		SphereShape theShape = new SphereShape(prefabSphere.transform.lossyScale.y / 2f);
		WorldsManager.Instance.worldController.CollisionShapes.Add(theShape);
		Vector3B localInertia = theShape.CalculateLocalInertia(defaultMass);

		//Info de construction pour body
		RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(defaultMass, null, theShape, localInertia);
		rbInfo.Restitution = 0.95f;

		Vector3B startSpherePosition = new Vector3B(0, 30, 0);

		for (int i = 0; i < 2000; i++)
		{
			//Graphics
			GameObject newGameObject = Instantiate(prefabSphere);

			//Start position
			rbInfo.MotionState = new DefaultMotionState(Matrix.Translation((Random.onUnitSphere * 5f).ToBullet() + startSpherePosition));
			
			//Body
			RigidBody body = new RigidBody(rbInfo);
			//body.AddConstraintRef(new TypedConstraint());

			//Make it drop from a height
			//body.Translate(new Vector3B(0f, 20f, 0f));
			

			WorldsManager.Instance.worldController.World.AddRigidBody(body);

			BulletRigidBodyProxy rbp = newGameObject.AddComponent<BulletRigidBodyProxy>();
			rbp.target = body;
		}
	}

	[SerializeField]
	private MeshFilter MeshPrefab;

	[SerializeField]
	private float Stiffness = 0.5f;
	[SerializeField]
	private int BendingConstraintsDistance = 2;

	private void CreateSoftBodyMesh()
	{
		if (!MeshPrefab)
		{
			return;
		}

		MeshFilter newMesh = Instantiate(MeshPrefab);

		Vector3B[] verticesBullet = new Vector3B[newMesh.mesh.vertices.Length];

		for (int i = verticesBullet.Length - 1; i >= 0; i--)
		{
			verticesBullet[i] = newMesh.mesh.vertices[i].ToBullet();
		}

		SoftBody softBody = SoftBodyHelpers.CreateFromTriMesh(
			WorldsManager.Instance.worldController.softBodyWorldInfo,
			verticesBullet,
			newMesh.mesh.triangles
		);

		BulletSharp.SoftBody.Material pm = softBody.AppendMaterial();
		pm.Flags -= MaterialFlags.DebugDraw;

		pm.LinearStiffness = Stiffness;
		pm.VolumeStiffness = Stiffness;
		pm.AngularStiffness = Stiffness;

		softBody.GenerateBendingConstraints(BendingConstraintsDistance, pm);

		//Config
		softBody.Cfg.PositionIterations = 2;
		softBody.Cfg.DynamicFriction = 0.5f;
		softBody.Cfg.Collisions |= BulletSharp.SoftBody.CollisionFlags.VertexFaceSoftSoft;

		softBody.RandomizeConstraints();

		softBody.Transform(Matrix.Translation(0, 4f, 0));
		softBody.Scale(Vector3B.One);
		softBody.SetTotalMass(2, true);

		//Add to physics world
		WorldsManager.Instance.worldController.SoftWorld.AddSoftBody(softBody);

		//BulletSoftBodyProxy
		BulletSoftBodyProxy sbp = newMesh.gameObject.AddComponent<BulletSoftBodyProxy>();
		sbp.target = softBody;
		sbp.verts = new UnityEngine.Vector3[softBody.Nodes.Count];
		sbp.norms = new UnityEngine.Vector3[softBody.Nodes.Count];
		sbp.tris = newMesh.mesh.triangles;
	}

	private void CreateGround()
	{
		// create the ground
		BoxShape groundShape = new BoxShape(1000f, 1f, 1000f);

		WorldsManager.Instance.worldController.CollisionShapes.Add(groundShape);
		CollisionObject ground = WorldsManager.Instance.worldController.LocalCreateRigidBody(0f, Matrix.Identity, groundShape);
		ground.Restitution = 1.0f;

		ground.UserObject = "Ground";
	}

	protected BoxShape shootBoxShape;
	protected float shootBoxInitialSpeed = 40;

	private bool shootOneBox = false;
	public virtual void ShootBox(Vector3B camPos, Vector3B destination)
	{
		if (WorldsManager.Instance.worldController == null)
			return;

		WorldController.AllocConsole();

		const float mass = 1.0f;

		if (shootBoxShape == null)
		{
			shootBoxShape = new BoxShape(1.0f);
			shootBoxShape.InitializePolyhedralFeatures();
		}

		RigidBody body = WorldsManager.Instance.worldController.LocalCreateRigidBody(mass, Matrix.Translation(camPos), shootBoxShape);
		body.LinearFactor = new Vector3B(1, 1, 1);
		//body.Restitution = 1;

		Vector3B linVel = destination - camPos;
		linVel.Normalize();

		body.LinearVelocity = linVel * shootBoxInitialSpeed;
		body.CcdMotionThreshold = 0.5f;
		body.CcdSweptSphereRadius = 0.9f;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			shootOneBox = true;
		}
	}

	private void FixedUpdate()
	{
		if (shootOneBox)
		{
			ShootBox(Camera.main.transform.position.ToBullet(), (Camera.main.transform.forward * 10f + Camera.main.transform.position).ToBullet());
			shootOneBox = false;
		}
	}

	//Create 125 (5x5x5) dynamic objects
	const int ArraySizeX = 5, ArraySizeY = 5, ArraySizeZ = 5;

	//Scaling of the objects (0.1 = 20 centimeter boxes )
	const float StartPosX = -5;
	const float StartPosY = -5;
	const float StartPosZ = -3;

	public void CreateCubeOfBox()
	{
		//Create a few dynamic rigidbodies
		const float mass = 1.0f;

		BoxShape colShape = new BoxShape(1);
		WorldsManager.Instance.worldController.CollisionShapes.Add(colShape);
		Vector3B localInertia = colShape.CalculateLocalInertia(mass);

		const float startX = StartPosX - ArraySizeX / 2;
		const float startY = StartPosY;
		const float startZ = StartPosZ - ArraySizeZ / 2;

		RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, null, colShape, localInertia);

		for (int k = 0; k < ArraySizeY; k++)
		{
			for (int i = 0; i < ArraySizeX; i++)
			{
				for (int j = 0; j < ArraySizeZ; j++)
				{
					Matrix startTransform = Matrix.Translation(
						2 * i + startX,
						2 * k + startY,
						2 * j + startZ
					);

					// using motionstate is recommended, it provides interpolation capabilities
					// and only synchronizes 'active' objects
					rbInfo.MotionState = new DefaultMotionState(startTransform);
					RigidBody body = new RigidBody(rbInfo);

					// make it drop from a height
					body.Translate(new Vector3B(0f, 20f, 0f));

					WorldsManager.Instance.worldController.World.AddRigidBody(body);
				}
			}
		}

		rbInfo.Dispose();
	}
}
