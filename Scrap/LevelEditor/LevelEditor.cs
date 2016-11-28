﻿using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Triangulator;
namespace LevelEditor
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class LevelEditor : Game
    {
        InputManager inputManager;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public World world;

        public Camera camera;
        Terrain terrain;
        public LevelEditor()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            terrain = new Terrain(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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
            inputManager = InputManager.GetManager();
            camera = new Camera(this);
            camera.Position = new Vector2(22, 20);
            world = new World(new Vector2(0, 1f));
            terrain.LoadContent();
            terrain.CreateGround(world);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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

            inputManager.Update();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            if (inputManager.WasKeyReleased(Keys.Q)) camera.Rotate(-.005f);
            if (inputManager.WasKeyReleased(Keys.E)) camera.Rotate(+.005f);
            if (inputManager.WasKeyReleased(Keys.D)) camera.Position += new Vector2(1f, 0f);
            if (inputManager.WasKeyReleased(Keys.A)) camera.Position += new Vector2(-1f, 0f);
            if (inputManager.WasKeyReleased(Keys.W)) camera.Position += new Vector2(0f, -1f);
            if (inputManager.WasKeyReleased(Keys.S)) camera.Position += new Vector2(0f, 1f);
            if (inputManager.WasKeyReleased(Keys.Space)) camera.Position = new Vector2(0f, 0f);



            camera.Zoom(inputManager.ScroleWheelDelta() * .01f);//ToDo: Camera Controls need to be changed


            camera.Update(gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            terrain.RenderTerrain();
            GraphicsDevice.Clear(Color.Beige);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, null, null, null, null, null);
            terrain.Draw(spriteBatch);
            spriteBatch.End();
            
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}