using BulletSharp.SoftBody;
using System;
using UnityEngine;

namespace BulletUnity
{
    public class BSoftBody : BCollisionObject, IDisposable
    {
        public SoftBody softBody { get; protected set; }

        //common Soft body settings class used for all softbodies, parameters set based on type of soft body
        [SerializeField]
        private SBSettings _softBodySettings = new SBSettings();      //SoftBodyEditor will display this when needed
        public SBSettings SoftBodySettings
        {
            get { return _softBodySettings; }
            set { _softBodySettings = value; }
        }

        private SoftRigidDynamicsWorld _world;
        protected SoftRigidDynamicsWorld World
        {
            get
            {
                if (_world != null)
                {
                    return _world;
                }
                else
                {
					_world = GetWorld()?.SoftWorld;
					return _world;
                }
            }
        }

        //for converting to/from unity mesh
        public Vector3[] verts { get; protected set; } = new Vector3[0];
        public Vector3[] norms { get; protected set; } = new Vector3[0];

        protected int[] tris = new int[1];

        protected override void Awake()
        {
			WorldID = WorldsManager.ActualWorldID;
            //disable warning
        }

        protected override void AddObjectToBulletWorld()
        {
            GetWorld().AddSoftBody(this);
        }

        protected override void RemoveObjectFromBulletWorld()
        {
            WorldController world = GetWorld();
            if (isInWorld)
            {
                world?.RemoveSoftBody(softBody);
            }
        }

        public void BuildSoftBody()
        {
            _BuildCollisionObject();
        }

        protected override void Dispose(bool isdisposing)
        {
            if (softBody != null)
            {
                if (isInWorld && isdisposing)
                {
                    World.RemoveSoftBody(softBody);
                }

                softBody.Dispose();
                softBody = null;
            }
        }

        public void DumpDataFromBullet()
        {
            if (isInWorld)
            {
                SoftBody m_BSoftBody = (SoftBody)collisionObject;
                if (verts.Length != m_BSoftBody.Nodes.Count)
                {
                    verts = new Vector3[m_BSoftBody.Nodes.Count];
                }

                if (norms.Length != verts.Length)
                {
                    norms = new Vector3[m_BSoftBody.Nodes.Count];
                }

                for (int i = 0; i < m_BSoftBody.Nodes.Count; i++)
                {
                    verts[i] = m_BSoftBody.Nodes[i].Position.ToUnity();
                    norms[i] = m_BSoftBody.Nodes[i].Normal.ToUnity();
                }
            }
        }

        public virtual void Update()
        {
            DumpDataFromBullet(); //Get Bullet data
            UpdateMesh(); //Update mesh based on bullet data
        }

        /// <summary>
        /// Update Mesh (or line renderer) at runtime, call from Update 
        /// </summary>
        public virtual void UpdateMesh() { }

    }
}
