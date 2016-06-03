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
            Handler(0.05f * x, 0, gameTime);
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

        public EnemyController(GameObject target) : base()
        {
            attributes.Add(this);   // EnemyController
            attributes.Add(target); // Target

            // Decision tree
            decisionTree.AddNode(decisionTree.root, (x => x[1] != null), new TurnToTarget(Turn));
        }
    }
}
