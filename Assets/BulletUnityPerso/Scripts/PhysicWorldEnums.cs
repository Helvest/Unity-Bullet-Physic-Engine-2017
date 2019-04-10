namespace PhysicWorldEnums
{
	public enum WorldType
	{
		CollisionOnly,
		RigidBodyDynamics,
		MultiBodyWorld, //for FeatherStone forward dynamics I think
		SoftBodyAndRigidBody
	}

	public enum CollisionConfType
	{
		DefaultDynamicsWorldCollisionConf,
		SoftBodyRigidBodyCollisionConf
	}

	public enum BroadphaseType
	{
		DynamicAABBBroadphase,
		Axis3SweepBroadphase,
		Axis3SweepBroadphase_32bit,
		//SimpleBroadphase,
	}

	public enum TimeStepTypes
	{
		OneStep,
		SubStep,
		SubStepAndTimeStep
	}
}