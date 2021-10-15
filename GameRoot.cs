using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace mono_newton_directx
{
    public class GameRoot : Game
    {
        public static GameRoot Instance { get; private set; }
        public static Viewport Viewport { get { return Instance.GraphicsDevice.Viewport; } }
        public static Vector2 ScreenSize { get { return new Vector2(Viewport.Width, Viewport.Height); } }
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        // Input
        public static KeyboardStateExtended Keyboard;
        public static MouseStateExtended Mouse;
        // Content
        public Texture2D Pixel { get; private set; }
        public SpriteFont DebugFont { get; private set; }
        // Settings
        public int pixelSize = 8;
        public int iterations = 2;
        public List<Vector2> polynomial = new List<Vector2> { new Vector2(1, 3), new Vector2(-1, 0) };
        public List<Complex> solutions = new List<Complex> { new Complex(1, 0), new Complex(-0.809, -0.588), new Complex(0.309, 0.951), new Complex(0.309, -0.951), new Complex(-0.809, 0.588) };
        public float offset;
        // Camera stuffs
        public static Vector2 CameraPosition = new Vector2(0, 0);
        public static float Zoom = 200;
        public Vector2 world_to_screen(Vector2 worldPosition) { return ((worldPosition - CameraPosition) * Zoom) + ScreenSize / 2; }
        public static Vector2 screen_to_world_pos(Vector2 screenPos) { return (screenPos - GameRoot.ScreenSize / 2) / Zoom + CameraPosition; }
        public GameRoot()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = 1366, PreferredBackBufferHeight = 768 };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        protected override void Initialize() { base.Initialize(); }
        protected override void LoadContent()
        {
            Pixel = Content.Load<Texture2D>("pixel");
            DebugFont = Content.Load<SpriteFont>("DebugFont");
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        protected override void Update(GameTime gameTime)
        {
            Keyboard = KeyboardExtended.GetState();
            Mouse = MouseExtended.GetState();
            // Rotate solutions
            solutions = new List<Complex>();
            offset += 0.005f;
            for (int i = 0; i < 5; i++)
            {
                float z = (float)i / 5 * MathF.PI * 2;
                solutions.Add(new Complex(Math.Cos(z + offset), Math.Sin(z + offset)));
            }
            // Camera (arrow keys or WASD)
            Vector2 direction = Vector2.Zero;
            if (Keyboard.IsKeyDown(Keys.Left) || Keyboard.IsKeyDown(Keys.A)) direction.X -= 1;
            if (Keyboard.IsKeyDown(Keys.Right) || Keyboard.IsKeyDown(Keys.D)) direction.X += 1;
            if (Keyboard.IsKeyDown(Keys.Up) || Keyboard.IsKeyDown(Keys.W)) direction.Y -= 1;
            if (Keyboard.IsKeyDown(Keys.Down) || Keyboard.IsKeyDown(Keys.S)) direction.Y += 1;
            if (direction != Vector2.Zero) CameraPosition += direction * 5 / Zoom;
            // Zoom (Q and E)
            if (Keyboard.IsKeyDown(Keys.Q))
                Zoom /= 1.03f;
            if (Keyboard.IsKeyDown(Keys.E))
                Zoom *= 1.03f;
            // Change pixel size (keys 1 through 5)
            if (Keyboard.WasKeyJustDown(Keys.D1)) pixelSize = 1;
            else if (Keyboard.WasKeyJustDown(Keys.D2)) pixelSize = 2;
            else if (Keyboard.WasKeyJustDown(Keys.D3)) pixelSize = 4;
            else if (Keyboard.WasKeyJustDown(Keys.D4)) pixelSize = 8;
            else if (Keyboard.WasKeyJustDown(Keys.D5)) pixelSize = 16;
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            List<List<int>> grid = new List<List<int>>();
            // Populate grid with zeros
            List<int> zeroRow = new List<int>();
            for (int x = 0; x < ScreenSize.X; x += pixelSize)
                zeroRow.Add(0);
            for (int y = 0; y < (int)ScreenSize.Y; y += pixelSize)
                grid.Add(zeroRow);
            // For each row in grid
            Parallel.For(0, (int)(ScreenSize.Y / pixelSize), z =>
            {
                var y = z * pixelSize;
                List<int> row = new List<int>();
                // For each pixel in row
                for (int x = 0; x < ScreenSize.X; x += pixelSize)
                {
                    Vector2 coords = screen_to_world_pos(new Vector2(x, y)); // Get pixel coordinates on screen and convert to cartesian coordinates
                    coords = new Vector2(coords.X * MathF.Cos(offset) - coords.Y * MathF.Sin(offset), coords.X * MathF.Sin(offset) + coords.Y * MathF.Cos(offset)); // rotate for fancy effect (not canonically part of newton method fractal)
                    Complex coordsComplex = new Complex(coords.X, coords.Y); // Convert cartesian to complex
                    Complex newtonCoordsComplex = coordsComplex; // Variable for storing newton iteration result
                    for (int i = 0; i <= iterations; i++)
                    {
                        Complex polySum = Complex.Zero; // f(x)
                        Complex derSum = Complex.Zero; // f'(x)
                        // Calculate f(x) and f'(x) values
                        foreach (var term in polynomial)
                        {
                            polySum += term.X * Complex.Pow(newtonCoordsComplex, term.Y);
                            derSum += term.X * term.Y * Complex.Pow(newtonCoordsComplex, term.Y - 1);
                        }
                        newtonCoordsComplex = newtonCoordsComplex - polySum / derSum; // x = x - f(x) / f'(x)
                    }
                    Vector2 newtonCoords = new Vector2((float)newtonCoordsComplex.Real, (float)newtonCoordsComplex.Imaginary); // Convert complex to cartesian
                    Complex bestSol = Complex.Zero; // Variable for storing nearest solution so far
                    float bestDist = float.PositiveInfinity; // Distance (squared) of nearest solution so far
                    // Find nearest root
                    foreach (var sol in solutions)
                    {
                        var dist = Vector2.DistanceSquared(new Vector2((float)sol.Real, (float)sol.Imaginary), newtonCoords); // Distance squared can be calculated faster than actual distance and does not affect method
                        if (dist < bestDist) // Check if this root is closer than the current closer root
                        {
                            bestDist = dist;
                            bestSol = sol;
                        }
                    }
                    // Colour pixel based on which root was nearest
                    int color = 0;
                    for (int i = 0; i < solutions.Count; i++)
                        if (bestSol == solutions[i])
                        {
                            color = i + 1;
                            break;
                        }
                    row.Add(color); // Add pixel to row
                }
                grid[z] = row; // Add row of pixels to grid
            });
            for (int y = 0; y < (int)(ScreenSize.Y / pixelSize); y++)
            { // For row in grid..
                for (int x = 0; x < (int)(ScreenSize.X / pixelSize); x++)
                { // Row pixel in row..
                    var cell = grid[y][x];
                    // Get colour based on colour code
                    Color color = Color.Black;
                    if (cell == 1) color = Color.Red;
                    else if (cell == 2) color = Color.Green;
                    else if (cell == 3) color = Color.Blue;
                    else if (cell == 4) color = Color.Purple;
                    else if (cell == 5) color = Color.Orange;
                    spriteBatch.Draw(Pixel, new Vector2(x * pixelSize, y * pixelSize), null, color, 0f, Vector2.Zero, pixelSize, SpriteEffects.None, 0); // Draw the pixel
                }
            }
            spriteBatch.DrawString(DebugFont, screen_to_world_pos(Mouse.Position.ToVector2()).ToString(), Vector2.Zero, Color.White); // Mouse coordinates
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
