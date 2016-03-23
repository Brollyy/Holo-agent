using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Class for every object on the scene.
    /// </summary>
    public class GameObject
    {

        // TODO: Implement Instantiate and Destroy static functions.

        /// <summary>
        /// Stores name of this object.
        /// </summary>
        private string name;

        /// <summary>
        /// Name property of this object.
        /// </summary>
        public string Name
        {
            get
            {
                if (name == null) return "null";
                return name;
            }
            set
            {
                name = value;
            }
        }

        /// <summary>
        /// Reference to scene parent of this GameObject.
        /// If null, local space is the same as global space.
        /// </summary>
        private GameObject parent;

        /// <summary>
        /// Scene parent property.
        /// Setting it will not change the transform of the object in the global space.
        /// </summary>
        public GameObject Parent
        {
            get
            {
                return parent;
            }
            set
            {
                // First set global space transform data.
                localPosition = GlobalPosition;
                localRotation = GlobalRotation;
                localScale = GlobalScale;
                // Next, use new parent global space transform to get local space again.
                // TODO: Implement that.
                // Finally, swap the references and informs parents about the children changes.
                if(parent != null) parent.RemoveChild(this);
                parent = value;
                if(parent != null) parent.AddChild(this);
            }
        }

        /// <summary>
        /// Stores references to children of this object.
        /// </summary>
        private SortedSet<GameObject> children;

        /// <summary>
        /// Adds child to set of this object's children. Called automatically when changing parent.
        /// </summary>
        /// <param name="child"> Object to be added to children set.</param>
        private void AddChild(GameObject child)
        {
            // TODO: Think about exceptions.
            children.Add(child);
        }

        /// <summary>
        /// Removes child from set of this object's children. Called automatically when changing parent.
        /// </summary>
        /// <param name="child"></param>
        private void RemoveChild(GameObject child)
        {
            // TODO: Think about exceptions.
            children.Remove(child);
        }
        
        /// <summary>
        /// Stores position of the object in local space.
        /// </summary>
        private Vector3 localPosition;
        /// <summary>
        /// Stores rotation of the object in local space.
        /// </summary>
        private Quaternion localRotation;
        /// <summary>
        /// Stores scale of the object in local space.
        /// </summary>
        private Vector3 localScale;

        /// <summary>
        /// Local position property of the object.
        /// </summary>
        public Vector3 LocalPosition
        {
            get
            {
                return localPosition;
            }
            set
            {
                localPosition = value;
            }
        }
        /// <summary>
        /// Local rotation property of the object.
        /// </summary>
        public Quaternion LocalQuaternionRotation
        {
            get
            {
                return localRotation;
            }
            set
            {
                localRotation = value;
            }
        }

        /// <summary>
        /// Local space rotation using Euler angles in yaw-pitch-roll notation.
        /// </summary>
        public Vector3 LocalEulerRotation
        {
            // TODO: implement conversion between quaternions and Euler angles.
            get;
            set;
        }

        /// <summary>
        /// Local scale property of the object.
        /// </summary>
        /// <value> 3D vector containing scale coefficients along X,Y and Z axes.</value>
        public Vector3 LocalScale
        {
            get
            {
                return localScale;
            }
            set
            {
                localScale = value;
            }
        }

        /// <summary>
        /// Global position property of the object.
        /// </summary>
        public Vector3 GlobalPosition
        {
            get
            {
                return Vector3.Add(localPosition, parent.GlobalPosition);
            }
            set
            {
                localPosition = value;
                if(parent != null)
                {
                    localPosition -= parent.GlobalPosition;
                }
            }
        }

        /// <summary>
        /// Global rotation property of the object.
        /// </summary>
        public Quaternion GlobalRotation
        {
            get
            {
                return Quaternion.Multiply(localRotation, parent.GlobalRotation);
            }
            set
            {
                localRotation = value;
                if(parent != null)
                {
                    localRotation = Quaternion.Multiply(
                                        Quaternion.Inverse(parent.GlobalRotation), 
                                        localRotation);
                }
            }
        }

        /// <summary>
        /// Global scale property of this object.
        /// </summary>
        public Vector3 GlobalScale
        {
            // TODO: Implement global scale conversions.
            get;
            set;
        }
    }
}