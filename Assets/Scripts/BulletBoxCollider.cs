using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using BulletUnity;
using UnityEngine;
using Vector3B = BulletSharp.Math.Vector3;

public class BulletBoxCollider : BulletCollider
{
	[SerializeField]
	private Vector3 size;

	private CollisionShape collisionShape;

	protected override void Awake()
	{
		base.Awake();

		//Vector3 trueSize = Vector3.Cross(transform.lossyScale, size);

		//BoxShape bob = new BoxShape(size.ToBullet());
		//collisionShape.LocalScaling = transform.lossyScale.ToBullet();
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
		Gizmos.DrawWireCube(center, size);
	}
#endif
}
