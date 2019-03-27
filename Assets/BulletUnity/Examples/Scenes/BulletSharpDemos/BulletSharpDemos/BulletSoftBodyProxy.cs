using BulletSharp.SoftBody;
using BulletUnity;
using UnityEngine;

public class BulletSoftBodyProxy : MonoBehaviour
{
	public SoftBody target;
	public Vector3[] verts;
	public Vector3[] norms;
	public int[] tris;
	public Mesh mesh;

	private void Awake()
	{
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
	}

	private void OnDestroy()
	{
		Destroy(mesh);
	}

	private static int indexer = 0;

	private void Update()
	{
		if (verts.Length != target.Nodes.Count)
		{
			verts = new Vector3[target.Nodes.Count];
		}

		if (norms.Length != target.Nodes.Count)
		{
			norms = new Vector3[target.Nodes.Count];
		}

		for (indexer = 0; indexer < target.Nodes.Count; indexer++)
		{
			verts[indexer] = target.Nodes[indexer].Position.ToUnity();
			norms[indexer] = target.Nodes[indexer].Normal.ToUnity();
		}

		mesh.vertices = verts;
		mesh.normals = norms;
		mesh.triangles = tris;
		mesh.RecalculateBounds();
	}
}
