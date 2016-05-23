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
        private BasicEffect effect;
        private Vector3 coordinates;
        private int tilesNumber;
        private float alpha;
        private GraphicsDeviceManager graphics;
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;
            }
        }
        public SpriteInstance(Texture2D texture, Vector3 coordinates, int tilesNumber, float alpha, GraphicsDeviceManager graphics)
        {
            this.texture = texture;
            this.coordinates = coordinates;
            this.tilesNumber = tilesNumber;
            spriteVerts = new VertexPositionTexture[6];
            this.alpha = alpha;
            this.graphics = graphics;
            effect = new BasicEffect(this.graphics.GraphicsDevice);
        }
        public SpriteInstance(SpriteInstance sprite)
        {
            texture = sprite.texture;
            coordinates = sprite.coordinates;
            tilesNumber = sprite.tilesNumber;
            spriteVerts = new VertexPositionTexture[6];
            alpha = sprite.alpha;
            graphics = sprite.graphics;
            effect = new BasicEffect(graphics.GraphicsDevice);
        }
        public override void Draw(GameTime gameTime)
        {
            spriteVerts[0].Position = new Vector3(-coordinates.X, -coordinates.Y, -coordinates.Z);
            spriteVerts[1].Position = new Vector3(-coordinates.X, -coordinates.Y, coordinates.Z);
            spriteVerts[2].Position = new Vector3(coordinates.X, coordinates.Y, -coordinates.Z);
            spriteVerts[3].Position = spriteVerts[1].Position;
            spriteVerts[4].Position = new Vector3(coordinates.X, coordinates.Y, coordinates.Z);
            spriteVerts[5].Position = spriteVerts[2].Position;
            spriteVerts[0].TextureCoordinate = new Vector2(0, 0);
            spriteVerts[1].TextureCoordinate = new Vector2(0, tilesNumber);
            spriteVerts[2].TextureCoordinate = new Vector2(tilesNumber, 0);
            spriteVerts[3].TextureCoordinate = spriteVerts[1].TextureCoordinate;
            spriteVerts[4].TextureCoordinate = new Vector2(tilesNumber, tilesNumber);
            spriteVerts[5].TextureCoordinate = spriteVerts[2].TextureCoordinate;
            effect.Projection = Owner.Scene.Camera.GetComponent<Camera>().ProjectionMatrix;
            effect.View = Owner.Scene.Camera.GetComponent<Camera>().ViewMatrix;
            effect.World = Owner.LocalToWorldMatrix;
            effect.TextureEnabled = true;
            effect.Texture = texture;
            effect.Alpha = alpha;
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, spriteVerts, 0, 2);
            }
        }
    }
}
