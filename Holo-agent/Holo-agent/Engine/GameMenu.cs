using System;
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
        private bool isMenu, isPauseMenu, isGameOverMenu;
        private bool[] isButtonSelected;
        private Texture2D keypadTexture;
        private Texture2D buttonFrame;
        private SpriteFont font;
        private Color[] buttonColor;
        private KeyboardState currentState, oldState;
        private ButtonState currentLeftButtonState, oldLeftButtonState;
        private string title = "Holo-agent", newGame = "New Game", quitToMenu = "Quit To Menu", quit = "Quit Game", resume = "Resume", gameOver = "Game Over";
        private Vector2 titleSize, newGameSize, frame, quitToMenuSize, quitSize, resumeSize, gameOverSize;
        private Rectangle newGameFrame, quitToMenuFrame, quitFrame, resumeFrame, keypadFrame;
        private Point w;
        private static KeypadInteraction keypad = null;
        private static bool isSelectingKeypad = false;

        public GameMenu()
        {
            isButtonSelected = new bool[4];
            for (int i = 0; i < isButtonSelected.Length; i++)
                isButtonSelected[i] = false;
            buttonColor = new Color[4];
            for (int i = 0; i < buttonColor.Length; i++)
                buttonColor[i] = Color.Black;
        }
        public GameState Update(GameState gameState, Game1 game)
        {
            currentState = Keyboard.GetState();

            if (isSelectingKeypad) gameState = GameState.Keypad;

            if (gameState.Equals(GameState.Menu))
            {
                if (!isMenu)
                    isMenu = true;
                if (isPauseMenu)
                    isPauseMenu = false;
                if (isGameOverMenu)
                    isGameOverMenu = false;
                if (isSelectingKeypad)
                    isSelectingKeypad = false;
                DetectSelection(gameState);
                ChangeFrameColor();
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
                ChangeFrameColor();
                DetectClick(ref gameState, game);
            }
            else if(gameState.Equals(GameState.Keypad))
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
                ChangeFrameColor();
                DetectClick(ref gameState, game);
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
            w = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            if (isMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null && buttonFrame != null)
                {
                    newGameFrame = new Rectangle((int)(w.X / 2 - frame.X / 2), (int)(0.7f * w.Y - frame.Y / 2), (int)frame.X, (int)frame.Y);
                    quitFrame = new Rectangle((int)(w.X / 2 - frame.X / 2), (int)(0.9f * w.Y - frame.Y / 2), (int)frame.X, (int)frame.Y);
                    spriteBatch.DrawString(font, title, new Vector2(w.X / 2 - titleSize.X / 2, 0.1f * w.Y), Color.Purple, 0, Vector2.Zero, 0.6f, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame, newGameFrame, null, buttonColor[0], 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, newGame, new Vector2(w.X / 2 - newGameSize.X / 2, 0.7f * w.Y - newGameSize.Y / 2), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                    spriteBatch.Draw(buttonFrame, quitFrame, null, buttonColor[1], 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, quit, new Vector2(w.X / 2 - quitSize.X / 2, 0.9f * w.Y - quitSize.Y / 2), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
            else if (isPauseMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null && buttonFrame != null)
                {
                    spriteBatch.DrawString(font, title, new Vector2(w.X / 2 - titleSize.X / 2, 0.1f * w.Y), Color.Purple, 0, Vector2.Zero, 0.6f, SpriteEffects.None, 0);
                    resumeFrame = new Rectangle((int)(w.X / 2 - frame.X / 2), (int)(0.5f * w.Y - frame.Y / 2), (int)frame.X, (int)frame.Y);
                    spriteBatch.Draw(buttonFrame, resumeFrame, null, buttonColor[3], 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, resume, new Vector2(w.X / 2 - resumeSize.X / 2, 0.5f * w.Y - resumeSize.Y / 2), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                    quitToMenuFrame = new Rectangle((int)(w.X / 2 - frame.X / 2), (int)(0.7f * w.Y - frame.Y / 2), (int)frame.X, (int)frame.Y);
                    spriteBatch.Draw(buttonFrame, quitToMenuFrame, null, buttonColor[2], 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, quitToMenu, new Vector2(w.X / 2 - quitToMenuSize.X / 2, 0.7f * w.Y - quitToMenuSize.Y / 2), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                    quitFrame = new Rectangle((int)(w.X / 2 - frame.X / 2), (int)(0.9f * w.Y - frame.Y / 2), (int)frame.X, (int)frame.Y);
                    spriteBatch.Draw(buttonFrame, quitFrame, null, buttonColor[1], 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, quit, new Vector2(w.X / 2 - quitSize.X / 2, 0.9f * w.Y - quitSize.Y / 2), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
            else if (isSelectingKeypad)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null)
                {
                    keypadFrame = new Rectangle((int)(w.X * 0.5f - (int)(w.Y * 0.3f * keypadTexture.Width / keypadTexture.Height)), (int)(w.Y * 0.2f), (int)(w.Y * 0.6f * keypadTexture.Width / keypadTexture.Height), (int)(w.Y * 0.6f));
                    spriteBatch.Draw(keypadTexture, keypadFrame, null, Color.White);
                    Vector2 passcodeSize = w.Y * 0.8f * 0.000521f * font.MeasureString(keypad.Passcode);
                    spriteBatch.DrawString(font, keypad.Passcode, new Vector2(w.X / 2 - passcodeSize.X / 2, keypadFrame.Top + 0.1f * keypadFrame.Height - passcodeSize.Y / 2), Color.Black, 0, Vector2.Zero, w.Y * 0.8f * 0.000521f, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
            else if (isGameOverMenu)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                if (font != null && buttonFrame != null)
                {
                    spriteBatch.DrawString(font, gameOver, new Vector2(w.X / 2 - gameOverSize.X / 2, 0.1f * w.Y), Color.Purple, 0, Vector2.Zero, 0.6f, SpriteEffects.None, 0);
                    quitToMenuFrame = new Rectangle((int)(w.X / 2 - frame.X / 2), (int)(0.7f * w.Y - frame.Y / 2), (int)frame.X, (int)frame.Y);
                    spriteBatch.Draw(buttonFrame, quitToMenuFrame, null, buttonColor[2], 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, quitToMenu, new Vector2(w.X / 2 - quitToMenuSize.X / 2, 0.7f * w.Y - quitToMenuSize.Y / 2), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                    quitFrame = new Rectangle((int)(w.X / 2 - frame.X / 2), (int)(0.9f * w.Y - frame.Y / 2), (int)frame.X, (int)frame.Y);
                    spriteBatch.Draw(buttonFrame, quitFrame, null, buttonColor[1], 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.DrawString(font, quit, new Vector2(w.X / 2 - quitSize.X / 2, 0.9f * w.Y - quitSize.Y / 2), Color.Purple, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                }
                spriteBatch.End();
            }
        }
        public void LoadContent(ContentManager content)
        {
            font = content.Load<SpriteFont>("Font/Holo-Agent");
            buttonFrame = content.Load<Texture2D>("Textures/Button_Frame");
            keypadTexture = content.Load<Texture2D>("Textures/Keypad");
            titleSize = 0.6f * font.MeasureString(title);
            newGameSize = 0.3f * font.MeasureString(newGame);
            frame = 1.5f * newGameSize;
            quitToMenuSize = 0.3f * font.MeasureString(quitToMenu);
            quitSize = 0.3f * font.MeasureString(quit);
            resumeSize = 0.3f * font.MeasureString(resume);
            gameOverSize = 0.6f * font.MeasureString(gameOver);
        }
        private void DetectSelection(GameState gameState)
        {
            int x = Mouse.GetState().Position.X;
            int y = Mouse.GetState().Position.Y;
            isButtonSelected[0] = (gameState.Equals(GameState.Menu) && x > newGameFrame.Left && x < newGameFrame.Right) && (y > newGameFrame.Top && y < newGameFrame.Bottom);
            isButtonSelected[1] = ((gameState.Equals(GameState.Menu) || gameState.Equals(GameState.Pause) || gameState.Equals(GameState.GameOver)) && x > quitFrame.Left && x < quitFrame.Right) && (y > quitFrame.Top && y < quitFrame.Bottom);
            isButtonSelected[2] = ((gameState.Equals(GameState.Pause) || gameState.Equals(GameState.GameOver)) && x > quitToMenuFrame.Left && x < quitToMenuFrame.Right) && (y > quitToMenuFrame.Top && y < quitToMenuFrame.Bottom);
            isButtonSelected[3] = (gameState.Equals(GameState.Pause) && x > resumeFrame.Left && x < resumeFrame.Right) && (y > resumeFrame.Top && y < resumeFrame.Bottom);
        }
        private void ChangeFrameColor()
        {
            for (int i = 0; i < isButtonSelected.Length; i++)
            {
                if (!isButtonSelected[i])
                    buttonColor[i] = Color.Black;
                else
                    buttonColor[i] = Color.Red;
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
        Keypad = 5
    }
}
