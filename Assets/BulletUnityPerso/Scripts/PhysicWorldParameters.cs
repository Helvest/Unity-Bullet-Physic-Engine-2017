using BulletSharp;
using BulletUnity.Debugging;
using PhysicWorldEnums;
using UnityEngine;
using Vector3B = BulletSharp.Math.Vector3;

[CreateAssetMenu(fileName = "PhysicWorldParameters", menuName = "PhysicWorldParameters", order = 1)]
public class PhysicWorldParameters : ScriptableObject
{
	public Vector3B gravity = new Vector3B(0f, -9.80665f, 0f);

	public WorldType worldType = WorldType.RigidBodyDynamics;

	public CollisionConfType collisionType = CollisionConfType.DefaultDynamicsWorldCollisionConf;

	public BroadphaseType broadphaseType = BroadphaseType.DynamicAABBBroadphase;

	[Header("Time")]
	public float fixedTimeStep = 1f / 60f;

	public float framerate = 60;

	public int maxSubsteps = 3;

	[Header("3Sweep")]
	public ushort axis3SweepMaxProxies = 32766;

	public Vector3B axis3SweepBroadphaseMin = new Vector3B(-1000f, -1000f, -1000f);

	public Vector3B axis3SweepBroadphaseMax = new Vector3B(1000f, 1000f, 1000f);

	[Header("Soft World")]
	public ulong sequentialImpulseConstraintSolverRandomSeed = 12345;

	[Header("Debug")]
	public bool debug = false;

	public DebugDrawModes debugDrawMode = DebugDrawModes.DrawWireframe;

	public BDebug.DebugType debugType;

}
