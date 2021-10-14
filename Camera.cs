using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace mono_newton_directx {
    static class Camera {
        public static Vector2 CameraPosition = new Vector2(0, 0);
        public static float Zoom = 100;
        public static Vector2 world_to_screen_pos(Vector2 worldPosition) {
            // Scale
            Vector2 position = (worldPosition - CameraPosition) * Zoom + CameraPosition;
            // Translate
            position = position - CameraPosition;
            // Translate by half screen size
            position = position + GameRoot.ScreenSize / 2;

            return position;
        }

        public static Vector2 screen_to_world_pos(Vector2 screenPos) {
            // Translate by half screen size
            Vector2 position = screenPos - GameRoot.ScreenSize / 2;
            // Translate
            position = position + CameraPosition;
            //Scale
            position = (position - CameraPosition) / Zoom + CameraPosition;

            return position;
        }

        public static void Update() {
            // Freecam (disables lerp if used)
            Vector2 direction = Vector2.Zero;
            if(Input.Keyboard.IsKeyDown(Keys.Left) || Input.Keyboard.IsKeyDown(Keys.A))
                direction.X -= 1;
            if(Input.Keyboard.IsKeyDown(Keys.Right) || Input.Keyboard.IsKeyDown(Keys.D))
                direction.X += 1;
            if(Input.Keyboard.IsKeyDown(Keys.Up) || Input.Keyboard.IsKeyDown(Keys.W))
                direction.Y -= 1;
            if(Input.Keyboard.IsKeyDown(Keys.Down) || Input.Keyboard.IsKeyDown(Keys.S))
                direction.Y += 1;
            if(direction != Vector2.Zero) {
                direction *= 5 / Zoom;
                move_relative(direction);
            }

            // Zoom (Q and E)
            if(Input.Keyboard.IsKeyDown(Keys.Q))
                Zoom /= 1.03f;
            if(Input.Keyboard.IsKeyDown(Keys.E))
                Zoom *= 1.03f;
        }

        private static void move_relative(Vector2 direction) {
            CameraPosition += direction;
        }

        private static void Lerp(Vector2 destination) {
            // Lerps (moves) the camera towards a given destination
            float lerp_speed = 0.1f;  // Higher values (between 0 and 1) move towards destination faster
            CameraPosition = (1 - lerp_speed) * CameraPosition + destination * lerp_speed;
        }

        public static Vector2 mouse_world_coords() {
            return screen_to_world_pos(Input.Mouse.Position.ToVector2());
        }
    }
}
