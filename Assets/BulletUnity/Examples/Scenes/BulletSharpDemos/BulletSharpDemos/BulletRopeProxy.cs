using BulletSharp.SoftBody;
using UnityEngine;

public class BulletRopeProxy : MonoBehaviour
{
	public SoftBody target;
	private int numVerts = 0;
	public float[] linkVerts = new float[0];
	private LineRenderer line;

	private void Awake()
	{
		line = GetComponent<LineRenderer>();
	}

	private static Vector3 tempVector3 = Vector3.zero;
	private static int index = 0;
	private static int idx = 0;
	private void Update()
	{
		target.GetLinkVertexData(ref linkVerts);

		if (numVerts != linkVerts.Length / 6)
		{
			numVerts = linkVerts.Length / 6 + 1;
			line.positionCount = numVerts;
		}

		if (linkVerts.Length > 0)
		{
			tempVector3.Set(linkVerts[0], linkVerts[1], linkVerts[2]);

			//link verts are in pairs marking the ends of the links.
			line.SetPosition(0, tempVector3);

			for (index = 0; index < numVerts - 1; index++)
			{
				idx = (index * 2 + 1) * 3;

				tempVector3.Set(linkVerts[idx], linkVerts[idx + 1], linkVerts[idx + 2]);

				line.SetPosition(index + 1, tempVector3);
			}
		}
	}
}
