using BulletSharp;
using UnityEngine;

namespace BulletUnity
{
	[AddComponentMenu("Physics Bullet/Shapes/Sphere")]
	public class BSphereShape : BCollisionShape
	{
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
		protected Vector3 m_localScaling = Vector3.one;
		public Vector3 LocalScaling
		{
			get { return m_localScaling; }
			set
			{
				m_localScaling = value;
				if (collisionShapePtr != null)
				{
					((SphereShape)collisionShapePtr).LocalScaling = value.ToBullet();
				}
			}
		}

		public override void OnDrawGizmosSelected()
		{
			BUtility.DebugDrawSphere(transform.position, transform.rotation, m_localScaling, radius, Color.green);
		}

		public override CollisionShape CopyCollisionShape()
		{
			SphereShape ss = new SphereShape(radius);
			ss.LocalScaling = m_localScaling.ToBullet();

			return ss;
		}

		public override CollisionShape GetCollisionShape()
		{
			if (collisionShapePtr == null)
			{
				collisionShapePtr = CopyCollisionShape();
			}

			return collisionShapePtr;
		}
	}
}
