using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace mono_newton_directx {
    public class GameRoot : Game {
        public static GameRoot Instance { get; private set; }
        public static Viewport Viewport { get { return Instance.GraphicsDevice.Viewport; } }
        public static Vector2 ScreenSize { get { return new Vector2(Viewport.Width, Viewport.Height); } }
        public static Texture2D Pixel { get; private set; }
        public static SpriteFont DebugFont { get; private set; }
        public static int pixelSize = 8;
        public static List<Vector2> polynomial = new List<Vector2> {
            new Vector2(1, 5),
            new Vector2(-1, 0)
        };
        public static List<Complex> solutions = new List<Complex> {
            new Complex(1, 0),
            new Complex(-0.809, -0.588),
            new Complex(0.309, 0.951),
            new Complex(0.309, -0.951),
            new Complex(-0.809, 0.588)
        };
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public GameRoot() {
            Instance = this;
            graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = 1366,
                PreferredBackBufferHeight = 768
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        protected override void Initialize() {
            base.Initialize();
        }
        protected override void LoadContent() {
            Pixel = Content.Load<Texture2D>("pixel");
            DebugFont = Content.Load<SpriteFont>("DebugFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        protected override void Update(GameTime gameTime) {
            if(Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            Input.Update();
            Camera.Update();
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            spriteBatch.Begin();
            for(int y = 0; y < ScreenSize.Y; y += pixelSize) {
                for(int x = 0; x < ScreenSize.X; x += pixelSize) {
                    Vector2 coords = Camera.screen_to_world_pos(new Vector2(x, y));
                    Complex coordsComplex = new Complex(coords.X, coords.Y);
                    Complex newtonCoordsComplex = coordsComplex;
                    for(int i = 0; i <= 5; i++) {
                        Complex polySum = Complex.Zero;
                        Complex derSum = Complex.Zero;
                        foreach(var term in polynomial) {
                            polySum += term.X * Complex.Pow(newtonCoordsComplex, term.Y);
                            derSum += term.X * term.Y * Complex.Pow(newtonCoordsComplex, term.Y - 1);
                        }
                        newtonCoordsComplex = newtonCoordsComplex - polySum / derSum;
                    }
                    Vector2 newtonCoords = new Vector2((float)newtonCoordsComplex.Real, (float)newtonCoordsComplex.Imaginary);
                    Complex bestSol = Complex.Zero;
                    float bestDist = float.PositiveInfinity;
                    foreach(var sol in solutions) {
                        var dist = Vector2.Distance(new Vector2((float)sol.Real, (float)sol.Imaginary), newtonCoords);
                        if(dist < bestDist) {
                            bestDist = dist;
                            bestSol = sol;
                        }
                    }
                    Color color = Color.White;
                    if(bestSol == solutions[0])
                        color = Color.Red;
                    else if(bestSol == solutions[1])
                        color = Color.Green;
                    else if(bestSol == solutions[2])
                        color = Color.Blue;
                    else if(bestSol == solutions[3])
                        color = Color.Purple;
                    else if(bestSol == solutions[4])
                        color = Color.Orange;
                    else
                        color = Color.Black; // error!
                    spriteBatch.Draw(Pixel, new Vector2(x, y), null, color, 0f, Vector2.Zero, pixelSize, SpriteEffects.None, 0);
                }
            }
            spriteBatch.DrawString(DebugFont, Camera.screen_to_world_pos(Input.MousePosition).ToString(), Vector2.Zero, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
