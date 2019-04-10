﻿using BulletSharp;
using UnityEngine;

namespace BulletUnity
{
	/**
    This script is last in the script execution order. Its purpose is to ensure that StepSimulation is called after other scripts LateUpdate calls
    Do not add this script manually. The BPhysicsWorld will add it.
    */
	
	public class BPhysicsWorldLateHelper : MonoBehaviour
	{
		internal BPhysicsWorld m_physicsWorld;
		internal BDefaultCollisionHandler m_collisionEventHandler = new BDefaultCollisionHandler();
		public void RegisterCollisionCallbackListener(BCollisionObject.BICollisionCallbackEventHandler toBeAdded)
		{
			if (m_collisionEventHandler != null)
			{
				m_collisionEventHandler.RegisterCollisionCallbackListener(toBeAdded);
			}
		}

		public void DeregisterCollisionCallbackListener(BCollisionObject.BICollisionCallbackEventHandler toBeRemoved)
		{
			if (m_collisionEventHandler != null)
			{
				m_collisionEventHandler.DeregisterCollisionCallbackListener(toBeRemoved);
			}
		}

		internal DiscreteDynamicsWorld m_ddWorld;
		internal CollisionWorld m_world;
		internal int m__frameCount = 0;
		internal float m_fixedTimeStep = 1f / 60f;
		internal int m_maxSubsteps = 3;

		protected virtual void FixedUpdate()
		{
			if (m_ddWorld != null)
			{
				float deltaTime = Time.fixedDeltaTime;
				if (Time.fixedDeltaTime > 0f)
				{
					///stepSimulation proceeds the simulation over 'timeStep', units in preferably in seconds.
					///By default, Bullet will subdivide the timestep in constant substeps of each 'fixedTimeStep'.
					///in order to keep the simulation real-time, the maximum number of substeps can be clamped to 'maxSubSteps'.
					///You can disable subdividing the timestep/substepping by passing maxSubSteps=0 as second argument to stepSimulation, but in that case you have to keep the timeStep constant.
					int numSteps = m_ddWorld.StepSimulation(Time.fixedDeltaTime, m_maxSubsteps, m_fixedTimeStep);
					m__frameCount += numSteps;
				}
			}

			//collisions
			if (m_collisionEventHandler != null)
			{
				m_collisionEventHandler.OnPhysicsStep(m_world);
			}
		}
	}
}
