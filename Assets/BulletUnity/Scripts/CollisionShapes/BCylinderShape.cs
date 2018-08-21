﻿using BulletSharp;
using UnityEngine;

namespace BulletUnity
{
	[AddComponentMenu("Physics Bullet/Shapes/Cylinder")]
	public class BCylinderShape : BCollisionShape
	{
		[SerializeField]
		protected Vector3 halfExtent = new Vector3(0.5f, 0.5f, 0.5f);
		public Vector3 HalfExtent
		{
			get { return halfExtent; }
			set
			{
				if (collisionShapePtr != null && value != halfExtent)
				{
					Debug.LogError("Cannot change the extents after the bullet shape has been created. Extents is only the initial value " +
									"Use LocalScaling to change the shape of a bullet shape.");
				}
				else
				{
					halfExtent = value;
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
					((CylinderShape)collisionShapePtr).LocalScaling = value.ToBullet();
				}
			}
		}

		public override void OnDrawGizmosSelected()
		{
			BUtility.DebugDrawCylinder(transform.position, transform.rotation, m_localScaling, halfExtent.x, halfExtent.y, 1, Color.green);
		}

		public override CollisionShape CopyCollisionShape()
		{
			CylinderShape cs = new CylinderShape(halfExtent.ToBullet());
			cs.LocalScaling = m_localScaling.ToBullet();

			return cs;
		}

		public override CollisionShape GetCollisionShape()
		{
			if (collisionShapePtr == null)
			{
				collisionShapePtr = new CylinderShape(halfExtent.ToBullet());
				((CylinderShape)collisionShapePtr).LocalScaling = m_localScaling.ToBullet();
			}

			return collisionShapePtr;
		}
	}
}
