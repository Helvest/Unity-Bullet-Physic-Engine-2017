using System.Collections.Generic;
using BulletSharp;
using BulletUnity;
using DemoFramework;
using UnityEngine;
using Vector3B = BulletSharp.Math.Vector3;

public class WorldsManager : MonoBehaviour
{
	public static WorldsManager Instance { get; private set; }

	public WorldController worldController { get; private set; }

	[SerializeField]
	private bool softWorld = false;

	public Material objectMat;
	public Material groundMat;
	public GameObject cubePrefab;
	public GameObject ropePrefab;
	public GameObject softBodyPrefab;
	public List<GameObject> createdObjs = new List<GameObject>();

	private void Awake()
	{
		Instance = this;

		//Create Physic World
		worldController = new WorldController(softWorld)
		{
			//DebugDrawMode = DebugDrawModes.DrawWireframe,
			IsDebugDrawEnabled = false
		};

		//Create Graphics
		//PostOnInitializePhysics();
	}

	private void FixedUpdate()
	{
		worldController.FixeUpdate(Time.fixedDeltaTime * timeSpeed);
	}

	[SerializeField, Range(0f, 10f)]
	private float timeSpeed = 1f;

	/*private void Update()
	{
		if (timeSpeed > 0)
		{
			worldController.FixeUpdate(Time.deltaTime * timeSpeed);
		}	
	}*/

	private void OnDestroy()
	{
		if (worldController != null)
		{
			worldController.Dispose();
		}
	}

	public void ExitPhysics()
	{
		for (int i = 0; i < createdObjs.Count; i++)
		{
			Destroy(createdObjs[i]);
		}

		createdObjs.Clear();
	}

	public void PostOnInitializePhysics()
	{
		GameObject newGameObject;
		GameObject newChildGameObject;
		CollisionObject collisionObject;
		CollisionShape collisionShape;
		BulletRigidBodyProxy rigidBodyProxy;

		for (int i = 0; i < worldController.World.CollisionObjectArray.Count; i++)
		{
			collisionObject = worldController.World.CollisionObjectArray[i];
			collisionShape = collisionObject.CollisionShape;

			switch (collisionShape.ShapeType)
			{
				//Rope or Cloth
				case BroadphaseNativeType.SoftBodyShape:

					BulletSharp.SoftBody.SoftBody softBody = (BulletSharp.SoftBody.SoftBody)collisionObject;

					if (softBody.Faces.Count == 0)
					{
						newGameObject = CreateUnitySoftBodyRope(softBody);
					}
					else
					{
						newGameObject = CreateUnitySoftBodyCloth(softBody);
					}

					break;

				case BroadphaseNativeType.CompoundShape:

					newGameObject = new GameObject("Compund Shape");
					rigidBodyProxy = newGameObject.AddComponent<BulletRigidBodyProxy>();
					rigidBodyProxy.target = collisionObject as RigidBody;

					foreach (CompoundShapeChild child in (collisionShape as CompoundShape).ChildList)
					{
						newChildGameObject = new GameObject(child.ToString());

						MeshFilter mf = newChildGameObject.AddComponent<MeshFilter>();

						MeshFactory2.CreateShape(child.ChildShape, mf.mesh);

						MeshRenderer mr = newChildGameObject.AddComponent<MeshRenderer>();
						mr.sharedMaterial = objectMat;
						newChildGameObject.transform.SetParent(newGameObject.transform);
						Matrix4x4 mt = child.Transform.ToUnity();
						newChildGameObject.transform.localPosition = BSExtensionMethods2.ExtractTranslationFromMatrix(ref mt);
						newChildGameObject.transform.localRotation = BSExtensionMethods2.ExtractRotationFromMatrix(ref mt);
						newChildGameObject.transform.localScale = BSExtensionMethods2.ExtractScaleFromMatrix(ref mt);
					}
					break;

				case BroadphaseNativeType.CapsuleShape:
					CapsuleShape css = (CapsuleShape)collisionShape;

					newChildGameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);

					Destroy(newChildGameObject.GetComponent<Collider>());

					newGameObject = new GameObject();
					newChildGameObject.transform.parent = newGameObject.transform;
					newChildGameObject.transform.localPosition = Vector3.zero;
					newChildGameObject.transform.localRotation = Quaternion.identity;
					newChildGameObject.transform.localScale = new Vector3(css.Radius * 2f, css.HalfHeight * 2f, css.Radius * 2f);

					rigidBodyProxy = newGameObject.AddComponent<BulletRigidBodyProxy>();
					rigidBodyProxy.target = collisionObject;
					break;

				default:
					//Debug.Log("Creating " + collisionShape.ShapeType + " for " + collisionObject.ToString());
					newGameObject = CreateUnityCollisionObjectProxy(collisionObject);
					break;
			}

			if (newGameObject)
			{
				createdObjs.Add(newGameObject);
				//Debug.Log("Created Unity Shape for shapeType=" + collisionObject.CollisionShape.ShapeType + " collisionShape=" + collisionObject.ToString());
			}

			newGameObject = null;
			newChildGameObject = null;
			rigidBodyProxy = null;
		}
	}

	public GameObject CreateUnityCollisionObjectProxy(CollisionObject body)
	{
		if (body is GhostObject)
		{
			Debug.Log("ghost obj");
		}

		GameObject newGameObject = new GameObject(body.ToString());
		MeshFilter mf = newGameObject.AddComponent<MeshFilter>();
		Mesh m = mf.mesh;
		MeshFactory2.CreateShape(body.CollisionShape, m);
		MeshRenderer mr = newGameObject.AddComponent<MeshRenderer>();
		mr.sharedMaterial = objectMat;
		if (body.UserObject != null && body.UserObject.Equals("Ground"))
		{
			mr.sharedMaterial = groundMat;
		}

		BulletRigidBodyProxy rbp = newGameObject.AddComponent<BulletRigidBodyProxy>();
		rbp.target = body;

		return newGameObject;
	}

	public GameObject CreateUnitySoftBodyRope(BulletSharp.SoftBody.SoftBody body)
	{
		//determine what kind of soft body it is
		//rope
		GameObject rope = Instantiate(ropePrefab);

		LineRenderer lr = rope.GetComponent<LineRenderer>();

		lr.positionCount = body.Nodes.Count;

		BulletRopeProxy ropeProxy = rope.GetComponent<BulletRopeProxy>();

		ropeProxy.target = body;

		return rope;
	}

	public GameObject CreateUnitySoftBodyCloth(BulletSharp.SoftBody.SoftBody body)
	{
		//build nodes 2 verts map
		Dictionary<BulletSharp.SoftBody.Node, int> node2vertIdx = new Dictionary<BulletSharp.SoftBody.Node, int>();
		for (int i = 0; i < body.Nodes.Count; i++)
		{
			node2vertIdx.Add(body.Nodes[i], i);
		}

		List<int> tris = new List<int>();
		for (int i = 0; i < body.Faces.Count; i++)
		{
			BulletSharp.SoftBody.Face f = body.Faces[i];

			if (f.Nodes.Count != 3)
			{
				Debug.LogError("Face was not a triangle");
				continue;
			}

			for (int j = 0; j < f.Nodes.Count; j++)
			{
				tris.Add(node2vertIdx[f.Nodes[j]]);
			}
		}

		GameObject newGameObject = Instantiate(softBodyPrefab);
		BulletSoftBodyProxy sbp = newGameObject.GetComponent<BulletSoftBodyProxy>();
		List<int> trisRev = new List<int>();
		for (int i = 0; i < tris.Count; i += 3)
		{
			trisRev.Add(tris[i]);
			trisRev.Add(tris[i + 2]);
			trisRev.Add(tris[i + 1]);
		}

		tris.AddRange(trisRev);
		sbp.target = body;
		sbp.verts = new Vector3[body.Nodes.Count];
		sbp.tris = tris.ToArray();

		return newGameObject;
	}

	public void CreateUnityMultiBodyLinkColliderProxy(MultiBodyLinkCollider body)
	{
		GameObject cube = Instantiate(cubePrefab);
		CollisionShape collisionShape = body.CollisionShape;
		if (collisionShape is BoxShape)
		{
			BoxShape bxcs = collisionShape as BoxShape;
			Vector3B s = bxcs.HalfExtentsWithMargin;
			MeshRenderer mr = cube.GetComponentInChildren<MeshRenderer>();
			mr.transform.localScale = s.ToUnity() * 2f;
			Matrix4x4 m = body.WorldTransform.ToUnity();
			cube.transform.position = BSExtensionMethods2.ExtractTranslationFromMatrix(ref m);
			cube.transform.rotation = BSExtensionMethods2.ExtractRotationFromMatrix(ref m);
			cube.transform.localScale = BSExtensionMethods2.ExtractScaleFromMatrix(ref m);
			Destroy(cube.GetComponent<BulletRigidBodyProxy>());
			BulletMultiBodyLinkColliderProxy cp = cube.AddComponent<BulletMultiBodyLinkColliderProxy>();
			cp.target = body;
		}
		else
		{
			Debug.LogError("Not implemented");
		}
	}
}
