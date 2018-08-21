using BulletSharp;
using BulletUnity;
using UnityEngine;

public class BulletRigidBodyProxy : MonoBehaviour
{
	public CollisionObject target;

	private Transform _transform;

	private void Awake()
	{
		_transform = transform;
	}

	private static Matrix4x4 m;
	private static Vector3 tempVector3;
	private static Quaternion tempQuaternion;

	private void Update()
	{
		target.WorldTransform.ToUnity(ref m);
		_transform.position = BSExtensionMethods2.ExtractTranslationFromMatrix(ref m, ref tempVector3);
		_transform.rotation = BSExtensionMethods2.ExtractRotationFromMatrix(ref m, ref tempQuaternion);
	}
}
