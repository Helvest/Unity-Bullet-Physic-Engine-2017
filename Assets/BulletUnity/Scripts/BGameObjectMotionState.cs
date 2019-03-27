using System;
using BulletSharp;
using UnityEngine;
using BM = BulletSharp.Math;

namespace BulletUnity
{
	[Serializable]
	public class BGameObjectMotionState : MotionState, IDisposable
	{
		public Transform transform;

		public BGameObjectMotionState(Transform t)
		{
			transform = t;
		}

		//public delegate void GetTransformDelegate(out BM.Matrix worldTrans);
		//public delegate void SetTransformDelegate(ref BM.Matrix m);

		private static BM.Matrix matrix;
		private static float _xx, _yy, _zz, _xy, _zw, _zx, _yw, _yz, _xw;

		//Bullet wants me to fill in worldTrans
		//This is called by bullet once when rigid body is added to the the world
		//For kinematic rigid bodies it is called every simulation step
		//[MonoPInvokeCallback(typeof(GetTransformDelegate))]
		public override void GetWorldTransform(out BM.Matrix worldTrans)
		{
			_xx = transform.rotation.x * transform.rotation.x;
			_yy = transform.rotation.y * transform.rotation.y;
			_zz = transform.rotation.z * transform.rotation.z;
			_xy = transform.rotation.x * transform.rotation.y;
			_zw = transform.rotation.z * transform.rotation.w;
			_zx = transform.rotation.z * transform.rotation.x;
			_yw = transform.rotation.y * transform.rotation.w;
			_yz = transform.rotation.y * transform.rotation.z;
			_xw = transform.rotation.x * transform.rotation.w;

			matrix.M11 = 1.0f - (2.0f * (_yy + _zz));
			matrix.M12 = 2.0f * (_xy + _zw);
			matrix.M13 = 2.0f * (_zx - _yw);

			matrix.M21 = 2.0f * (_xy - _zw);
			matrix.M22 = 1.0f - (2.0f * (_zz + _xx));
			matrix.M23 = 2.0f * (_yz + _xw);

			matrix.M31 = 2.0f * (_zx + _yw);
			matrix.M32 = 2.0f * (_yz - _xw);
			matrix.M33 = 1.0f - (2.0f * (_yy + _xx));

			matrix.M41 = transform.position.x;
			matrix.M42 = transform.position.y;
			matrix.M43 = transform.position.z;

			worldTrans = matrix;
		}

		private static Vector3 tempVector3 = Vector3.zero;
		private static Quaternion tempQuaternion = Quaternion.identity;

		//Bullet calls this so I can copy bullet data to unity
		public override void SetWorldTransform(ref BM.Matrix m)
		{
			transform.position = BSExtensionMethods2.ExtractTranslationFromMatrix(ref m, ref tempVector3);
			transform.rotation = BSExtensionMethods2.ExtractRotationFromMatrix(ref m, ref tempQuaternion);
		}
	}
}
