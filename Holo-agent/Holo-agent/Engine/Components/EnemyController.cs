using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Engine.Utilities;

namespace Engine.Components
{
    delegate void Turn(float x, float y, GameTime gameTime);
    class TurnToTarget : Decision
    {
        private Turn Handler;

        public TurnToTarget(Turn handler)
        {
            Handler = handler;
        }

        public override void Update(GameTime gameTime, ref List<object> attributes)
        {
            EnemyController contr = attributes[0] as EnemyController;
            if(contr == null || contr.Owner == null)
            {
                EndDecision(DecisionOutcome.UNSUCCESSFUL);
                return;
            }
            GameObject target = attributes[1] as GameObject;
            Vector3 direction = (target.GlobalPosition - contr.Owner.GlobalPosition);
            direction.Normalize();
            Matrix rotation = Matrix.CreateFromQuaternion(contr.Owner.GlobalRotation);
            float x = Vector3.Dot(direction, rotation.Right);
            if(Math.Abs(x) < 0.1f) EndDecision(DecisionOutcome.SUCCESSFUL);
            Handler(0.05f * x, 0, gameTime);
        }
    }

    delegate void MovementHandler();
    class PerformMovement : Decision
    {
        private MovementHandler Handler;

        public PerformMovement(MovementHandler handler)
        {
            Handler = handler;
        }

        public override void Update(GameTime gameTime, ref List<object> attributes)
        {
            EnemyController contr = attributes[0] as EnemyController;
            if (contr == null || contr.Owner == null)
            {
                EndDecision(DecisionOutcome.UNSUCCESSFUL);
                return;
            }
            Handler();
            EndDecision(DecisionOutcome.SUCCESSFUL);
        }
    }

    public class EnemyController : AIController
    {
        private void Turn(float xMove, float yMove, GameTime gameTime)
        {
            Vector3 rot = Owner.LocalEulerRotation;
            rot.X -= (float)(20.0 * xMove * gameTime.ElapsedGameTime.TotalMilliseconds);
            if (rot.Y < 75f || rot.Y > 285f)
            {
                float newRot = rot.Y - (float)(20.0 * yMove * gameTime.ElapsedGameTime.TotalMilliseconds);
                if (newRot < 75f || newRot > 285f)
                    rot.Y = newRot;
            }
            Owner.LocalEulerRotation = Vector3.Lerp(Owner.LocalEulerRotation, rot, 0.75f);
        }

        private void MoveForward()
        {
            if (movement == Movement.WALK) return;
            
            movement = Movement.WALK;
            Owner.GetComponent<AnimationController>().Blend("run", 0.2f);
        }

        private void StopMoving()
        {
            if (movement == Movement.IDLE) return;

            movement = Movement.IDLE;
            Owner.GetComponent<AnimationController>().Blend("idle", 0.2f);
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null)
                rigidbody.AddForce(Vector3.Zero, 0, rigidbody.Velocity.Y, 0);
        }

        public override void Update(GameTime gameTime)
        {
            if(movement == Movement.WALK)
            {
                Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
                if (rigidbody != null && rigidbody.isGrounded())
                {
                    Vector3 initialVelocity = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.ToRadians(Owner.LocalEulerRotation.X))) * (float)(walkSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                    rigidbody.AddForce(Vector3.Zero, initialVelocity.X, initialVelocity.Y, initialVelocity.Z);
                }
            }

            base.Update(gameTime);
        }

        public EnemyController(GameObject target) : base()
        {
            movement = Movement.IDLE;

            attributes.Add(this);   // EnemyController
            attributes.Add(target); // Target

            // Decision tree
            DecisionTreeNode targetNode = decisionTree.AddNode(decisionTree.root, (x => x[1] != null), null);
            decisionTree.AddNode(targetNode, 
                (x => (Matrix.CreateFromQuaternion((x[0] as EnemyController).Owner.GlobalRotation).Forward -
                      Vector3.Normalize((x[1] as GameObject).GlobalPosition - (x[0] as EnemyController).Owner.GlobalPosition)).LengthSquared() > 0.1f),
                new TurnToTarget(Turn));
            DecisionTreeNode moveNode = decisionTree.AddNode(targetNode, (x => true), null);
            decisionTree.AddNode(moveNode, 
                (x => ((x[0] as EnemyController).Owner.GlobalPosition - (x[1] as GameObject).GlobalPosition).Length() > 10.0f), 
                new PerformMovement(MoveForward));
            decisionTree.AddNode(moveNode, (x => true), new PerformMovement(StopMoving));
        }
    }
}
