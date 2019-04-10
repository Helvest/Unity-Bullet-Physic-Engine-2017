using BulletSharp;
using UnityEngine;

namespace BulletUnity
{
	public abstract class BCollisionCallbacks : MonoBehaviour, BCollisionObject.BICollisionCallbackEventHandler
	{
		public int WorldID { get; protected set; } = -1;

		public WorldController GetWorld()
		{
			return WorldsManager.GetWorldController(WorldID);
		}

		protected virtual void Awake()
		{	
			WorldID = WorldsManager.ActualWorldID;
		}

		private void OnEnable()
		{
			BCollisionObject co = GetComponent<BCollisionObject>();

			if (co != null)
			{
				co.AddOnCollisionCallbackEventHandler(this);
			}
		}

		private void OnDisable()
		{
			BCollisionObject co = GetComponent<BCollisionObject>();
			if (co != null)
			{
				co.RemoveOnCollisionCallbackEventHandler();
			}
		}

		public abstract void OnFinishedVisitingManifolds();

		public abstract void OnVisitPersistentManifold(PersistentManifold pm);
	}
}
