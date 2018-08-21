using BulletUnity;
using UnityEngine;

public class AddForcesAndTorques : MonoBehaviour
{
	private BRigidBody rb;

	// Use this for initialization
	private void Start()
	{
		rb = GetComponent<BRigidBody>();
	}

	private void FixedUpdate()
	{
		if (Time.frameCount > 10 && Time.frameCount < 20)
		{
			rb.AddTorque(Vector3.right * 100f);
			rb.AddTorque(Vector3.up * 10f);
		}
	}
}
