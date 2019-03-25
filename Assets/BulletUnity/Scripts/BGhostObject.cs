using System.Collections.Generic;
using BulletSharp;
using UnityEngine;

namespace BulletUnity
{
	public class BGhostObject : BCollisionObject
	{
		private GhostObject m_ghostObject
		{
			get { return (GhostObject)collisionObject; }
		}

		internal override bool _BuildCollisionObject()
		{
			BPhysicsWorld world = BPhysicsWorld.Get();
			if (collisionObject != null)
			{
				if (isInWorld && world != null)
				{
					world.RemoveCollisionObject(this);
				}
			}

			if (transform.localScale != Vector3.one)
			{
				Debug.LogError("The local scale on this collision shape is not one. Bullet physics does not support scaling on a rigid body world transform. Instead alter the dimensions of the CollisionShape.");
			}

			m_collisionShape = GetComponent<BCollisionShape>();
			if (m_collisionShape == null)
			{
				Debug.LogError("There was no collision shape component attached to this BRigidBody. " + name);
				return false;
			}

			CollisionShape cs = m_collisionShape.GetCollisionShape();

			if (collisionObject == null)
			{
				collisionObject = new BulletSharp.GhostObject();
				collisionObject.CollisionShape = cs;
				BulletSharp.Math.Matrix worldTrans;
				BulletSharp.Math.Quaternion q = transform.rotation.ToBullet();
				BulletSharp.Math.Matrix.RotationQuaternion(ref q, out worldTrans);
				worldTrans.Origin = transform.position.ToBullet();
				collisionObject.WorldTransform = worldTrans;
				collisionObject.UserObject = this;
				collisionObject.CollisionFlags = collisionObject.CollisionFlags | BulletSharp.CollisionFlags.NoContactResponse;
			}
			else
			{
				BulletSharp.Math.Matrix worldTrans;
				BulletSharp.Math.Quaternion q = transform.rotation.ToBullet();
				BulletSharp.Math.Matrix.RotationQuaternion(ref q, out worldTrans);
				worldTrans.Origin = transform.position.ToBullet();
				collisionObject.WorldTransform = worldTrans;
				collisionObject.CollisionShape = cs;
				collisionObject.CollisionFlags = collisionObject.CollisionFlags | BulletSharp.CollisionFlags.NoContactResponse;
			}

			return true;
		}

		private HashSet<CollisionObject> objsIWasInContactWithLastFrame = new HashSet<CollisionObject>();
		private HashSet<CollisionObject> objsCurrentlyInContactWith = new HashSet<CollisionObject>();
		private CollisionObject otherObj;
		private void FixedUpdate()
		{
			//TODO should do two passes like with collisions
			objsCurrentlyInContactWith.Clear();
			for (int i = 0; i < m_ghostObject.NumOverlappingObjects; i++)
			{
				otherObj = m_ghostObject.GetOverlappingObject(i);
				objsCurrentlyInContactWith.Add(otherObj);
				if (!objsIWasInContactWithLastFrame.Contains(otherObj))
				{
					BOnTriggerEnter(otherObj, null);
				}
				else
				{
					BOnTriggerStay(otherObj, null);
				}
			}

			objsIWasInContactWithLastFrame.ExceptWith(objsCurrentlyInContactWith);

			foreach (CollisionObject co in objsIWasInContactWithLastFrame)
			{
				BOnTriggerExit(co);
			}

			//swap the hashsets so objsIWasInContactWithLastFrame now contains the list of objs.
			HashSet<CollisionObject> temp = objsIWasInContactWithLastFrame;
			objsIWasInContactWithLastFrame = objsCurrentlyInContactWith;
			objsCurrentlyInContactWith = temp;
		}

		public virtual void BOnTriggerEnter(CollisionObject other, AlignedManifoldArray details)
		{
			//Debug.LogWarning("BOnTriggerEnter: " + ((BRigidBody)other.UserObject).name);
		}

		public virtual void BOnTriggerStay(CollisionObject other, AlignedManifoldArray details)
		{
		}

		public virtual void BOnTriggerExit(CollisionObject other)
		{
		}
	}
}
