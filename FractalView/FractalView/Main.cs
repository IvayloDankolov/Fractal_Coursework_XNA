using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FractalView
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        Effect marcher;
        
        Screen scr;

        Matrix View;
        Matrix Projection;

        Vector3 CameraPos;
        Vector3 CameraDir;
        Vector3 CameraUp;

        float leftRot =0;
        float upRot = 0;
        float MoveSpeed = 30;

        public int Width { get { return GraphicsDevice.DisplayMode.Width; } }
        public int Height { get { return GraphicsDevice.DisplayMode.Height; } }

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(OnResize);
            Content.RootDirectory = "Content";
            scr = new Screen();
        }

        void OnResize(object sender, EventArgs e)
        {
            PresentationParameters pp = new PresentationParameters
            {
                BackBufferWidth = Window.ClientBounds.Width,
                BackBufferHeight = Window.ClientBounds.Height,
                DeviceWindowHandle = Window.Handle,
                DepthStencilFormat = DepthFormat.Depth24,
                IsFullScreen = false,
            };
            GraphicsDevice.Reset(pp);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            CameraPos = new Vector3(0, 0, -5);
            CameraDir = new Vector3(0, 0, 1);
            CameraUp = new Vector3(0, 1, 0);

            UpdateView();

            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 3f, Width / Height, 1, 1000);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            marcher = Content.Load<Effect>("raytracer");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        void UpdateView()
        {
            Matrix rot = Matrix.CreateRotationX(upRot) * Matrix.CreateRotationY(leftRot);
            View = Matrix.CreateTranslation(CameraPos) * rot;
            CameraDir = Vector3.Transform(new Vector3(0, 0, 1), rot);

        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upRot) * Matrix.CreateRotationY(leftRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            CameraPos += MoveSpeed * rotatedVector;
            UpdateView();
        }

        private void HandleKeyboard(float amount)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            if (keyState.IsKeyDown(Keys.Q))
                moveVector += new Vector3(0, 1, 0);
            if (keyState.IsKeyDown(Keys.Z))
                moveVector += new Vector3(0, -1, 0);

            if(moveVector != Vector3.Zero)
            
            AddToCameraPosition(moveVector * amount);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            HandleKeyboard(timeDifference);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            marcher.Parameters["View"].SetValue(View);
            marcher.Parameters["Projection"].SetValue(Projection);
            marcher.Parameters["camPos"].SetValue(CameraPos);

            marcher.Parameters["Iterations"].SetValue(100);
            marcher.Parameters["MarchSteps"].SetValue(100);
            marcher.Parameters["Power"].SetValue(8);
            marcher.Parameters["Bailout"].SetValue(4);

            marcher.CurrentTechnique = marcher.Techniques["Raymarch"];
            foreach( var pass in marcher.CurrentTechnique.Passes)
            {
                pass.Apply();
               
                scr.Draw(GraphicsDevice);
            }

            //GraphicsDevice.Present();

            base.Draw(gameTime);
        }
    }
}
