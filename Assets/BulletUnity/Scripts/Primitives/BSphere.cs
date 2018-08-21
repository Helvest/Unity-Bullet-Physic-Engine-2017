using UnityEngine;

namespace BulletUnity.Primitives
{
	/// <summary>
	/// Basic BSphere
	/// </summary>
	[RequireComponent(typeof(BRigidBody))]
	[RequireComponent(typeof(BSphereShape))]
	public class BSphere : BPrimitive
	{
		private const string nameString = "BSphere";

		public BSphereMeshSettings meshSettings = new BSphereMeshSettings();

		public static GameObject CreateNew(Vector3 position, Quaternion rotation)
		{
			GameObject go = new GameObject(nameString);

			go.AddComponent<BSphereShape>();

			BSphere bSphere = go.AddComponent<BSphere>();

			CreateNewBase(go, position, rotation);

			bSphere.BuildMesh();

			return go;
		}

		public override void BuildMesh()
		{
			GetComponent<MeshFilter>().sharedMesh = meshSettings.Build();
			GetComponent<BSphereShape>().Radius = meshSettings.radius;
		}
	}
}
