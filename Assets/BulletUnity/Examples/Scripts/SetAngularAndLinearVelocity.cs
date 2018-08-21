using BulletUnity;
using UnityEngine;

public class SetAngularAndLinearVelocity : MonoBehaviour
{
	private BRigidBody rb;

	private void Start()
	{
		rb = GetComponent<BRigidBody>();
		rb.velocity = Vector3.right * 4f;
		rb.angularVelocity = Vector3.up * 5f;
	}

	private void Update()
	{
		Debug.Log("Velocity " + rb.velocity);
	}
}
