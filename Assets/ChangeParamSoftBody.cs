using BulletSharp.SoftBody;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeParamSoftBody : MonoBehaviour
{
	private BSoftBodyWMesh bSoftBodyWMesh;

	[SerializeField]
	private float speed = 1f;

	private void Awake()
    {
        bSoftBodyWMesh = GetComponent<BSoftBodyWMesh>();
    }

	private void FixedUpdate()
	{
		float theValue = 0;

		if (Input.GetKey(KeyCode.LeftArrow))
		{
			theValue -= Time.fixedDeltaTime * speed;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			theValue += Time.fixedDeltaTime * speed;
		}

		if (theValue != 0)
		{
			SoftBody m_BSoftBody = (SoftBody)bSoftBodyWMesh.m_collisionObject;

			m_BSoftBody.Cfg.Pressure += theValue * 100f;

			Debug.Log("Pressure: " + m_BSoftBody.Cfg.Pressure);

		}
	}
}
