﻿using BulletSharp;
using UnityEngine;

namespace BulletUnity
{
	[AddComponentMenu("Physics Bullet/Shapes/Box")]
	public class BBoxShape : BCollisionShape
	{
		[SerializeField]
		protected Vector3 extents = Vector3.one;
		public Vector3 Extents
		{
			get { return extents; }
			set
			{
				if (collisionShapePtr != null && value != extents)
				{
					Debug.LogError("Cannot change the extents after the bullet shape has been created. Extents is only the initial value " +
									"Use LocalScaling to change the shape of a bullet shape.");
				}
				else
				{
					extents = value;
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
					((BoxShape)collisionShapePtr).LocalScaling = value.ToBullet();
				}
			}
		}

		public override void OnDrawGizmosSelected()
		{
			BUtility.DebugDrawBox(transform.position, transform.rotation, m_localScaling, extents, Color.green);
		}

		public override CollisionShape CopyCollisionShape()
		{
			BoxShape bs = new BoxShape(extents.ToBullet());
			bs.LocalScaling = m_localScaling.ToBullet();

			return bs;
		}

		public override CollisionShape GetCollisionShape()
		{
			if (collisionShapePtr == null)
			{
				collisionShapePtr = new BoxShape(extents.ToBullet());
				((BoxShape)collisionShapePtr).LocalScaling = m_localScaling.ToBullet();
			}

			return collisionShapePtr;
		}
	}
}
