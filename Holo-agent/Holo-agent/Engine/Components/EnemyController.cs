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
            
            if(attributes[1] == null)
            {
                EndDecision(DecisionOutcome.UNSUCCESSFUL);
                return;
            }

            Vector3 target = Vector3.Zero;
            if (attributes[1] is Vector3?)
            {
                target = (attributes[1] as Vector3?).Value;
            }
            else if(attributes[1] is GameObject)
            {
                target = (attributes[1] as GameObject).GlobalPosition;
            }
            Vector3 direction = (target - contr.Owner.GlobalPosition);
            direction.Normalize();
            Matrix rotation = Matrix.CreateFromQuaternion(contr.Owner.GlobalRotation);
            float x = Vector3.Dot(direction, rotation.Right);
            if(Math.Abs(x) < 0.05f) EndDecision(DecisionOutcome.SUCCESSFUL);
            Handler(0.05f * ((direction + rotation.Forward).LengthSquared() < 0.00001f ? 1 : x), 0, gameTime);
        }
    }

    delegate void ActionHandler(GameTime gameTime);
    class PerformAction : Decision
    {
        private ActionHandler Handler;

        public PerformAction(ActionHandler handler)
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
            Handler(gameTime);
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
        private GameObject weapon;
        private List<Vector3> patrolPoints = new List<Vector3>();
        private int patrolIndex = 0;

        public bool CanAttack
        {
            get { return false; }
        }

        public GameObject Weapon
        {
            get { return weapon; }
        }

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

        private void MoveForward(GameTime gameTime)
        {
            if (movement == Movement.WALK) return;
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null && rigidbody.IsGrounded)
            {
                movement = Movement.WALK;
                Owner.GetComponent<AnimationController>().PlayAnimation("walk");
            }
        }

        private void StopMoving(GameTime gameTime)
        {
            if (movement == Movement.IDLE) return;

            movement = Movement.IDLE;
            Owner.GetComponent<AnimationController>().StopAnimation("walk");
            Rigidbody rigidbody = Owner.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                Vector3 velocity = rigidbody.Velocity;
                velocity.Y = 0;
                rigidbody.AddVelocityChange(-velocity);
            }
        }

        private void Shoot(GameTime gameTime)
        {
            if (weapon != null)
            {
                Weapon weap = weapon.GetComponent<Weapon>();
                Ray(1000.0f, Owner.Scene.GetNearbyObjects(Owner), Owner.LocalToWorldMatrix.Forward);
                weap.shoot();
            }
        }
        
        private void Attack(GameTime gameTime)
        {

        }

        private void PickNextPatrolTarget(GameTime gameTime)
        {
            if(patrolPoints.Count > 0)
            {
                patrolIndex++;
                if(patrolIndex == patrolPoints.Count)
                {
                    patrolIndex = 0;
                }
                attributes[1] = (Vector3?)(patrolPoints[patrolIndex]);
            }
            else
            {
                attributes[1] = null;
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
            StopMoving(null);
            Minimap.Enemies.Remove(Owner);
            if (weapon != null) weapon.Destroy();
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

        private void LookForTarget()
        {
            List<GameObject> nearbyObjects = Owner.Scene.GetNearbyObjects(Owner);
            GameObject found = null;
            foreach (GameObject go in nearbyObjects)
            {
                Vector3 distance = go.GlobalPosition - Owner.GlobalPosition;
                if (go.IsVisible && distance.LengthSquared() < range*range)
                {
                    distance.Normalize();
                    if ((distance - Owner.LocalToWorldMatrix.Forward).LengthSquared() > 1.0f) continue;
                    if(go.Name.Equals("Player")) // Possibly temporary, but seems good
                    {
                        Ray(range, Owner.Scene.GetNearbyObjects(Owner), Vector3.Normalize(go.GlobalPosition - Owner.GlobalPosition));
                        if (found == null && ClosestObject == go)
                        {
                            found = go;
                            state = EnemyState.Alert;
                        }
                    }

                    if (go.Name.Equals("HologramPlayback"))
                    {
                        Ray(range, Owner.Scene.GetNearbyObjects(Owner), Vector3.Normalize(go.GlobalPosition - Owner.GlobalPosition));
                        if (ClosestObject == go)
                        {
                            found = go;
                            state = EnemyState.Alert;
                            break;
                        }
                    }
                }
            }

            if (found != null) attributes[1] = found;
        }

        protected override void InitializeNewOwner(GameObject newOwner)
        {
            weapon.GetComponent<Weapon>().Collision = false;
            weapon.GetComponent<Weapon>().IsArmed = true;
            weapon.IsVisible = true;
            if (weapon.Parent != newOwner)
            {
                weapon.Parent = newOwner;
                weapon.LocalScale = Vector3.One;
                weapon.LocalQuaternionRotation = Quaternion.Identity;
                weapon.LocalPosition = weapon.GetComponent<Weapon>().AsChildPosition;
            }

            base.InitializeNewOwner(newOwner);
        }

        public EnemyController(GameObject weapon, List<Vector3> patrolPoints = null, float range = 150, float shootingRange = 200, float meleeRange = 20) : base()
        {
            movement = Movement.IDLE;
            this.range = range;
            this.weapon = weapon;

            attributes.Add(this);   // EnemyController
            attributes.Add(null);   // Target
            attributes.Add(weapon);   // Weapon

            if (patrolPoints != null && patrolPoints.Count > 0)
            {
                this.patrolPoints = patrolPoints;
                attributes[1] = (Vector3?)(patrolPoints[0]);
            }

            // Decision tree
            DecisionTreeNode startNode = decisionTree.AddNode(decisionTree.root, (x => x[1] != null), null);
            {
                DecisionTreeNode targetNode = decisionTree.AddNode(startNode, (x => x[1] is GameObject), null);
                {
                    decisionTree.AddNode(targetNode,
                        (x => (Matrix.CreateFromQuaternion((x[0] as EnemyController).Owner.GlobalRotation).Forward -
                              Vector3.Normalize((x[1] as GameObject).GlobalPosition - (x[0] as EnemyController).Owner.GlobalPosition)).LengthSquared() > 0.05f),
                        new TurnToTarget(Turn));
                    DecisionTreeNode shootingNode = decisionTree.AddNode(targetNode, 
                        (x => x[2] != null && (x[2] as GameObject).GetComponent<Weapon>() != null && (x[2] as GameObject).GetComponent<Weapon>().getAmmo() > 0), 
                        null);
                    {
                        DecisionTreeNode shootingRangeNode = decisionTree.AddNode(shootingNode,
                            (x => ((x[0] as EnemyController).Owner.GlobalPosition - (x[1] as GameObject).GlobalPosition).Length() < shootingRange),
                            null);
                        {
                            decisionTree.AddNode(shootingRangeNode,
                                (x => !(x[2] as GameObject).GetComponent<Weapon>().IsLocked), new PerformAction(Shoot));
                            decisionTree.AddNode(shootingRangeNode,
                                (x => ((x[0] as EnemyController).Owner.GlobalPosition - (x[1] as GameObject).GlobalPosition).Length() < range),
                                new PerformAction(StopMoving));
                        }
                        decisionTree.AddNode(shootingNode, (x => true), new PerformAction(MoveForward));
                    }
                    DecisionTreeNode meleeNode = decisionTree.AddNode(targetNode,
                        (x => ((x[0] as EnemyController).Owner.GlobalPosition - (x[1] as GameObject).GlobalPosition).Length() < meleeRange), null);
                    {
                        decisionTree.AddNode(meleeNode, (x => (x[0] as EnemyController).CanAttack), new PerformAction(Attack));
                        decisionTree.AddNode(meleeNode, (x => true), new PerformAction(StopMoving));
                    }
                    decisionTree.AddNode(targetNode, (x => true), new PerformAction(MoveForward));
                }
                DecisionTreeNode patrolNode = decisionTree.AddNode(startNode, (x => true), null);
                {
                    decisionTree.AddNode(patrolNode, 
                        (x => ((x[0] as EnemyController).Owner.GlobalPosition - (x[1] as Vector3?).Value).Length() < meleeRange), 
                        new PerformAction(PickNextPatrolTarget));
                    decisionTree.AddNode(patrolNode,
                        (x => (Matrix.CreateFromQuaternion((x[0] as EnemyController).Owner.GlobalRotation).Forward -
                              Vector3.Normalize((x[1] as Vector3?).Value - (x[0] as EnemyController).Owner.GlobalPosition)).LengthSquared() > 0.05f), 
                        new TurnToTarget(Turn));
                    decisionTree.AddNode(patrolNode, (x => true), new PerformAction(MoveForward));
                }
            }
            decisionTree.AddNode(decisionTree.root, (x => true), new PerformAction(StopMoving));
        }
    }
}
