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
            CameraPos = new Vector3(0, 0, 0);
            CameraDir = new Vector3(0, 0, 1);
            CameraUp = new Vector3(0, 1, 0);
            View = Matrix.CreateLookAt(CameraPos, CameraPos + CameraDir, CameraUp);

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
