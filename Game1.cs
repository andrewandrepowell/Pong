using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pong
{
    public class Game1 : Game
    {
        private const int _PADDLE_SPEED = 400;
        private const int _BALL_SPEED = 600;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private ICollection<IGameObject> _gameObjects;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _gameObjects = new List<IGameObject>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Rectangle roomBounds = new Rectangle(
                x: 0,
                y: 0,
                width: _graphics.PreferredBackBufferWidth,
                height: _graphics.PreferredBackBufferHeight);
            Ball ball = new Ball(
                content: Content,
                position: new Point(x: 0, y: 0),
                roomBounds: roomBounds,
                speed: _BALL_SPEED);
            HumanPaddle humanPaddle = new HumanPaddle(
                content: Content,
                position: new Point(x: 0, y: 0),
                roomBounds: roomBounds,
                speed: _PADDLE_SPEED);
            AIPaddle aiPaddle = new AIPaddle(
                content: Content,
                position: new Point(x: 0, y: 0),
                roomBounds: roomBounds,
                defendingY: 0,
                speed: _PADDLE_SPEED);

            _gameObjects.Add(ball);
            _gameObjects.Add(humanPaddle);
            _gameObjects.Add(aiPaddle);

            ball.CollidableGameObjects = new IGameObject[] { humanPaddle, aiPaddle };
            ball.Position = new Point(
                x: _graphics.PreferredBackBufferWidth / 2 - ball.Mask.Bounds.Width / 2, 
                y: _graphics.PreferredBackBufferHeight / 2 - ball.Mask.Bounds.Height / 2);
            humanPaddle.Position = new Point(
                x: _graphics.PreferredBackBufferWidth / 2 - aiPaddle.Mask.Bounds.Width / 2,
                y: _graphics.PreferredBackBufferHeight - aiPaddle.Mask.Bounds.Height);
            aiPaddle.Position = new Point(
                x: _graphics.PreferredBackBufferWidth / 2 - aiPaddle.Mask.Bounds.Width / 2,
                y: 0);
            aiPaddle.Ball = ball;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            foreach (IGameObject gameObject in _gameObjects)
            {
                gameObject.UpdatePosition(gameTime);
                gameObject.ServiceCollisions(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            foreach (IGameObject gameObject in _gameObjects)
                gameObject.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
