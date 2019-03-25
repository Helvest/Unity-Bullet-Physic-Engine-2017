using BulletSharp.SoftBody;
using BulletUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeParamSoftBody : MonoBehaviour
{
    private BSoftBodyWMesh bSoftBodyWMesh;
    private SoftBody softBody;

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
            softBody = bSoftBodyWMesh.softBody;

            softBody.config.Pressure += theValue * 100f;

            Debug.Log("Pressure: " + softBody.config.Pressure);
        }


        softBody = bSoftBodyWMesh.softBody;

        if (softBody != null)
        {
           // Debug.DrawLine(softBody.Nodes[0].Position.ToUnity(), softBody.Nodes[0].Q.ToUnity(), Color.red, 0.2f);

            BulletSharp.Math.Vector3 aabbMin, aabbMax;

            softBody.GetAabb(out aabbMin, out aabbMax);

            Vector3 centre = ((aabbMin + aabbMax) / 2).ToUnity();

            Debug.DrawLine(centre, centre + Vector3.up * 20, Color.green);
            Debug.DrawLine(centre, centre + Vector3.right * 20, Color.red);
            Debug.DrawLine(centre, centre + Vector3.forward * 20, Color.blue);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {



            /*
            Debug.Log("Nodes[0].Q: " + softBody.Nodes[0].Q);
            Debug.Log("Nodes[0].Position: " + softBody.Nodes[0].Position);
            */

           

           
            Debug.Log("InterpolationWorldTransform: " + softBody.InterpolationWorldTransform);




            /* Debug.Log("IsVolumeValid: " + softBody.Pose.IsVolumeValid);
             Debug.Log("IsFrameValid: " + softBody.Pose.IsFrameValid);
             Debug.Log("Pose.Positions: " + softBody.Pose.Positions);
             Debug.Log("Pose.Volume: " + softBody.Pose.Volume);
             Debug.Log("Pose.Aqq: " + softBody.Pose.Aqq);
             Debug.Log("Pose.Com: " + softBody.Pose.Com);*/

        }
    }
}
