using Engine.Components;
using Holo_agent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public class GameMenu
    {
        private bool isMenu, isPauseMenu, isGameOverMenu, isIntro, isCredits;
        private bool[] isButtonSelected;
        private Texture2D keypadTexture, activeButtonFrame, inactiveButtonFrame, introTexture, menuTexture, creditsTexture;
        private Texture2D titleTexture, newGameTexture, authorsTexture, quitTexture, resumeTexture, quitToMenuTexture, gameOverTexture;
        private Texture2D[] buttonFrame;
        private SpriteFont font;
        private KeyboardState currentState, oldState;
        private ButtonState currentLeftButtonState, oldLeftButtonState;
        private Rectangle titleFrame, newGameFrame, quitToMenuFrame, quitFrame, resumeFrame, keypadFrame, authorsFrame, gameOverFrame;
        private Point screen;
        private static KeypadInteraction keypad = null;
        private static bool isSelectingKeypad = false;
        private float introTimer, creditsPositionY;
        private readonly float introTime;
        private Effect introShader;
        public GameMenu()
        {
            isButtonSelected = new bool[5];
            for (int i = 0; i < isButtonSelected.Length; i++)
                isButtonSelected[i] = false;
            buttonFrame = new Texture2D[5];
            introTimer = 0.0f;
            introTime = 3.0f;
        }
        public GameState Update(GameState gameState, Game1 game, GameTime gameTime)
        {
            currentState = Keyboard.GetState();
            if (isSelectingKeypad)
                gameState = GameState.Keypad;
            if (gameState.Equals(GameState.Intro))
            {
                isIntro = true;
                introTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                introShader.Parameters["IntroTime"].SetValue(introTimer);
                if (introTimer >= introTime)
                    gameState = GameState.Menu;
            }
            else if (gameState.Equals(GameState.Menu))
            {
                if (!isMenu)
                    isMenu = true;
                if (isIntro)
                    isIntro = false;
                if (isCredits)
                    isCredits = false;
                if (isPauseMenu)
                    isPauseMenu = false;
                if (isGameOverMenu)
                    isGameOverMenu = false;
                if (isSelectingKeypad)
                    isSelectingKeypad = false;
                creditsPositionY = game.Window.ClientBounds.Height * 0.875f;
                DetectSelection(gameState);
                ChangeFrame();
                DetectClick(ref gameState, game);
            }
            else if (gameState.Equals(GameState.NewGame))
            {
                game.InitializeGame();
                game.LoadGame();
                gameState = GameState.Game;
            }
            else if (gameState.Equals(GameState.Game))
            {
                if (isMenu)
                    isMenu = false;
                if (isPauseMenu)
                    isPauseMenu = false;
                if (currentState.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape))
                    gameState = GameState.Pause;
                if (game.PlayerHealth <= 0.0f)
                    gameState = GameState.GameOver;
            }
            else if (gameState.Equals(GameState.Pause))
            {
                if (!isPauseMenu)
                    isPauseMenu = true;
                if (currentState.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape))
                    gameState = GameState.Game;
                DetectSelection(gameState);
                ChangeFrame();
                DetectClick(ref gameState, game);
            }
            else if (gameState.Equals(GameState.Keypad))
            {
                if (currentState.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape))
                {
                    isSelectingKeypad = false;
                    gameState = GameState.Game;
                }
                DetectKeypad(ref gameState, game);
            }
            else if (gameState.Equals(GameState.GameOver))
            {
                isGameOverMenu = true;
                DetectSelection(gameState);
                ChangeFrame();
                DetectClick(ref gameState, game);
            }
            else if (gameState.Equals(GameState.Credits))
            {
                isMenu = false;
                isCredits = true;
                if (currentState.IsKeyDown(Keys.Escape) && oldState.IsKeyUp(Keys.Escape))
                    gameState = GameState.Menu;
                if (creditsPositionY <= -creditsTexture.Height * 0.4875f)
                    gameState = GameState.Menu;
            }
            oldState = currentState;
            return gameState;
        }
        private void DetectKeypad(ref GameState gameState, Game1 game)
        {
            currentLeftButtonState = Mouse.GetState().LeftButton;
            if (currentLeftButtonState.Equals(ButtonState.Pressed) && oldLeftButtonState.Equals(ButtonState.Released))
            {
                int x = Mouse.GetState().X;
                int y = Mouse.GetState().Y;
                int selection = -1;
                for (int i = 0; i < 4; ++i)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        int bordX = keypadFrame.Left + (int)(10.0 / 240.0 * keypadFrame.Width) + j * (int)(80.0 / 240.0 * keypadFrame.Width);
                        int bordY = keypadFrame.Bottom - (int)(10.0 / 240.0 * keypadFrame.Width) - (3 - i) * (int)(80.0 / 240.0 * keypadFrame.Width);
                        if (x >= bordX && x < bordX + (int)(60.0 / 240.0 * keypadFrame.Width) &&
                           y >= bordY - (int)(60.0 / 240.0 * keypadFrame.Width) && y < bordY)
                        {
                            selection = 3 * i + j;
                            break;
                        }
                    }
                    if (selection > -1) break;
                }
                if (selection > -1)
                {
                    switch (selection)
                    {
                        case 0: keypad.Passcode = keypad.Passcode + '1'; break;
                        case 1: keypad.Passcode = keypad.Passcode + '2'; break;
                        case 2: keypad.Passcode = keypad.Passcode + '3'; break;
                        case 3: keypad.Passcode = keypad.Passcode + '4'; break;
                        case 4: keypad.Passcode = keypad.Passcode + '5'; break;
                        case 5: keypad.Passcode = keypad.Passcode + '6'; break;
                        case 6: keypad.Passcode = keypad.Passcode + '7'; break;
                        case 7: keypad.Passcode = keypad.Passcode + '8'; break;
                        case 8: keypad.Passcode = keypad.Passcode + '9'; break;
                        case 9: keypad.Passcode = ""; break;
                        case 10: keypad.Passcode = keypad.Passcode + '0'; break;
                        case 11:
                            {
                                if (keypad.Check())
                                {
                                    isSelectingKeypad = false;
                                    gameState = GameState.Game;
                                }
                                else keypad.Passcode = "";
                                break;
                            }
                    }
                    if (keypad.Passcode.Length > 4) keypad.Passcode = keypad.Passcode.Substring(0, 4);
                }
            }
            oldLeftButtonState = currentLeftButtonState;
        }
        public void Draw(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
        {
            screen = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            if (isMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null && buttonFrame != null)
                {
                    titleFrame = new Rectangle((int)(screen.X * 0.01f), (int)(screen.Y * 0.01f), (int)(screen.X * 0.5f), (int)(screen.Y * 0.2f));
                    newGameFrame = new Rectangle((int)(screen.X * 0.115f), (int)(screen.Y * 0.35f), (int)(screen.X * 0.3f), (int)(screen.Y * 0.125f));
                    authorsFrame = new Rectangle((int)(screen.X * 0.115f), (int)(screen.Y * 0.5f), (int)(screen.X * 0.3f), (int)(screen.Y * 0.125f));
                    quitFrame = new Rectangle((int)(screen.X * 0.115f), (int)(screen.Y * 0.65f), (int)(screen.X * 0.3f), (int)(screen.Y * 0.125f));
                    spriteBatch.Draw(menuTexture, graphics.GraphicsDevice.Viewport.Bounds, Color.White);
                    spriteBatch.Draw(titleTexture, titleFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame[0], newGameFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(newGameTexture, new Rectangle((int)(newGameFrame.X + newGameFrame.Width * 0.15f), (int)(newGameFrame.Y + newGameFrame.Height * 0.2f), (int)(newGameFrame.Width * 0.75f), (int)(newGameFrame.Height * 0.6f)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame[4], authorsFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(authorsTexture, new Rectangle((int)(authorsFrame.X + authorsFrame.Width * 0.15f), (int)(authorsFrame.Y + authorsFrame.Height * 0.2f), (int)(authorsFrame.Width * 0.75f), (int)(authorsFrame.Height * 0.6f)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame[1], quitFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(quitTexture, new Rectangle((int)(quitFrame.X + quitFrame.Width * 0.15f), (int)(quitFrame.Y + quitFrame.Height * 0.2f), (int)(quitFrame.Width * 0.75f), (int)(quitFrame.Height * 0.6f)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
            else if (isPauseMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null && buttonFrame != null)
                {
                    titleFrame = new Rectangle((int)((screen.X * 0.5f) - (screen.X * 0.25f)), (int)(screen.Y * 0.1f), (int)(screen.X * 0.5f), (int)(screen.Y * 0.2f));
                    resumeFrame = new Rectangle((int)((screen.X * 0.5f) - ((screen.X * 0.3f) * 0.5f)), (int)(screen.Y * 0.525f), (int)(screen.X * 0.3f), (int)(screen.Y * 0.125f));
                    quitToMenuFrame = new Rectangle((int)((screen.X * 0.5f) - ((screen.X * 0.3f) * 0.5f)), (int)(screen.Y * 0.675f), (int)(screen.X * 0.3f), (int)(screen.Y * 0.125f));
                    quitFrame = new Rectangle((int)((screen.X * 0.5f) - ((screen.X * 0.3f) * 0.5f)), (int)(screen.Y * 0.825f), (int)(screen.X * 0.3f), (int)(screen.Y * 0.125f));
                    spriteBatch.Draw(titleTexture, titleFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame[3], resumeFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(resumeTexture, new Rectangle((int)(resumeFrame.X + resumeFrame.Width * 0.15f), (int)(resumeFrame.Y + resumeFrame.Height * 0.2f), (int)(resumeFrame.Width * 0.75f), (int)(resumeFrame.Height * 0.6f)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame[2], quitToMenuFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(quitToMenuTexture, new Rectangle((int)(quitToMenuFrame.X + quitToMenuFrame.Width * 0.15f), (int)(quitToMenuFrame.Y + quitToMenuFrame.Height * 0.2f), (int)(quitToMenuFrame.Width * 0.75f), (int)(quitToMenuFrame.Height * 0.6f)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame[1], quitFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(quitTexture, new Rectangle((int)(quitFrame.X + quitFrame.Width * 0.15f), (int)(quitFrame.Y + quitFrame.Height * 0.2f), (int)(quitFrame.Width * 0.75f), (int)(quitFrame.Height * 0.6f)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
            else if (isSelectingKeypad)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null)
                {
                    keypadFrame = new Rectangle((int)(screen.X * 0.5f - (int)(screen.Y * 0.3f * keypadTexture.Width / keypadTexture.Height)), (int)(screen.Y * 0.2f), (int)(screen.Y * 0.6f * keypadTexture.Width / keypadTexture.Height), (int)(screen.Y * 0.6f));
                    spriteBatch.Draw(keypadTexture, keypadFrame, null, Color.White);
                    Vector2 passcodeSize = screen.Y * 0.8f * 0.000521f * font.MeasureString(keypad.Passcode);
                    spriteBatch.DrawString(font, keypad.Passcode, new Vector2(screen.X / 2 - passcodeSize.X / 2, keypadFrame.Top + 0.1f * keypadFrame.Height - passcodeSize.Y / 2), Color.Black, 0, Vector2.Zero, screen.Y * 0.8f * 0.000521f, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
            else if (isGameOverMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null && buttonFrame != null)
                {
                    gameOverFrame = new Rectangle((int)((screen.X * 0.5f) - (screen.X * 0.25f)), (int)(screen.Y * 0.1f), (int)(screen.X * 0.5f), (int)(screen.Y * 0.2f));
                    quitToMenuFrame = new Rectangle((int)((screen.X * 0.5f) - ((screen.X * 0.3f) * 0.5f)), (int)(screen.Y * 0.675f), (int)(screen.X * 0.3f), (int)(screen.Y * 0.125f));
                    quitFrame = new Rectangle((int)((screen.X * 0.5f) - ((screen.X * 0.3f) * 0.5f)), (int)(screen.Y * 0.825f), (int)(screen.X * 0.3f), (int)(screen.Y * 0.125f));
                    spriteBatch.Draw(gameOverTexture, gameOverFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame[2], quitToMenuFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(quitToMenuTexture, new Rectangle((int)(quitToMenuFrame.X + quitToMenuFrame.Width * 0.15f), (int)(quitToMenuFrame.Y + quitToMenuFrame.Height * 0.2f), (int)(quitToMenuFrame.Width * 0.75f), (int)(quitToMenuFrame.Height * 0.6f)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame[1], quitFrame, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.Draw(quitTexture, new Rectangle((int)(quitFrame.X + quitFrame.Width * 0.15f), (int)(quitFrame.Y + quitFrame.Height * 0.2f), (int)(quitFrame.Width * 0.75f), (int)(quitFrame.Height * 0.6f)), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
            else if(isIntro)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone, introShader);
                spriteBatch.Draw(introTexture, graphics.GraphicsDevice.Viewport.Bounds, Color.White);
                spriteBatch.End();
            }
            else if(isCredits)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                creditsPositionY -= 2.5f;
                spriteBatch.Draw(creditsTexture, new Rectangle(0, (int)creditsPositionY, graphics.GraphicsDevice.Viewport.Bounds.Width, (int)(creditsTexture.Height * 0.5f)), Color.White);
                spriteBatch.End();
            }
        }
        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Interface");
            keypadTexture = content.Load<Texture2D>("Textures/Keypad");
            introTexture = content.Load<Texture2D>("Textures/Intro");
            introShader = content.Load<Effect>("FX/Intro");
            introShader.Parameters["IntroTime"].SetValue(introTimer);
            introShader.Parameters["IntroTimeLimit"].SetValue(introTime);
            menuTexture = content.Load<Texture2D>("Textures/Menu");
            activeButtonFrame = content.Load<Texture2D>("Textures/ActiveButtonFrame");
            inactiveButtonFrame = content.Load<Texture2D>("Textures/InactiveButtonFrame");
            titleTexture = content.Load<Texture2D>("Textures/Holo-Agent");
            newGameTexture = content.Load<Texture2D>("Textures/NewGame");
            authorsTexture = content.Load<Texture2D>("Textures/Authors");
            quitTexture = content.Load<Texture2D>("Textures/Quit");
            resumeTexture = content.Load<Texture2D>("Textures/Resume");
            quitToMenuTexture = content.Load<Texture2D>("Textures/MainMenu");
            gameOverTexture = content.Load<Texture2D>("Textures/GameOver");
            creditsTexture = content.Load<Texture2D>("Textures/Credits");
            for (int i = 0; i < buttonFrame.Length; i++)
                buttonFrame[i] = inactiveButtonFrame;
        }
        private void DetectSelection(GameState gameState)
        {
            int x = Mouse.GetState().Position.X;
            int y = Mouse.GetState().Position.Y;
            isButtonSelected[0] = (gameState.Equals(GameState.Menu) && x > newGameFrame.Left && x < newGameFrame.Right) && (y > newGameFrame.Top && y < newGameFrame.Bottom);
            isButtonSelected[1] = ((gameState.Equals(GameState.Menu) || gameState.Equals(GameState.Pause) || gameState.Equals(GameState.GameOver)) && x > quitFrame.Left && x < quitFrame.Right) && (y > quitFrame.Top && y < quitFrame.Bottom);
            isButtonSelected[2] = ((gameState.Equals(GameState.Pause) || gameState.Equals(GameState.GameOver)) && x > quitToMenuFrame.Left && x < quitToMenuFrame.Right) && (y > quitToMenuFrame.Top && y < quitToMenuFrame.Bottom);
            isButtonSelected[3] = (gameState.Equals(GameState.Pause) && x > resumeFrame.Left && x < resumeFrame.Right) && (y > resumeFrame.Top && y < resumeFrame.Bottom);
            isButtonSelected[4] = (gameState.Equals(GameState.Menu) && x > authorsFrame.Left && x < authorsFrame.Right) && (y > authorsFrame.Top && y < authorsFrame.Bottom);
        }
        private void ChangeFrame()
        {
            for (int i = 0; i < isButtonSelected.Length; i++)
            {
                if (!isButtonSelected[i])
                    buttonFrame[i] = inactiveButtonFrame;
                else
                    buttonFrame[i] = activeButtonFrame;
            }
        }
        private void DetectClick(ref GameState gameState, Game1 game)
        {
            currentLeftButtonState = Mouse.GetState().LeftButton;
            if(currentLeftButtonState.Equals(ButtonState.Pressed) && oldLeftButtonState.Equals(ButtonState.Released))
            {
                if (isButtonSelected[0])
                    gameState = GameState.NewGame;
                if (isButtonSelected[1])
                    game.Exit();
                if (isButtonSelected[2])
                    gameState = GameState.Menu;
                if (isButtonSelected[3])
                    gameState = GameState.Game;
                if (isButtonSelected[4])
                    gameState = GameState.Credits;
            }
            oldLeftButtonState = currentLeftButtonState;
        }
        public static void ShowKeypadScreen(KeypadInteraction pad)
        {
            isSelectingKeypad = true;
            keypad = pad;
        }
    }
    public enum GameState
    {
        Menu = 0,
        NewGame = 1,
        Game = 2,
        GameOver = 3,
        Pause = 4,
        Intro = 5,
        Credits = 6,
        Keypad = 7
    }
}
