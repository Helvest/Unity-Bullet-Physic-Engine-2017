using BulletSharp.SoftBody;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSoftBody : MonoBehaviour
{
    public Transform transformWhoFollow;

    private BSoftBodyWMesh bSoftBodyWMesh;

    private void Awake()
    {
        bSoftBodyWMesh = GetComponent<BSoftBodyWMesh>();
    }

    private void FixedUpdate()
    {
        //BulletSharp.Math.Matrix matrix = bSoftBodyWMesh.m_collisionObject.WorldTransform;

        //Debug.Log(new Vector3(matrix.M41, matrix.M42, matrix.M43));

        SoftBody m_BSoftBody = (SoftBody)bSoftBodyWMesh.m_collisionObject;

       

        if (bSoftBodyWMesh.verts.Length > 0)
        {
           // Debug.Log(bSoftBodyWMesh.verts[0]);

            if (transformWhoFollow)
            {
                transformWhoFollow.position = bSoftBodyWMesh.verts[0];
            }

        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            m_BSoftBody.AddVelocity(new BulletSharp.Math.Vector3(0, 100 * Time.fixedDeltaTime, 0));
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            m_BSoftBody.AddVelocity(new BulletSharp.Math.Vector3(0, -100 * Time.fixedDeltaTime, 0));
        }
    }

}
