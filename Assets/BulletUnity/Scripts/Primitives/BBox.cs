using UnityEngine;

namespace BulletUnity.Primitives
{
	/// <summary>
	/// Basic BBox
	/// </summary>
	[RequireComponent(typeof(BRigidBody))]
	[RequireComponent(typeof(BBoxShape))]
	public class BBox : BPrimitive
	{
		private const string stringName = "BBox";

		public BBoxMeshSettings meshSettings = new BBoxMeshSettings();

		public static GameObject CreateNew(Vector3 position, Quaternion rotation)
		{
			GameObject go = new GameObject(stringName);

			BBox bBox = go.AddComponent<BBox>();

			CreateNewBase(go, position, rotation);
			bBox.BuildMesh();

			return go;
		}

		public override void BuildMesh()
		{
			GetComponent<MeshFilter>().sharedMesh = meshSettings.Build();
			GetComponent<BBoxShape>().Extents = meshSettings.extents / 2f;
		}
	}
}
