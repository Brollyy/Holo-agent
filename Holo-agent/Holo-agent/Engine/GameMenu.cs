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

        private string title = "Holo-agent";
        private string newGame = "New Game";
        private string quit = "Quit";
        private Vector2 titleSize;
        private Vector2 newGameSize;
        private Vector2 frame;
        private Vector2 quitSize;
        private Rectangle newGameFrame;
        private Rectangle quitFrame;
        private Point w;

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
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    game.Exit();
                    return;
                }
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
        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            if (isMenu)
            {
                Point w = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null && buttonFrame != null)
                {
                    newGameFrame = new Rectangle((int)(w.X / 2 - frame.X / 2), (int)(0.5f * w.Y - frame.Y / 2), (int)frame.X, (int)frame.Y);
                    quitFrame = new Rectangle((int)(w.X / 2 - frame.X / 2), (int)(0.7f * w.Y - frame.Y / 2), (int)frame.X, (int)frame.Y);
                    spriteBatch.DrawString(font, title, new Vector2(w.X/2 - titleSize.X/2, 0.1f*w.Y), Color.Purple, 0, Vector2.Zero, 0.6f, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame, newGameFrame, null, buttonColor[0], 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, newGame, new Vector2(w.X/2 - newGameSize.X/2, 0.5f*w.Y - newGameSize.Y/2), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame, quitFrame, null, buttonColor[1], 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, quit, new Vector2(w.X/2 - quitSize.X/2, 0.7f*w.Y - quitSize.Y/2), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
        }
        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("Textures/Arial");
            buttonFrame = content.Load<Texture2D>("Textures/Button_Frame");
            titleSize = 0.6f * font.MeasureString(title);
            newGameSize = 0.3f * font.MeasureString(newGame);
            frame = 1.5f * newGameSize;
            quitSize = 0.3f * font.MeasureString(quit);
        }
        private void DetectSelection()
        {
            int x = Mouse.GetState().Position.X;
            int y = Mouse.GetState().Position.Y;
            isButtonSelected[0] = (x > newGameFrame.Left && x < newGameFrame.Right) && (y > newGameFrame.Top && y < newGameFrame.Bottom);
            isButtonSelected[1] = (x > quitFrame.Left && x < quitFrame.Right) && (y > quitFrame.Top && y < quitFrame.Bottom);
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
