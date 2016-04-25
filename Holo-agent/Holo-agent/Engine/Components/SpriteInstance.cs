using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Engine.Components
{
    public class SpriteInstance : Component
    {
        private Texture2D texture;
        private VertexPositionTexture[] spriteVerts;
        private BasicEffect spriteEffect;
        private Vector3 spriteCoordinates;
        private int tilesNumber;
        private GraphicsDeviceManager graphics;
        public SpriteInstance(Texture2D texture, Vector3 spriteCoordinates, int tilesNumber, BasicEffect spriteEffect, GraphicsDeviceManager graphics)
        {
            this.texture = texture;
            this.spriteCoordinates = spriteCoordinates;
            this.tilesNumber = tilesNumber;
            this.spriteEffect = spriteEffect;
            spriteVerts = new VertexPositionTexture[6];
            this.graphics = graphics;
        }
        public override void Draw(GameTime gameTime)
        {
            spriteVerts[0].Position = new Vector3(-spriteCoordinates.X, -spriteCoordinates.Y, -spriteCoordinates.Z);
            spriteVerts[1].Position = new Vector3(-spriteCoordinates.X, -spriteCoordinates.Y, spriteCoordinates.Z);
            spriteVerts[2].Position = new Vector3(spriteCoordinates.X, spriteCoordinates.Y, -spriteCoordinates.Z);
            spriteVerts[3].Position = spriteVerts[1].Position;
            spriteVerts[4].Position = new Vector3(spriteCoordinates.X, spriteCoordinates.Y, spriteCoordinates.Z);
            spriteVerts[5].Position = spriteVerts[2].Position;
            spriteVerts[0].TextureCoordinate = new Vector2(0, 0);
            spriteVerts[1].TextureCoordinate = new Vector2(0, tilesNumber);
            spriteVerts[2].TextureCoordinate = new Vector2(tilesNumber, 0);
            spriteVerts[3].TextureCoordinate = spriteVerts[1].TextureCoordinate;
            spriteVerts[4].TextureCoordinate = new Vector2(tilesNumber, tilesNumber);
            spriteVerts[5].TextureCoordinate = spriteVerts[2].TextureCoordinate;
            spriteEffect.Projection = Owner.Scene.Camera.GetComponent<Camera>().ProjectionMatrix;
            spriteEffect.View = Owner.Scene.Camera.GetComponent<Camera>().ViewMatrix;
            spriteEffect.World = Owner.LocalToWorldMatrix;
            spriteEffect.TextureEnabled = true;
            spriteEffect.Texture = texture;
            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, spriteVerts, 0, 2);
            }
        }
    }
}
