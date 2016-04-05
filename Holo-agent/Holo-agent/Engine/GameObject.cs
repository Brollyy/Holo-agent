using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Engine.Components;

namespace Engine
{
    /// <summary>
    /// Class for every object on the scene.
    /// </summary>
    public class GameObject
    {

        // TODO: Implement Instantiate and Destroy static functions (maybe?).

        private Scene scene;

        public Scene Scene
        {
            get
            {
                return scene;
            }
        }

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
                // Save world transform data temporarly.
                localPosition = GlobalPosition;
                localRotation = GlobalRotation;
                localScale = GlobalScale;
                // Next, change the transform matrix.
                localToWorldMatrix = Matrix.CreateTranslation(localPosition) *
                                     Matrix.CreateFromQuaternion(localRotation) * 
                                     Matrix.CreateScale(localScale);
                // Set local transform data through global transform.
                GlobalPosition = localPosition;
                GlobalRotation = localRotation;
                GlobalScale = localScale;
                // Finally, swap the references and informs parents about the children changes.
                if(parent != null) parent.RemoveChild(this);
                parent = value;
                if(parent != null) parent.AddChild(this);
            }
        }

        /// <summary>
        /// Stores references to children of this object.
        /// </summary>
        private SortedList<string, GameObject> children;

        /// <summary>
        /// Adds child to set of this object's children. Called automatically when changing parent.
        /// </summary>
        /// <param name="child"> Object to be added to children set.</param>
        private void AddChild(GameObject child)
        {
            // TODO: Think about exceptions.
            children.Add(child.name, child);
        }

        /// <summary>
        /// Removes child from set of this object's children. Called automatically when changing parent.
        /// </summary>
        /// <param name="child"></param>
        private void RemoveChild(GameObject child)
        {
            // TODO: Think about exceptions.
            children.Remove(child.name);
        }

        /// <summary>
        /// Finds and returns child object with specified name. Returns null if no object was found.
        /// </summary>
        /// <param name="name">Name of the child object.</param>
        public GameObject GetChild(string name)
        {
            GameObject child = null;
            bool success = children.TryGetValue(name, out child);
            if (success) return child;
            else return null;
        }

        /// <summary>
        /// Returns child object at specified index. Return null if index was incorrect.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GameObject GetChild(int index)
        {
            if (index < 0 || index >= children.Count) return null;
            return children.Values[index];
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
        /// Stores local to world coordinates transform matrix.
        /// </summary>
        private Matrix localToWorldMatrix;

        /// <summary>
        /// Transform matrix used to transform points from local coordinates to global.
        /// </summary>
        public Matrix LocalToWorldMatrix
        {
            get
            {
                return localToWorldMatrix;
            }
        }

        /// <summary>
        /// Transform matrix used to transform points from global coordinates to local.
        /// </summary>
        public Matrix WorldToLocalMatrix
        {
            get
            {
                return Matrix.Invert(localToWorldMatrix);
            }
        }

        /// <summary>
        /// Global position property of the object.
        /// </summary>
        public Vector3 GlobalPosition
        {
            get
            {
                return Vector3.Transform(localPosition, LocalToWorldMatrix);
            }
            set
            {
                localPosition = Vector3.Transform(value, WorldToLocalMatrix);
            }
        }

        /// <summary>
        /// Global rotation property of the object.
        /// </summary>
        public Quaternion GlobalRotation
        {
            get
            {
                return localRotation * LocalToWorldMatrix.Rotation;
            }
            set
            {
                localRotation = value * WorldToLocalMatrix.Rotation;
            }
        }

        /// <summary>
        /// Global scale property of this object.
        /// </summary>
        public Vector3 GlobalScale
        {
            // TODO: Implement global scale conversions.
            get
            {
                return (LocalToWorldMatrix * Matrix.CreateScale(localScale)).Scale;
            }
            set
            {
                localScale = (WorldToLocalMatrix * Matrix.CreateScale(value)).Scale;
            }
        }

        /// <summary>
        /// Stores object's components.
        /// </summary>
        private List<Component> components;

        /// <summary>
        /// Adds component to the object.
        /// </summary>
        /// <param name="comp">Component to be added.</param>
        public void AddComponent(Component comp)
        {
            components.Add(comp);
            comp.GetType().GetProperty("go").SetValue(comp, this);
        }

        /// <summary>
        /// Creates new component of specified type and adds it to the object.
        /// Returns newly created component.
        /// </summary>
        /// <typeparam name="T">Type of the new component</typeparam>
        public T AddNewComponent<T>() where T : Component, new()
        {
            T comp = new T();
            components.Add(comp);
            comp.GetType().GetProperty("go").SetValue(comp, this);
            return comp;
        }

        /// <summary>
        /// Removes specified component from the object. Returns false if component was not found.
        /// </summary>
        /// <param name="comp">Component to be removed.</param>
        /// <returns></returns>
        public bool RemoveComponent(Component comp)
        {
            bool success = components.Remove(comp);
            if(success) comp.GetType().GetProperty("go").SetValue(comp, null);
            return success;
        }

        /// <summary>
        /// Finds first component of the object with specified type. Returns null if no component was found.
        /// </summary>
        /// <typeparam name="T">Type of the component to get.</typeparam>
        public T GetComponent<T>() where T : Component
        {
            return (T)components.Find(comp => comp.IsType<T>());
        }

        public List<T> GetComponents<T>() where T : Component
        {
            return components.FindAll(comp => comp.IsType<T>()).ConvertAll<T>(comp => (T)comp);
        }

        /// <summary>
        /// Default constructor for GameObject. Sets unnamed, parentless object at point (0,0,0), without any rotation and scale.
        /// </summary>
        public GameObject() : this(null, Vector3.Zero, Quaternion.Identity, Vector3.One)
        {
        }

        /// <summary>
        /// Partial constructor for GameObject. Uses the same default values except as default constructor, except it gives object name.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        public GameObject(string name) : this(name, Vector3.Zero, Quaternion.Identity, Vector3.One)
        {
        }

        /// <summary>
        /// Full constructor for GameObject.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        /// <param name="position">Local position of the object.</param>
        /// <param name="rotation">Local rotation of the object.</param>
        /// <param name="scale">Local scale of the object.</param>
        /// <param name="parent">Scene parent of the object (optional).</param>
        public GameObject(string name, Vector3 position, Quaternion rotation, Vector3 scale, GameObject parent = null)
        {
            localPosition = position;
            localRotation = rotation;
            localScale = scale;
            localToWorldMatrix = Matrix.Identity;
            this.Parent = parent;
            children = new SortedList<string, GameObject>();
            components = new List<Component>();
            this.name = name;
        }

    }
}