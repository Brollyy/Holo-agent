﻿using System;
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
            if(target == null)
            {
                EndDecision(DecisionOutcome.UNSUCCESSFUL);
                return;
            }
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

    public enum EnemyState
    {
        Patrolling,
        Alert,
        Combat
    }

    public class EnemyController : AIController
    {
        private EnemyState state = EnemyState.Patrolling;
        private float range;
        private GameTime lastSearch = new GameTime();

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
            Owner.GetComponent<AnimationController>().PlayAnimation("walk");
        }

        private void StopMoving()
        {
            if (movement == Movement.IDLE) return;

            movement = Movement.IDLE;
            Owner.GetComponent<AnimationController>().StopAnimation("walk");
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.AddVelocityChange(-rigidbody.Velocity);
            }
        }

        public override void DealDamage(float amount, Weapon causer)
        {
            base.DealDamage(amount, causer);

            if (causer.Owner.Parent.Name == "Player" && attributes[1] != causer.Owner.Parent)
            {
                attributes[1] = causer.Owner.Parent;
                state = EnemyState.Combat;
                decisionTree.InterruptCurrentDecision();
            }
        }

        protected override void HandleDeath()
        {
            decisionTree.InterruptCurrentDecision();
            StopMoving();
            Minimap.Enemies.Remove(Owner);
            AnimationController contr = Owner.GetComponent<AnimationController>();
            if(contr != null)
            {
                contr.StopAllAnimations(0.2f);
                contr.PlayAnimation("death", 1, 0f);
                contr.SetBindPose("death", 0.2f, 1);
            }
            List<Component> comps = Owner.GetComponents<Component>();
            foreach(Component comp in comps)
            {
                if (!(comp is AnimationController) && !(comp is MeshInstance))
                {
                    comp.Enabled = false;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(movement == Movement.WALK)
            {
                Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
                if (rigidbody != null && rigidbody.IsGrounded)
                {
                    rigidbody.AddForce(rigidbody.Mass * walkSpeed * Owner.LocalToWorldMatrix.Forward);
                }
            }
            if (state != EnemyState.Combat && gameTime.TotalGameTime.Subtract(lastSearch.TotalGameTime).TotalSeconds > 0.5)
            {
                LookForTarget();
                lastSearch.TotalGameTime = new TimeSpan(gameTime.TotalGameTime.Ticks);
            }

            base.Update(gameTime);
        }

        private GameObject Ray(float maxDistance, List<GameObject> objects, Vector3 direction)
        {
            float? closest = null;
            GameObject closestGameObject = null;
            Raycast ray = new Raycast(Owner.GlobalPosition, direction, maxDistance);
            foreach (GameObject go in objects)
            {
                if (go == Owner) continue;
                Collider col = go.GetComponent<Collider>();
                if (go.IsVisible && go.Name != "Level")
                {
                    float? distance = (col != null ? ray.Intersect(col.bound) : ray.Intersect(go.Bound));
                    if (distance != null)
                    {
                        if (closest == null || distance < closest)
                        {
                            closest = distance;
                            closestGameObject = go;
                        }
                    }
                }
            }
            return closestGameObject;
        }

        private void LookForTarget()
        {
            List<GameObject> nearbyObjects = Owner.Scene.GetNearbyObjects(Owner);
            attributes[1] = null;
            foreach(GameObject go in nearbyObjects)
            {
                Vector3 distance = go.GlobalPosition - Owner.GlobalPosition;
                if (go.IsVisible && distance.LengthSquared() < range*range)
                {
                    distance.Normalize();
                    if ((distance - Owner.LocalToWorldMatrix.Forward).LengthSquared() > 1.0f) continue;
                    if(go.Name.Equals("Player")) // Possibly temporary, but seems good
                    {
                        if (attributes[1] == null &&
                            Ray(range, Owner.Scene.GetNearbyObjects(Owner), Vector3.Normalize(go.GlobalPosition - Owner.GlobalPosition)) == go)
                        {
                            attributes[1] = go;
                            state = EnemyState.Alert;
                        }
                    }

                    if (go.Name.Equals("HologramPlayback"))
                    {
                        if (Ray(range, Owner.Scene.GetNearbyObjects(Owner), Vector3.Normalize(go.GlobalPosition - Owner.GlobalPosition)) == go)
                        {
                            attributes[1] = go;
                            state = EnemyState.Alert;
                            return;
                        }
                    }
                }
            }
        }

        public EnemyController(float range = 200) : base()
        {
            movement = Movement.IDLE;
            this.range = range;

            attributes.Add(this);   // EnemyController
            attributes.Add(null); // Target

            // Decision tree
            DecisionTreeNode targetNode = decisionTree.AddNode(decisionTree.root, (x => x[1] != null), null);
            decisionTree.AddNode(decisionTree.root, (x => true), new PerformMovement(StopMoving));
            decisionTree.AddNode(targetNode, 
                (x => (Matrix.CreateFromQuaternion((x[0] as EnemyController).Owner.GlobalRotation).Forward -
                      Vector3.Normalize((x[1] as GameObject).GlobalPosition - (x[0] as EnemyController).Owner.GlobalPosition)).LengthSquared() > 0.1f),
                new TurnToTarget(Turn));
            DecisionTreeNode moveNode = decisionTree.AddNode(targetNode, (x => true), null);
            decisionTree.AddNode(moveNode, 
                (x => ((x[0] as EnemyController).Owner.GlobalPosition - (x[1] as GameObject).GlobalPosition).Length() > 20.0f), 
                new PerformMovement(MoveForward));
            decisionTree.AddNode(moveNode, (x => true), new PerformMovement(StopMoving));
        }
    }
}
