using System;
using BulletSharp;
using BulletUnity.Debugging;
using UnityEngine;
using Matrix = BulletSharp.Math.Matrix;
using QuaternionB = BulletSharp.Math.Quaternion;
using Vector3B = BulletSharp.Math.Vector3;

namespace BulletUnity
{
	[AddComponentMenu("Physics Bullet/RigidBody")]
	public class BRigidBody : BCollisionObject, IDisposable
	{
		#region Variables

		private BGameObjectMotionState m_motionState;

		private RigidBody m_rigidBody
		{
			get { return (RigidBody)m_collisionObject; }
			set { m_collisionObject = value; }
		}

		public RigidBody RigidBody
		{
			get
			{
				return (RigidBody)m_collisionObject;
			}
		}

		private Vector3B _localInertia = Vector3B.Zero;
		public Vector3B localInertia
		{
			get
			{
				return _localInertia;
			}
		}

		public override BulletSharp.CollisionFlags collisionFlags
		{
			get { return m_collisionFlags; }
			set
			{
				if (m_collisionObject != null && value != m_collisionFlags)
				{
					bool wasDynamic = isDynamic();

					m_collisionObject.CollisionFlags = value;
					m_collisionFlags = value;

					if (wasDynamic && !isDynamic())
					{
						//need to set mass to zero for kinematic and static
						m_rigidBody.SetMassProps(0f, Vector3B.Zero);
					}
					else if (!wasDynamic && isDynamic())
					{
						//need to set mass to mass
						m_rigidBody.SetMassProps(_mass, _localInertia);
					}
				}
				else
				{
					m_collisionFlags = value;
				}
			}
		}

		[SerializeField]
		private float _friction = .5f;
		public float friction
		{
			get { return _friction; }
			set
			{
				if (m_collisionObject != null && _friction != value)
				{
					m_collisionObject.Friction = value;
				}

				_friction = value;
			}
		}

		[SerializeField]
		private float _rollingFriction = 0f;
		public float rollingFriction
		{
			get { return _rollingFriction; }
			set
			{
				if (m_collisionObject != null && _rollingFriction != value)
				{
					m_collisionObject.RollingFriction = value;
				}

				_rollingFriction = value;
			}
		}

		[SerializeField]
		private float _linearDamping = 0f;
		public float linearDamping
		{
			get { return _linearDamping; }
			set
			{
				if (m_collisionObject != null && _linearDamping != value)
				{
					m_rigidBody.SetDamping(value, _angularDamping);
				}

				_linearDamping = value;
			}
		}

		[SerializeField]
		private float _angularDamping = 0f;
		public float angularDamping
		{
			get { return _angularDamping; }
			set
			{
				if (m_collisionObject != null && _angularDamping != value)
				{
					m_rigidBody.SetDamping(_linearDamping, value);
				}

				_angularDamping = value;
			}
		}

		[SerializeField]
		private float _restitution = 0f;
		public float restitution
		{
			get { return _restitution; }
			set
			{
				if (m_collisionObject != null && _restitution != value)
				{
					m_collisionObject.Restitution = value;
				}

				_restitution = value;
			}
		}

		[SerializeField]
		private float _linearSleepingThreshold = .8f;
		public float linearSleepingThreshold
		{
			get { return _linearSleepingThreshold; }
			set
			{
				if (m_collisionObject != null && _linearSleepingThreshold != value)
				{
					m_rigidBody.SetSleepingThresholds(value, _angularSleepingThreshold);
				}

				_linearSleepingThreshold = value;
			}
		}

		[SerializeField]
		private float _angularSleepingThreshold = 1f;
		public float angularSleepingThreshold
		{
			get { return _angularSleepingThreshold; }
			set
			{
				if (m_collisionObject != null && _angularSleepingThreshold != value)
				{
					m_rigidBody.SetSleepingThresholds(_linearSleepingThreshold, value);
				}

				_angularSleepingThreshold = value;
			}
		}

		[SerializeField]
		private bool _additionalDamping = false;
		public bool additionalDamping
		{
			get { return _additionalDamping; }
			set
			{
				if (isInWorld && _additionalDamping != value)
				{
					Debug.LogError("Need to remove and re-add the rigid body to change additional damping setting");
					return;
				}

				_additionalDamping = value;
			}
		}

		[SerializeField]
		private float _additionalDampingFactor = .005f;
		public float additionalDampingFactor
		{
			get { return _additionalDampingFactor; }
			set
			{
				if (m_collisionObject != null && _additionalDampingFactor != value)
				{
					Debug.LogError("Additional Damping settings cannot be changed once the Rigid Body has been created");
					return;
				}

				_additionalDampingFactor = value;
			}
		}

		[SerializeField]
		private float _additionalLinearDampingThresholdSqr = .01f;
		public float additionalLinearDampingThresholdSqr
		{
			get { return _additionalLinearDampingThresholdSqr; }
			set
			{
				if (m_collisionObject != null && _additionalLinearDampingThresholdSqr != value)
				{
					Debug.LogError("Additional Damping settings cannot be changed once the Rigid Body has been created");
					return;
				}

				_additionalLinearDampingThresholdSqr = value;
			}
		}

		[SerializeField]
		private float _additionalAngularDampingThresholdSqr = .01f;
		public float additionalAngularDampingThresholdSqr
		{
			get { return _additionalAngularDampingThresholdSqr; }
			set
			{
				if (m_collisionObject != null && _additionalAngularDampingThresholdSqr != value)
				{
					Debug.LogError("Additional Damping settings cannot be changed once the Rigid Body has been created");
					return;
				}

				_additionalAngularDampingThresholdSqr = value;
			}
		}

		[SerializeField]
		private float _additionalAngularDampingFactor = .01f;
		public float additionalAngularDampingFactor
		{
			get { return _additionalAngularDampingFactor; }
			set
			{
				if (m_collisionObject != null && _additionalAngularDampingFactor != value)
				{
					Debug.LogError("Additional Damping settings cannot be changed once the Rigid Body has been created");
					return;
				}

				_additionalAngularDampingFactor = value;
			}
		}

		/* can lock axis with this */
		[SerializeField]
		private Vector3 _linearFactor = Vector3.one;
		public Vector3 linearFactor
		{
			get { return _linearFactor; }
			set
			{
				if (m_collisionObject != null && _linearFactor != value)
				{
					m_rigidBody.LinearFactor = value.ToBullet();
				}

				_linearFactor = value;
			}
		}

		[SerializeField]
		private Vector3 _angularFactor = Vector3.one;
		public Vector3 angularFactor
		{
			get { return _angularFactor; }
			set
			{
				if (m_rigidBody != null && _angularFactor != value)
				{
					m_rigidBody.AngularFactor = value.ToBullet();
				}

				_angularFactor = value;
			}
		}

		[SerializeField]
		private float _mass = 1f;
		public float mass
		{
			set
			{
				_mass = value;
			}

			get
			{
				return _mass;
			}
		}

		[SerializeField]
		protected Vector3 _linearVelocity;
		public Vector3 velocity
		{
			get
			{
				if (m_rigidBody != null)
				{
					return m_rigidBody.LinearVelocity.ToUnity();
				}
				else
				{
					return _linearVelocity;
				}
			}
			set
			{
				if (m_rigidBody != null)
				{
					m_rigidBody.LinearVelocity = value.ToBullet();
				}

				_linearVelocity = value;
			}
		}

		[SerializeField]
		protected Vector3 _angularVelocity;
		public Vector3 angularVelocity
		{
			get
			{
				if (m_rigidBody != null)
				{
					return m_rigidBody.AngularVelocity.ToUnity();
				}
				else
				{
					return _angularVelocity;
				}
			}
			set
			{
				if (m_rigidBody != null)
				{
					m_rigidBody.AngularVelocity = value.ToBullet();
				}

				_angularVelocity = value;
			}
		}

		[SerializeField]
		private ActivationState _activationState = ActivationState.Undefined;
		public ActivationState activationState
		{
			get { return _activationState; }
			set
			{
				if (m_collisionObject != null && _activationState != value)
				{
					m_collisionObject.ActivationState = _activationState;
				}

				_activationState = value;
			}
		}

		public BDebug.DebugType debugType;

		#endregion Variables

		#region States

		public bool isDynamic()
		{
			return (m_collisionFlags & BulletSharp.CollisionFlags.StaticObject) != BulletSharp.CollisionFlags.StaticObject
				&& (m_collisionFlags & BulletSharp.CollisionFlags.KinematicObject) != BulletSharp.CollisionFlags.KinematicObject;
		}

		public bool isStatic()
		{
			return (m_collisionFlags & BulletSharp.CollisionFlags.StaticObject) == BulletSharp.CollisionFlags.StaticObject;
		}

		public bool isKinematic()
		{
			return (m_collisionFlags & BulletSharp.CollisionFlags.KinematicObject) == BulletSharp.CollisionFlags.KinematicObject;
		}

		#endregion States

		#region BuildCollisionObject

		/// <summary>
		/// Called by Physics World just before rigid body is added to world. 
		/// The current rigid body properties are used to rebuild the rigid body.
		/// </summary>
		internal override bool _BuildCollisionObject()
		{
			BPhysicsWorld world = BPhysicsWorld.Get();

			if (m_rigidBody != null && isInWorld && world != null)
			{
				isInWorld = false;
				world.RemoveRigidBody(m_rigidBody);
			}

			if (transform.localScale != Vector3.one)
			{
				Debug.LogErrorFormat("The local scale on {0} rigid body is not one. Bullet physics does not support scaling on a rigid body world transform. Instead alter the dimensions of the CollisionShape.", name);
			}

			m_collisionShape = GetComponent<BCollisionShape>();
			if (m_collisionShape == null)
			{
				Debug.LogErrorFormat("There was no collision shape component attached to this BRigidBody. {0}", name);
				return false;
			}

			CollisionShape cs = m_collisionShape.GetCollisionShape();

			//MotionState only for no static objects
			if (m_motionState == null && !isStatic())
			{
				m_motionState = new BGameObjectMotionState(transform);
			}

			RigidBody rb = (RigidBody)m_collisionObject;

			CreateOrConfigureRigidBody(ref rb, ref _localInertia, cs, m_motionState);

			m_collisionObject = rb;
			m_collisionObject.UserObject = this;

			return true;
		}

		#endregion BuildCollisionObject

		#region Awake

		protected override void Awake()
		{
			BRigidBody[] rbs = GetComponentsInParent<BRigidBody>();
			if (rbs.Length != 1)
			{
				Debug.LogErrorFormat("Can't nest rigid bodies. The transforms are updated by Bullet in undefined order which can cause spasing. Object {0}", name);
			}

			m_collisionShape = GetComponent<BCollisionShape>();
			if (m_collisionShape == null)
			{
				Debug.LogErrorFormat("BRigidBody component {0} does not have a BCollisionShape component.", name);
			}
		}

		#endregion Awake

		#region Add & Remove To World

		protected override void AddObjectToBulletWorld()
		{
			BPhysicsWorld.Get().AddRigidBody(this);
		}

		protected override void RemoveObjectFromBulletWorld()
		{
			BPhysicsWorld pw = BPhysicsWorld.Get();

			if (pw != null && m_rigidBody != null && isInWorld)
			{
				Debug.Assert(m_rigidBody.NumConstraintRefs == 0, "Removing rigid body that still had constraints. Remove constraints first.");
				//constraints must be removed before rigid body is removed
				pw.RemoveRigidBody((RigidBody)m_collisionObject);
			}
		}

		#endregion Add & Remove To World

		#region AddForce & AddImpulse

		public void AddImpulse(Vector3 impulse)
		{
			if (isInWorld)
			{
				m_rigidBody.ApplyCentralImpulse(impulse.ToBullet());
			}
		}

		public void AddImpulseAtPosition(Vector3 impulse, Vector3 relativePostion)
		{
			if (isInWorld)
			{
				m_rigidBody.ApplyImpulse(impulse.ToBullet(), relativePostion.ToBullet());
			}
		}

		public void AddTorqueImpulse(Vector3 impulseTorque)
		{
			if (isInWorld)
			{
				m_rigidBody.ApplyTorqueImpulse(impulseTorque.ToBullet());
			}
		}

		/// <summary>
		///  Warning for single pulses use AddImpulse. AddForce should only be used over a period of time (several fixedTimeSteps or longer)
		/// The force accumulator is cleared after every StepSimulation call including interpolation StepSimulation calls which clear the force
		/// accumulator and do nothing.
		/// </summary>
		public void AddForce(Vector3 force)
		{
			if (isInWorld)
			{
				m_rigidBody.ApplyCentralForce(force.ToBullet());
			}
		}

		/// <summary>
		/// Warning for single pulses use AddImpulse. AddForce should only be used over a period of time (several fixedTimeSteps or longer).
		/// The force accumulator is cleared after every StepSimulation call including interpolation StepSimulation calls which clear the force
		/// accumulator and do nothing.
		/// </summary>
		public void AddForceAtPosition(Vector3 force, Vector3 relativePostion)
		{
			if (isInWorld)
			{
				m_rigidBody.ApplyForce(force.ToBullet(), relativePostion.ToBullet());
			}
		}

		/// <summary>
		/// Warning for single pulses use AddImpulse. AddForce should only be used over a period of time (several fixedTimeSteps or longer)
		/// The force accumulator is cleared after every StepSimulation call including interpolation StepSimulation calls which clear the force
		/// accumulator and do nothing.
		/// </summary>
		public void AddTorque(Vector3 torque)
		{
			if (isInWorld)
			{
				m_rigidBody.ApplyTorque(torque.ToBullet());
			}
		}

		#endregion AddForce & AddImpulse

		#region OnDisable

		protected override void OnDisable()
		{
			if (m_rigidBody != null && isInWorld)
			{
				//all constraints using RB must be disabled before rigid body is disabled
				for (int i = m_rigidBody.NumConstraintRefs - 1; i >= 0; i--)
				{
					BTypedConstraint btc = (BTypedConstraint)m_rigidBody.GetConstraintRef(i).Userobject;
					Debug.Assert(btc != null);
					btc.enabled = false; //should remove it from the scene
				}
			}

			base.OnDisable();
		}

		#endregion OnDisable

		#region Dispose

		protected override void Dispose(bool isdisposing)
		{
			if (isInWorld && isdisposing && m_rigidBody != null)
			{
				BPhysicsWorld pw = BPhysicsWorld.Get();

				if (pw != null && pw.world != null)
				{
					//constraints must be removed before rigid body is removed
					for (int i = m_rigidBody.NumConstraintRefs; i > 0; i--)
					{
						BTypedConstraint tc = (BTypedConstraint)m_rigidBody.GetConstraintRef(i - 1).Userobject;
						((DiscreteDynamicsWorld)pw.world).RemoveConstraint(tc.GetConstraint());
					}

					((DiscreteDynamicsWorld)pw.world).RemoveRigidBody(m_rigidBody);
				}
			}

			if (m_rigidBody != null)
			{
				if (m_rigidBody.MotionState != null)
				{
					m_rigidBody.MotionState.Dispose();
				}

				m_rigidBody.Dispose();
				m_rigidBody = null;
			}
		}

		#endregion Dispose

		#region CreateOrConfigureRigidBody

		/// <summary>
		///  Creates or configures a RigidBody based on the current settings. Does not alter the internal state of this component in any way. 
		///  Can be used to create copies of this BRigidBody for use in other physics simulations.
		/// </summary>
		public bool CreateOrConfigureRigidBody(ref RigidBody rb, ref Vector3B localInertia, CollisionShape cs, MotionState motionState)
		{
			//rigidbody is dynamic if and only if mass is non zero, otherwise static
			localInertia = Vector3B.Zero;
			if (isDynamic())
			{
				cs.CalculateLocalInertia(_mass, out localInertia);
			}

			if (rb == null)
			{
				float bulletMass = _mass;
				if (!isDynamic())
				{
					bulletMass = 0f;
				}

				RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(bulletMass, motionState, cs, localInertia)
				{
					Friction = _friction,
					RollingFriction = _rollingFriction,
					LinearDamping = _linearDamping,
					AngularDamping = _angularDamping,
					Restitution = _restitution,
					LinearSleepingThreshold = _linearSleepingThreshold,
					AngularSleepingThreshold = _angularSleepingThreshold,
					AdditionalDamping = _additionalDamping,
					AdditionalAngularDampingFactor = _additionalAngularDampingFactor,
					AdditionalAngularDampingThresholdSqr = _additionalAngularDampingThresholdSqr,
					AdditionalDampingFactor = _additionalDampingFactor,
					AdditionalLinearDampingThresholdSqr = _additionalLinearDampingThresholdSqr
				};

				rb = new RigidBody(rbInfo);

				if (motionState == null)
				{
					QuaternionB rotation = transform.rotation.ToBullet();
					Vector3B position = transform.position.ToBullet();
					//Vector3B scale = transform.lossyScale.ToBullet();

					rb.WorldTransform = Matrix.AffineTransformationCustom(ref position, ref rotation);
				}

				rbInfo.Dispose();
			}
			else
			{
				rb.SetMassProps(isDynamic() ? _mass : 0, localInertia);
				rb.Friction = _friction;
				rb.RollingFriction = _rollingFriction;
				rb.SetDamping(_linearDamping, _angularDamping);
				rb.Restitution = _restitution;
				rb.SetSleepingThresholds(_linearSleepingThreshold, _angularSleepingThreshold);
				rb.CollisionShape = cs;
			}

			rb.AngularVelocity = angularVelocity.ToBullet();
			rb.LinearVelocity = velocity.ToBullet();

			rb.CollisionFlags = m_collisionFlags;
			rb.LinearFactor = _linearFactor.ToBullet();
			rb.AngularFactor = _angularFactor.ToBullet();

			if (m_rigidBody != null)
			{
				rb.DeactivationTime = m_rigidBody.DeactivationTime;
				rb.InterpolationLinearVelocity = m_rigidBody.InterpolationLinearVelocity;
				rb.InterpolationAngularVelocity = m_rigidBody.InterpolationAngularVelocity;
				rb.InterpolationWorldTransform = m_rigidBody.InterpolationWorldTransform;
			}

			//if kinematic then disable deactivation
			if ((m_collisionFlags & BulletSharp.CollisionFlags.KinematicObject) != 0)
			{
				rb.ActivationState = ActivationState.DisableDeactivation;
			}

			return true;
		}

		#endregion CreateOrConfigureRigidBody
	}
}
