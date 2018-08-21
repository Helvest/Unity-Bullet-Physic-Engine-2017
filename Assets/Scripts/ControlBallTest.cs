using BulletUnity;
using UnityEngine;

public class ControlBallTest : MonoBehaviour
{
	private B6DOFConstraint _B6DOFConstraint;
	//private BRigidBody _BRigidBody;

	[SerializeField]
	private Vector3 origine;
	private Vector3 tempVector3;

	private void Awake()
	{
		_B6DOFConstraint = GetComponent<B6DOFConstraint>();
		//_BRigidBody = GetComponent<BRigidBody>();

		origine = transform.position;
	}

	private void Update()
	{
		if (contraintYChange != contraintY)
		{
			contraintYChange = contraintY;

			if (contraintY)
			{
				float newY = transform.position.y - origine.y;

				tempVector3 = _B6DOFConstraint.linearLimitLower;
				tempVector3.y = newY;
				_B6DOFConstraint.linearLimitLower = tempVector3;

				tempVector3 = _B6DOFConstraint.linearLimitUpper;
				tempVector3.y = newY;
				_B6DOFConstraint.linearLimitUpper = tempVector3;
			}
			else
			{
				tempVector3 = _B6DOFConstraint.linearLimitLower;
				tempVector3.y = 1;
				_B6DOFConstraint.linearLimitLower = tempVector3;

				tempVector3 = _B6DOFConstraint.linearLimitUpper;
				tempVector3.y = 0;
				_B6DOFConstraint.linearLimitUpper = tempVector3;
			}
		}
	}

	private bool contraintYChange;
	public bool contraintY;
}
