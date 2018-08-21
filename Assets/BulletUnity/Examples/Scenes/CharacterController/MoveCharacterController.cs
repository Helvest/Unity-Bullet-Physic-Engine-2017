using BulletUnity;
using UnityEngine;

public class MoveCharacterController : MonoBehaviour
{
	public BCharacterController go;

	private void Update()
	{
		if (Input.GetKey(KeyCode.UpArrow))
		{
			go.Move(go.transform.forward * .1f);
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			go.Move(-go.transform.forward * .1f);
		}
		else
		{
			go.Move(Vector3.zero);
		}

		if (Input.GetKey(KeyCode.LeftArrow))
		{
			go.Rotate(-Mathf.PI * .01f);
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			go.Rotate(Mathf.PI * .01f);
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			go.Jump();
		}
	}
}
