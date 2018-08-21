using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using BulletSharp.Math;
using BulletUnity;
using UnityEngine;
using Vector3B = BulletSharp.Math.Vector3;

public abstract class BulletCollider : MonoBehaviour
{
	public CollisionObject target;

	protected Transform _transform;

	[Header("Body")]
	[SerializeField]
	protected float mass = 1;
	[SerializeField]
	protected float drag = 0;
	[SerializeField]
	protected float angularDrag = 0;

	[SerializeField]
	protected float restitution = 0.5f;
	[SerializeField]
	protected float friction = 0;
	[SerializeField]
	protected float rollingFriction = 0;

	[SerializeField]
	protected float linearSleepingThreshold = 0;
	[SerializeField]
	protected float angularSleepingThreshold = 0;

	[SerializeField]
	protected bool isKinematique;

	[Space, Header("Collider")]
	[SerializeField]
	protected bool isTrigger;
	[SerializeField]
	protected UnityEngine.Vector3 center;

	protected virtual void Awake()
	{
		_transform = transform;
	}

	protected virtual void CreateBody(CollisionShape collisionShape)
	{
		if (isTrigger)
		{
			//GhostObject ghostObject = new GhostObject();
		}


		//WorldsManager.Instance.worldController.CollisionShapes.Add(collisionShape);

		RigidBodyConstructionInfo rbInfo;

		if (!gameObject.isStatic)
		{
			Vector3B localInertia = collisionShape.CalculateLocalInertia(mass);
			rbInfo = new RigidBodyConstructionInfo(mass, null, collisionShape, localInertia);
		}
		else
		{
			mass = 0;
			rbInfo = new RigidBodyConstructionInfo(mass, null, collisionShape);
		}
		
		rbInfo.LinearDamping = drag;
		rbInfo.AngularDamping = angularDrag;

		rbInfo.Restitution = restitution;
		rbInfo.Friction = friction;
		rbInfo.RollingFriction = rollingFriction;
		rbInfo.LinearSleepingThreshold = linearSleepingThreshold;
		rbInfo.AngularSleepingThreshold = angularSleepingThreshold;

		rbInfo.StartWorldTransform = Matrix.Translation(_transform.position.ToBullet());

		RigidBody body = new RigidBody(rbInfo);

		rbInfo.Dispose();
	}

	private static Matrix4x4 m = Matrix4x4.identity;
	private static UnityEngine.Vector3 tempVector3;
	private static UnityEngine.Quaternion tempQuaternion;

	protected virtual void Update()
	{
		if (target != null)
		{
			target.WorldTransform.ToUnity(ref m);
			_transform.position = BSExtensionMethods2.ExtractTranslationFromMatrix(ref m, ref tempVector3);
			_transform.rotation = BSExtensionMethods2.ExtractRotationFromMatrix(ref m, ref tempQuaternion);
		}
	}

/*
	protected void CreateShape()
	{
		//Debug.Log("Creating Shape " + shape);
		switch (BroadphaseNativeType.CylinderShape)
		{
			case BroadphaseNativeType.BoxShape:
				BoxShape box = new BoxShape(Vector3B.One);
				return;


			case BroadphaseNativeType.ConvexHullShape:
				//ConvexHullShape convexHullShape = new ConvexHullShape();
				return;

			case BroadphaseNativeType.ConeShape:
				ConeShape coneShape = new ConeShape(1, 1);
				return;

			case BroadphaseNativeType.CylinderShape:
				//CylinderShape cylinderShape = new CylinderShape();
				return;

			case BroadphaseNativeType.GImpactShape:
				//GImpactMeshShape gImpactMeshShape = new GImpactMeshShape();
				return;

			case BroadphaseNativeType.SphereShape:
				SphereShape sphereShape = new SphereShape(1);
				return;

			case BroadphaseNativeType.StaticPlaneShape:
				StaticPlaneShape staticPlaneShape = new StaticPlaneShape(Vector3B.UnitY, 0);
				return;

			case BroadphaseNativeType.TriangleMeshShape:
				//CreateTriangleMeshShape((shape as TriangleMeshShape).MeshInterface, mesh);
				return;
		}
	}*/
}
