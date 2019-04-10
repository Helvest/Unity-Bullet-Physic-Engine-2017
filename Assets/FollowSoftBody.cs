using BulletUnity;
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
            bSoftBodyWMesh.softBody.AddVelocity(new BulletSharp.Math.Vector3(0, 100 * Time.fixedDeltaTime, 0));
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            bSoftBodyWMesh.softBody.AddVelocity(new BulletSharp.Math.Vector3(0, -100 * Time.fixedDeltaTime, 0));
        }
    }

}
