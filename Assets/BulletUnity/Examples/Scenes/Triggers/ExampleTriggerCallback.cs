using BulletSharp;
using BulletUnity;
using UnityEngine;

public class ExampleTriggerCallback : BGhostObject
{
	public override void BOnTriggerEnter(CollisionObject other, AlignedManifoldArray details)
	{
		Debug.Log("Enter with " + other.UserObject + " fixedFrame " + GetWorld().FrameCount);
	}

	public override void BOnTriggerStay(CollisionObject other, AlignedManifoldArray details)
	{
		Debug.Log("Stay with " + other.UserObject + " fixedFrame " + GetWorld().FrameCount);
	}

	public override void BOnTriggerExit(CollisionObject other)
	{
		Debug.Log("Exit with " + other.UserObject + " fixedFrame " + GetWorld().FrameCount);
	}
}
