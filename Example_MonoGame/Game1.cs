using ExampleShared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NuklearDotNet;

namespace Example_MonoGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private MonoGameDevice _monoGameDevice;

        private MouseState _prevMouse;
        private KeyboardState _prevKeyboard;
        private int _prevScrollValue;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;

            // With dpiAware=false in the manifest, Windows handles DPI scaling
            // via DWM so coordinates are always consistent. 1024x768 is large
            // enough to fit all Nuklear demo panels.
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _monoGameDevice = new MonoGameDevice(GraphicsDevice);

            Window.TextInput += (_, e) => _monoGameDevice.OnText(e.Character.ToString());

            Shared.Init(_monoGameDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            HandleMouseInput();
            HandleKeyboardInput();

            base.Update(gameTime);
        }

        private void HandleMouseInput()
        {
            var mouse = Mouse.GetState();

            if (mouse.X != _prevMouse.X || mouse.Y != _prevMouse.Y)
                _monoGameDevice.OnMouseMove(mouse.X, mouse.Y);

            CheckMouseButton(mouse.LeftButton, _prevMouse.LeftButton, NuklearEvent.MouseButton.Left, mouse.X, mouse.Y);
            CheckMouseButton(mouse.RightButton, _prevMouse.RightButton, NuklearEvent.MouseButton.Right, mouse.X, mouse.Y);
            CheckMouseButton(mouse.MiddleButton, _prevMouse.MiddleButton, NuklearEvent.MouseButton.Middle, mouse.X, mouse.Y);

            int scrollDelta = mouse.ScrollWheelValue - _prevScrollValue;
            if (scrollDelta != 0)
                _monoGameDevice.OnScroll(0, System.Math.Sign(scrollDelta) * 1f);

            _prevScrollValue = mouse.ScrollWheelValue;
            _prevMouse = mouse;
        }

        private void CheckMouseButton(ButtonState current, ButtonState previous, NuklearEvent.MouseButton button, int x, int y)
        {
            if (current == ButtonState.Pressed && previous == ButtonState.Released)
                _monoGameDevice.OnMouseButton(button, x, y, true);
            else if (current == ButtonState.Released && previous == ButtonState.Pressed)
                _monoGameDevice.OnMouseButton(button, x, y, false);
        }

        private void HandleKeyboardInput()
        {
            var kb = Keyboard.GetState();

            SendKey(NkKeys.Shift, kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift));
            SendKeyPress(NkKeys.Del, Keys.Delete, kb);
            SendKeyPress(NkKeys.Enter, Keys.Enter, kb);
            SendKeyPress(NkKeys.Tab, Keys.Tab, kb);
            SendKeyPress(NkKeys.Backspace, Keys.Back, kb);
            SendKeyPress(NkKeys.Up, Keys.Up, kb);
            SendKeyPress(NkKeys.Down, Keys.Down, kb);
            SendKeyPress(NkKeys.Left, Keys.Left, kb);
            SendKeyPress(NkKeys.Right, Keys.Right, kb);

            bool ctrl = kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl);
            SendKeyPress(NkKeys.Copy, Keys.C, kb, ctrl);
            SendKeyPress(NkKeys.Paste, Keys.V, kb, ctrl);
            SendKeyPress(NkKeys.Cut, Keys.X, kb, ctrl);
            SendKeyPress(NkKeys.TextSelectAll, Keys.A, kb, ctrl);
            SendKeyPress(NkKeys.TextWordLeft, Keys.Left, kb, ctrl);
            SendKeyPress(NkKeys.TextWordRight, Keys.Right, kb, ctrl);
            SendKeyPress(NkKeys.LineStart, Keys.Home, kb);
            SendKeyPress(NkKeys.LineEnd, Keys.End, kb);

            _prevKeyboard = kb;
        }

        private void SendKey(NkKeys nkKey, bool down)
        {
            _monoGameDevice.OnKey(nkKey, down);
        }

        private void SendKeyPress(NkKeys nkKey, Keys xnaKey, KeyboardState kb, bool modifier = true)
        {
            if (modifier && kb.IsKeyDown(xnaKey) && !_prevKeyboard.IsKeyDown(xnaKey))
                _monoGameDevice.OnKey(nkKey, true);
            else if (!kb.IsKeyDown(xnaKey) && _prevKeyboard.IsKeyDown(xnaKey))
                _monoGameDevice.OnKey(nkKey, false);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Shared.DrawLoop((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Draw(gameTime);
        }
    }
}
