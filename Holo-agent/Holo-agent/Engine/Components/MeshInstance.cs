using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Engine.Utilities;

namespace Engine.Components
{
    public class MeshInstance : Component
    {
        /// <summary>
        /// Stores model that this class wraps.
        /// </summary>
        private AnimatedModel model;

        public AnimatedModel Model
        {
            get
            {
                return model;
            }
        }

        public Vector3 Offset
        {
            get;
            set;
        }

        public override void Draw(GameTime gameTime)
        {
            model.Draw(gameTime, Owner, Offset);
        }

        public MeshInstance() : this(null as Model)
        {
        }

        public MeshInstance(MeshInstance other) : this(other.model.Model)
        {
        }

        public MeshInstance(Model model)
        {
            this.model = new AnimatedModel(model);
            foreach(ModelMesh mesh in model.Meshes)
            {
                foreach(Effect effect in mesh.Effects)
                {
                    if (effect is BasicEffect)
                    {
                        (effect as BasicEffect).EnableDefaultLighting();
                    }
                    if(effect is SkinnedEffect)
                    {
                        (effect as SkinnedEffect).EnableDefaultLighting();
                    }
                }
            }
            Offset = Vector3.Zero;
        }
    }
}
