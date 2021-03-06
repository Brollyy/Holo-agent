﻿using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Engine.Components
{
    [DataContract]
    public abstract class Component
    {
        /// <summary>
        /// Stores object this component is attached to.
        /// </summary>
        [DataMember]
        private GameObject go;

        [DataMember]
        public bool Enabled
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Owner property of this component. 
        /// Used by GameObject class to manage its components' owners automatically.
        /// Null if component has no owner.
        /// </summary>
        [IgnoreDataMember]
        public GameObject Owner
        {
            get
            {
                return go;
            }
        }

        protected virtual void InitializeNewOwner(GameObject newOwner)
        {
            go = newOwner;
        }

        public bool IsType<T>() where T : Component
        {
            return this is T;
        }

        /// <summary>
        /// Draw function of this component. Override this method to implement custom drawing.
        /// </summary>
        /// <param name="gameTime">Object containing present game time.</param>
        public virtual void Draw(GameTime gameTime)
        {
        }
        /// <summary>
        /// Update function of this component. Override this method to implement custom logic.
        /// </summary>
        /// <param name="gameTime">Object containing present game time.</param>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Used to cleanup after component. Override this method to implement custom cleanup. 
        /// </summary>
        public virtual void Destroy()
        {
        }
    }
}
