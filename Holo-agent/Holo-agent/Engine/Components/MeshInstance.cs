using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Engine.Components
{
    public class MeshInstance : Component
    {
        /// <summary>
        /// Stores model that this class wraps.
        /// </summary>
        private Model model;

        public Model Model
        {
            get
            {
                return model;
            }
        }

        private Dictionary<ModelMesh, Texture2D> textures;
        private Texture2D globalTexture;

        public void AddTexture(Texture2D texture, ModelMesh mesh)
        {
            textures.Add(mesh, texture);
        }

        public bool RemoveTexture(ModelMesh mesh)
        {
            return textures.Remove(mesh);
        }

        public Texture2D GlobalTexture
        {
            get
            {
                return globalTexture;
            }
            set
            {
                globalTexture = value;
            }
        }

        protected override void InitializeNewOwner(GameObject newOwner)
        {
            base.InitializeNewOwner(newOwner);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.Projection = newOwner.Scene.Camera.GetComponent<Camera>().ProjectionMatrix;
                    effect.View = newOwner.Scene.Camera.GetComponent<Camera>().ViewMatrix;
                    effect.World = newOwner.LocalToWorldMatrix;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = Owner.Scene.Camera.GetComponent<Camera>().ViewMatrix;
                    effect.World = Owner.LocalToWorldMatrix;
                }
                mesh.Draw();
            }
        }

        private void UpdateTextures()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (globalTexture != null || textures.ContainsKey(mesh))
                    {
                        effect.TextureEnabled = true;
                        if (globalTexture != null) effect.Texture = globalTexture;
                        else effect.Texture = textures[mesh];
                    }
                    else
                    {
                        effect.TextureEnabled = false;
                    }
                }
            }
        }

        public MeshInstance() : this(null, null)
        {
        }

        public MeshInstance(MeshInstance other) : this(other.model, other.globalTexture)
        {
        }

        public MeshInstance(Model model, Texture2D globalTexture)
        {
            this.model = model;
            foreach(ModelMesh mesh in model.Meshes)
            {
                foreach(BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                }
            }
            GlobalTexture = globalTexture;
            textures = new Dictionary<ModelMesh, Texture2D>();
        }
    }
}
