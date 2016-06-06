using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public class GameMenu
    {
        private bool isMenu;
        private bool[] isButtonSelected;
        private Texture2D buttonFrame;
        private SpriteFont font;
        private Color[] buttonColor;
        public GameMenu()
        {
            isButtonSelected = new bool[2];
            for (int i = 0; i < isButtonSelected.Length; i++)
                isButtonSelected[i] = false;
            buttonColor = new Color[2];
            for (int i = 0; i < buttonColor.Length; i++)
                buttonColor[i] = Color.Black;
        }
        public void Update(ref GameState gameState, Game game)
        {
            if (gameState == GameState.Menu)
            {
                if (!isMenu)
                    isMenu = true;
                DetectSelection();
                for (int i = 0; i < isButtonSelected.Length; i++)
                {
                    if (!isButtonSelected[i])
                        buttonColor[i] = Color.Black;
                    else
                        buttonColor[i] = Color.Red;
                }
                DetectClick(ref gameState, game);
            }
            if (gameState == GameState.GameRunning)
            {
                if (isMenu)
                    isMenu = false;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (isMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null && buttonFrame != null)
                {
                    spriteBatch.DrawString(font, "Holo-agent", new Vector2(75, 15), Color.Purple, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame, new Vector2(300, 200), null, buttonColor[0], 0, Vector2.Zero, new Vector2(0.4f, 0.4f), SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "New Game", new Vector2(303, 227.5f), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame, new Vector2(300, 350), null, buttonColor[1], 0, Vector2.Zero, new Vector2(0.4f, 0.4f), SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, "Quit", new Vector2(360, 377.5f), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
        }
        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("Textures/Arial");
            buttonFrame = content.Load<Texture2D>("Textures/Button_Frame");
        }
        private void DetectSelection()
        {
            int x = Mouse.GetState().Position.X;
            int y = Mouse.GetState().Position.Y;
            if ((x > 300 && x < 500) && (y > 200 && y < 300))
                isButtonSelected[0] = true;
            else
                isButtonSelected[0] = false;
            if ((x > 300 && x < 500) && (y > 350 && y < 450))
                isButtonSelected[1] = true;
            else
                isButtonSelected[1] = false;
        }
        private void DetectClick(ref GameState gameState, Game game)
        {
            ButtonState leftButton = Mouse.GetState().LeftButton;
            if (isButtonSelected[0] && leftButton == ButtonState.Pressed)
            {
                gameState = GameState.GameRunning;
            }
            if(isButtonSelected[1] && leftButton == ButtonState.Pressed)
            {
                game.Exit();
            }
        }
    }
    public enum GameState
    {
        Menu = 0,
        GameRunning = 1,
        Pause = 2
    }
}
