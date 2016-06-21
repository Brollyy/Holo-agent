using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Engine.Components
{
    [DataContract]
    public class Camera : Component
    {
        [DataMember]
        private Matrix projectionMatrix;
        public Matrix ProjectionMatrix
        {
            get
            {
                return projectionMatrix;
            }
        }
        [DataMember]
        private Matrix viewMatrix;
        public Matrix ViewMatrix
        {
            get
            {
                return viewMatrix;
            }
        }
        protected override void InitializeNewOwner(GameObject newOwner)
        {
            base.InitializeNewOwner(newOwner);
            Matrix worldMatrix = newOwner.LocalToWorldMatrix;
            viewMatrix = Matrix.CreateLookAt(newOwner.GlobalPosition, newOwner.GlobalPosition + worldMatrix.Forward, worldMatrix.Up);
        }
        public override void Update(GameTime gameTime)
        {
            Matrix worldMatrix = Owner.LocalToWorldMatrix;
            viewMatrix = Matrix.CreateLookAt(Owner.GlobalPosition, Owner.GlobalPosition + worldMatrix.Forward, worldMatrix.Up);
        }
        public Camera() : this(45.0f, 4.0f / 3.0f, 1, 1000)
        {
        }
        public Camera(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fieldOfView), aspectRatio, nearPlaneDistance, farPlaneDistance);
            Matrix worldMatrix = Matrix.Identity;
            viewMatrix = Matrix.CreateLookAt(Vector3.Zero, worldMatrix.Forward, worldMatrix.Up);
        }
    }
}