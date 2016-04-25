using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Engine.Components;
using System.Reflection;

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
                if(value != null)
                {
                    localToWorldMatrix = value.LocalToWorldMatrix;
                }
                else
                {
                    localToWorldMatrix = Matrix.Identity;
                }
                                     
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

        public List<GameObject> GetChildren()
        {
            return new List<GameObject>(children.Values);
        }

        public bool RemoveChild(string name)
        {
            if(children.ContainsKey(name))
            {
                children.Remove(name);
                return true;
            }
            return false;
        }

        public bool RemoveChild(int index)
        {
            if (index < 0 || index >= children.Count) return false;
            children.RemoveAt(index);
            return true;
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
                foreach(GameObject child in children.Values)
                {
                    child.UpdateLocalToWorldMatrix(LocalToWorldMatrix);
                }
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
                foreach (GameObject child in children.Values)
                {
                    child.UpdateLocalToWorldMatrix(LocalToWorldMatrix);
                }
            }
        }

        /// <summary>
        /// Local space rotation using Euler angles in yaw-pitch-roll notation.
        /// </summary>
        public Vector3 LocalEulerRotation
        {
            // TODO: implement conversion between quaternions and Euler angles.
            get
            {
                Vector3 YawPitchRoll = Vector3.Zero;
                Matrix RotationMatrix = Matrix.CreateFromQuaternion(localRotation);
                double ForwardY = -RotationMatrix.M32;
                if (ForwardY <= -1.0f)
                {
                    YawPitchRoll.Y = MathHelper.ToDegrees(-MathHelper.PiOver2);
                }
                else if (ForwardY >= 1.0f)
                {
                    YawPitchRoll.Y = MathHelper.ToDegrees(MathHelper.PiOver2);
                }
                else
                {
                    YawPitchRoll.Y = MathHelper.ToDegrees((float)Math.Asin(ForwardY));
                }

                //Gimbal lock
                if (ForwardY > 0.9999f)
                {
                    YawPitchRoll.X = 0f;
                    YawPitchRoll.Z = MathHelper.ToDegrees((float)Math.Atan2(RotationMatrix.M13, RotationMatrix.M11));
                }
                else
                {
                    YawPitchRoll.X = MathHelper.ToDegrees((float)Math.Atan2(RotationMatrix.M31, RotationMatrix.M33));
                    YawPitchRoll.Z = MathHelper.ToDegrees((float)Math.Atan2(RotationMatrix.M12, RotationMatrix.M22));
                }

                while (YawPitchRoll.X > 360) YawPitchRoll.X -= 360;
                while (YawPitchRoll.X < 0) YawPitchRoll.X += 360;
                while (YawPitchRoll.Y > 360) YawPitchRoll.Y -= 360;
                while (YawPitchRoll.Y < 0) YawPitchRoll.Y += 360;
                while (YawPitchRoll.Z > 360) YawPitchRoll.Z -= 360;
                while (YawPitchRoll.Z < 0) YawPitchRoll.Z += 360;
                return YawPitchRoll;
            }
            set
            {
                localRotation = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(value.X), 
                                                                  MathHelper.ToRadians(value.Y), 
                                                                  MathHelper.ToRadians(value.Z));
                foreach (GameObject child in children.Values)
                {
                    child.UpdateLocalToWorldMatrix(LocalToWorldMatrix);
                }
            }
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
                foreach (GameObject child in children.Values)
                {
                    child.UpdateLocalToWorldMatrix(LocalToWorldMatrix);
                }
            }
        }

        /// <summary>
        /// Stores local to world coordinates transform matrix.
        /// </summary>
        private Matrix localToWorldMatrix;

        private void UpdateLocalToWorldMatrix(Matrix parentLocalToWorldMatrix)
        {
            localToWorldMatrix = parentLocalToWorldMatrix;
            foreach(GameObject child in children.Values)
            {
                child.UpdateLocalToWorldMatrix(LocalToWorldMatrix);
            }
        }

        /// <summary>
        /// Transform matrix used to transform points from local coordinates to global.
        /// </summary>
        public Matrix LocalToWorldMatrix
        {
            get
            {
                return Matrix.CreateScale(localScale) *
                       Matrix.CreateFromQuaternion(localRotation) *
                       Matrix.CreateTranslation(localPosition) *
                       localToWorldMatrix;
            }
        }

        /// <summary>
        /// Transform matrix used to transform points from global coordinates to local.
        /// </summary>
        public Matrix WorldToLocalMatrix
        {
            get
            {
                return Matrix.Invert(LocalToWorldMatrix);
            }
        }

        /// <summary>
        /// Global position property of the object.
        /// </summary>
        public Vector3 GlobalPosition
        {
            get
            {
                return Vector3.Transform(localPosition, localToWorldMatrix);
            }
            set
            {
                localPosition = Vector3.Transform(value, Matrix.Invert(localToWorldMatrix));
                foreach (GameObject child in children.Values)
                {
                    child.UpdateLocalToWorldMatrix(LocalToWorldMatrix);
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
                return localRotation * localToWorldMatrix.Rotation;
            }
            set
            {
                localRotation = value * Matrix.Invert(localToWorldMatrix).Rotation;
                foreach (GameObject child in children.Values)
                {
                    child.UpdateLocalToWorldMatrix(LocalToWorldMatrix);
                }
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
                return (localToWorldMatrix * Matrix.CreateScale(localScale)).Scale;
            }
            set
            {
                localScale = (Matrix.Invert(localToWorldMatrix) * Matrix.CreateScale(value)).Scale;
                foreach (GameObject child in children.Values)
                {
                    child.UpdateLocalToWorldMatrix(LocalToWorldMatrix);
                }
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
            MethodInfo mi = comp.GetType().GetMethod("InitializeNewOwner", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);
            object[] args = { this };
            mi.Invoke(comp, args);
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
            MethodInfo mi = comp.GetType().GetMethod("InitializeNewOwner", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);
            object[] args = { this };
            mi.Invoke(comp, args);
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
            if (success)
            {
                FieldInfo gameObjectField = null;
                Type type = comp.GetType();
                while (type != null)
                {
                    gameObjectField = type.GetField("go", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (gameObjectField != null) break;
                    type = type.BaseType;
                }

                if(gameObjectField != null)
                {
                    gameObjectField.SetValue(comp, null);
                }
            }
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

        public void Update(GameTime gameTime)
        {
            List<Component> copyComponents = new List<Component>(components);
            foreach(Component comp in copyComponents)
            {
                if(comp.Enabled) comp.Update(gameTime);
            }

            List<GameObject> copyChildren = new List<GameObject>(children.Values);
            foreach(GameObject child in copyChildren)
            {
                child.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            foreach(Component comp in components)
            {
                if(comp.Enabled) comp.Draw(gameTime);
            }

            foreach(GameObject child in children.Values)
            {
                child.Draw(gameTime);
            }
        }

        public void Destroy()
        {
            Parent = null;

            foreach(Component comp in components)
            {
                comp.Destroy();
            }

            List<GameObject> copyChildren = new List<GameObject>(children.Values);
            foreach(GameObject child in copyChildren)
            {
                child.Destroy();
            }
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
        public GameObject(string name, Vector3 position, Quaternion rotation, Vector3 scale, Scene scene = null, GameObject parent = null)
        {
            this.name = name;
            this.scene = scene;
            if (scene != null && parent == null) scene.AddObject(this);
            components = new List<Component>();
            localToWorldMatrix = Matrix.Identity;
            children = new SortedList<string, GameObject>();
            this.Parent = parent;
            localPosition = position;
            localRotation = rotation;
            localScale = scale;
        }

    }
}