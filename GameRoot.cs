using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace mono_newton_directx {
    public class GameRoot : Game {
        public static GameRoot Instance { get; private set; }
        public static Viewport Viewport { get { return Instance.GraphicsDevice.Viewport; } }
        public static Vector2 ScreenSize { get { return new Vector2(Viewport.Width, Viewport.Height); } }
        public static Texture2D Pixel { get; private set; }
        public static SpriteFont DebugFont { get; private set; }
        public static int pixelSize = 3;
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
            List<List<int>> grid = new List<List<int>>();
            var watch = Stopwatch.StartNew();
            List<int> zeroRow = new List<int>();
            for(int y = 0; y < ScreenSize.Y; y += pixelSize)
                zeroRow.Add(0);
            for(int x = 0; x < (int)ScreenSize.X; x += pixelSize)
                grid.Add(zeroRow);
            //for(int y = 0; y < ScreenSize.Y; y += pixelSize) {
            //Parallel.ForEach(Enumerable.Range(0, (int)(ScreenSize.Y / pixelSize)).Select(y => y * pixelSize), y => {

            Parallel.For(0, (int)(ScreenSize.Y / pixelSize), z => {
                var y = z * pixelSize;
                List<int> row = new List<int>();
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

                    /*Color color = Color.Black;
                    if(bestSol == solutions[0])
                        color = Color.Red;
                    else if(bestSol == solutions[1])
                        color = Color.Green;
                    else if(bestSol == solutions[2])
                        color = Color.Blue;
                    else if(bestSol == solutions[3])
                        color = Color.Purple;
                    else if(bestSol == solutions[4])
                        color = Color.Orange;*/
                    int color = 0;
                    if(bestSol == solutions[0])
                        color = 1;
                    else if(bestSol == solutions[1])
                        color = 2;
                    else if(bestSol == solutions[2])
                        color = 3;
                    else if(bestSol == solutions[3])
                        color = 4;
                    else if(bestSol == solutions[4])
                        color = 5;
                    //spriteBatch.Draw(Pixel, new Vector2(x, y), null, color, 0f, Vector2.Zero, pixelSize, SpriteEffects.None, 0);
                    //grid[x][y] = color;
                    row.Add(color);
                }
                grid[y] = row;
                //System.Diagnostics.Debug.WriteLine(row.Count);
            });
            watch.Stop();
            Debug.WriteLine($"Calc: {watch.ElapsedMilliseconds}");
            watch = Stopwatch.StartNew();
            Debug.WriteLine($"grid.count {grid.Count}, {grid[0].Count}");
            for(int y = 0; y < (int)(ScreenSize.Y / pixelSize); y++) {
                for(int x = 0; x < (int)(ScreenSize.X / pixelSize); x++) {
                    var cell = grid[y][x];
                    Color color = Color.Black;
                    if (cell == 1)
                        color = Color.Red;
                    else if(cell == 2)
                        color = Color.Green;
                    else if(cell == 3)
                        color = Color.Blue;
                    else if(cell == 4)
                        color = Color.Purple;
                    else if(cell == 5)
                        color = Color.Orange;
                    spriteBatch.Draw(Pixel, new Vector2(x * pixelSize, y * pixelSize), null, color, 0f, Vector2.Zero, pixelSize, SpriteEffects.None, 0);
                }
            }
            watch.Stop();
            Debug.WriteLine($"Draw: {watch.ElapsedMilliseconds}");
            spriteBatch.DrawString(DebugFont, Camera.screen_to_world_pos(Input.MousePosition).ToString(), Vector2.Zero, Color.White);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
