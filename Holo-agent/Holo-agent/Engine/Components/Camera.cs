using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Components
{
    public class Camera : Component
    {
        // TODO: Once input handling is in CharacterController, delete this.
        private float cameraSpeed;
        private float mouseSpeed;
        private MouseState currentMouseState;

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

        public override void Update(GameTime gameTime)
        {
            // TODO: This needs to be handled inside CharacterController rather than here.
            // TODO: Change this so it uses Input class rather than input mechanisms directly.
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Owner.LocalPosition += Vector3.Forward * (float)(cameraSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Owner.LocalPosition += Vector3.Backward * (float)(cameraSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Owner.LocalPosition += Vector3.Left * (float)(cameraSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Owner.LocalPosition += Vector3.Right * (float)(cameraSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                //Jump.
            }
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
                //Crouch.
            }
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                cameraSpeed = 125;
            }
            else
            {
                cameraSpeed = 75;
            }
            Vector3 rot = Owner.LocalEulerRotation;
            if (Mouse.GetState().X - currentMouseState.X < 0)
            {

                rot.X += (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                
            }
            if (Mouse.GetState().X - currentMouseState.X > 0)
            {
                rot.X -= (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Mouse.GetState().Y - currentMouseState.Y < 0)
            {
                if (rot.Y < 90)
                    rot.Y += (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            if (Mouse.GetState().Y - currentMouseState.Y > 0)
            {
                if (rot.Y < 90 || rot.Y > 270)
                    rot.Y -= (float)(mouseSpeed * gameTime.ElapsedGameTime.TotalSeconds);
            }
            Owner.LocalEulerRotation = rot;
            currentMouseState = Mouse.GetState();
        }

        public Camera() : this(90.0f, 4.0f/3.0f, 1, 100)
        {
        }

        public Camera(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, 
                                                                   nearPlaneDistance, farPlaneDistance);
            Matrix worldMatrix = Owner.LocalToWorldMatrix;
            viewMatrix = Matrix.CreateLookAt(Owner.LocalPosition, worldMatrix.Forward, worldMatrix.Up);
            mouseSpeed = 175;
            cameraSpeed = 75;
        }
    }
}
