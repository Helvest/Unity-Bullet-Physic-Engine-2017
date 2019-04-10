using BulletUnity;
using UnityEngine;

public class ChangeParamSoftBody : MonoBehaviour
{
    private BSoftBodyWMesh bSoftBodyWMesh;
    private MeshRenderer meshRenderer;

    [SerializeField]
    private float speed = 1f;

    private void Awake()
    {
        bSoftBodyWMesh = GetComponent<BSoftBodyWMesh>();
        meshRenderer = GetComponent<MeshRenderer>();
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
            bSoftBodyWMesh.softBody.config.Pressure += theValue * 100f;

            //Debug.Log("Pressure: " + bSoftBodyWMesh.softBody.config.Pressure);
        }

        if (bSoftBodyWMesh.softBody != null)
        {
            // Debug.DrawLine(softBody.Nodes[0].Position.ToUnity(), softBody.Nodes[0].Q.ToUnity(), Color.red, 0.2f);

            BulletSharp.Math.Vector3 center;

            bSoftBodyWMesh.softBody.GetAabbCenter(out center);

            //DrawCenter(center.ToUnity());

            Debug.DrawLine(center.ToUnity(), center.ToUnity() + Vector3.up * 20, Color.red);
        }

        if (meshRenderer)
        {
            //DrawCenter(meshRenderer.bounds.center);

            Debug.DrawLine(meshRenderer.bounds.center, meshRenderer.bounds.center + Vector3.up * 20, Color.green);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            /*
            Debug.Log("Nodes[0].Q: " + softBody.Nodes[0].Q);
            Debug.Log("Nodes[0].Position: " + softBody.Nodes[0].Position);
            */

            Debug.Log("InterpolationWorldTransform: " + bSoftBodyWMesh.softBody.InterpolationWorldTransform);

            /* Debug.Log("IsVolumeValid: " + softBody.Pose.IsVolumeValid);
             Debug.Log("IsFrameValid: " + softBody.Pose.IsFrameValid);
             Debug.Log("Pose.Positions: " + softBody.Pose.Positions);
             Debug.Log("Pose.Volume: " + softBody.Pose.Volume);
             Debug.Log("Pose.Aqq: " + softBody.Pose.Aqq);
             Debug.Log("Pose.Com: " + softBody.Pose.Com);*/
        }
    }

    private void DrawCenter(Vector3 center, float size = 20)
    {
        Debug.DrawLine(center, center + Vector3.up * size, Color.green);
        Debug.DrawLine(center, center + Vector3.right * size, Color.red);
        Debug.DrawLine(center, center + Vector3.forward * size, Color.blue);
    }
}
