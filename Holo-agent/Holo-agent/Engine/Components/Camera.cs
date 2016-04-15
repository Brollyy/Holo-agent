using Microsoft.Xna.Framework;

namespace Engine.Components
{
    public class Camera : Component
    {
        // TODO: Once input handling is in CharacterController, delete this.
        private float cameraSpeed;
        private float mouseSpeed;
        private Matrix projectionMatrix;
        public Matrix ProjectionMatrix
        {
            get
            {
                return projectionMatrix;
            }
        }
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
            viewMatrix = Matrix.CreateLookAt(newOwner.LocalPosition, newOwner.LocalPosition + worldMatrix.Forward, worldMatrix.Up);
        }
        public override void Update(GameTime gameTime)
        {
            // TODO: This needs to be handled inside CharacterController rather than here.
            // TODO: Change this so it uses Input class rather than input mechanisms directly.
            if (Input.KEY_STATE.IsKeyDown(Input.MOVE_FORWARD))
            {
                Owner.LocalPosition += Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                       (float)(cameraSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.KEY_STATE.IsKeyDown(Input.MOVE_BACKWARD))
            {
                Owner.LocalPosition += Vector3.Transform(Vector3.Backward, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                       (float)(cameraSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.KEY_STATE.IsKeyDown(Input.STRAFE_LEFT))
            {
                Owner.LocalPosition += Vector3.Transform(Vector3.Left, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                       (float)(cameraSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.KEY_STATE.IsKeyDown(Input.STRAFE_RIGHT))
            {
                Owner.LocalPosition += Vector3.Transform(Vector3.Right, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) *
                                       (float)(cameraSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.KEY_STATE.IsKeyDown(Input.JUMP))
            {
                //Jump.
            }
            if (Input.KEY_STATE.IsKeyDown(Input.CROUCH))
            {
                //Crouch.
            }
            if (Input.KEY_STATE.IsKeyDown(Input.RUN))
            {
                cameraSpeed = 125;
            }
            else
            {
                cameraSpeed = 75;
            }
            Vector3 rot = Owner.LocalEulerRotation;
            if (Input.MOUSE_AXIS_X < 0)
            {
                rot.X += (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.MOUSE_AXIS_X > 0)
            {
                rot.X -= (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Input.MOUSE_AXIS_Y < 0)
            {
                if (rot.Y < 75f || rot.Y > 285f)
                {
                    float newRot = rot.Y + (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                    if (newRot < 75f || newRot > 285f)
                        rot.Y += (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                }
            }
            if (Input.MOUSE_AXIS_Y > 0)
            {
                if (rot.Y < 75f || rot.Y > 285f)
                {
                    float newRot = rot.Y - (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                    if (newRot < 75f || newRot > 285f)
                        rot.Y -= (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                }
            }
            Owner.LocalEulerRotation = Vector3.Lerp(Owner.LocalEulerRotation, rot, 0.75f);
            Matrix worldMatrix = Owner.LocalToWorldMatrix;
            viewMatrix = Matrix.CreateLookAt(Owner.LocalPosition, Owner.LocalPosition + worldMatrix.Forward, worldMatrix.Up);
        }
        public Camera() : this(45.0f, 4.0f / 3.0f, 1, 1000)
        {
        }
        public Camera(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fieldOfView), aspectRatio, nearPlaneDistance, farPlaneDistance);
            Matrix worldMatrix = Matrix.Identity;
            viewMatrix = Matrix.CreateLookAt(Vector3.Zero, worldMatrix.Forward, worldMatrix.Up);
            mouseSpeed = 200;
            cameraSpeed = 75;
        }
    }
}