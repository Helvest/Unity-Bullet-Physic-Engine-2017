using BulletSharp;
using UnityEngine;

namespace BulletUnity
{
	[AddComponentMenu("Physics Bullet/Shapes/Capsule")]
	public class BCapsuleShape : BCollisionShape
	{
		public enum CapsuleAxis
		{
			x = 0,
			y = 1,
			z = 2
		}

		[SerializeField]
		protected float radius = 1f;
		public float Radius
		{
			get { return radius; }

			set
			{
				if (collisionShapePtr != null && value != radius)
				{
					Debug.LogError("Cannot change the radius after the bullet shape has been created. Radius is only the initial value " +
									"Use LocalScaling to change the shape of a bullet shape.");
				}
				else
				{
					radius = value;
				}
			}
		}

		[SerializeField]
		protected float height = 2f;
		public float Height
		{
			get { return height; }

			set
			{
				if (collisionShapePtr != null && value != height)
				{
					Debug.LogError("Cannot change the height after the bullet shape has been created. Height is only the initial value " +
									"Use LocalScaling to change the shape of a bullet shape.");
				}
				else
				{
					height = value;
				}
			}
		}

		[SerializeField]
		protected CapsuleAxis upAxis = CapsuleAxis.y;
		public CapsuleAxis UpAxis
		{
			get { return upAxis; }
			set
			{
				if (collisionShapePtr != null && value != upAxis)
				{
					Debug.LogError("Cannot change the upAxis after the bullet shape has been created. upAxis is only the initial value " +
									"Use LocalScaling to change the shape of a bullet shape.");
				}
				else
				{
					upAxis = value;
				}
			}
		}

		[SerializeField]
		protected Vector3 m_localScaling = Vector3.one;
		public Vector3 LocalScaling
		{
			get { return m_localScaling; }

			set
			{
				m_localScaling = value;
				if (collisionShapePtr != null)
				{
					((CapsuleShape)collisionShapePtr).LocalScaling = value.ToBullet();
				}
			}
		}

		public override void OnDrawGizmosSelected()
		{
			BUtility.DebugDrawCapsule(transform.position, transform.rotation, m_localScaling, radius, height / 2f, (int)upAxis, Color.green);
		}

		private CapsuleShape _CreateCapsuleShape()
		{
			CapsuleShape cs = null;

			switch (upAxis)
			{
				default:
				case CapsuleAxis.x:
					cs = new CapsuleShapeX(radius, height);
					break;
				case CapsuleAxis.y:
					cs = new CapsuleShape(radius, height);
					break;
				case CapsuleAxis.z:
					cs = new CapsuleShapeZ(radius, height);
					break;
			}

			cs.LocalScaling = m_localScaling.ToBullet();

			return cs;
		}

		public override CollisionShape CopyCollisionShape()
		{
			return _CreateCapsuleShape();
		}

		public override CollisionShape GetCollisionShape()
		{
			if (collisionShapePtr == null)
			{
				collisionShapePtr = _CreateCapsuleShape();
			}

			return collisionShapePtr;
		}
	}
}
