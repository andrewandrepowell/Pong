using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pong
{
    internal class HumanPaddle : IGameObject
    {
        private static readonly Color _TRANSPARENT_MASK = Color.White;
        private readonly Texture2D _mask;
        private readonly int _speed;
        private Rectangle _roomBounds;
        private ICollection<IGameObject> _collidableObjects;
        private Point _position;
        private Point _velocity;
        
        public HumanPaddle(ContentManager content, Point position, Rectangle roomBounds, int speed = 200)
        {
            _mask = content.Load<Texture2D>("paddle");
            _roomBounds = roomBounds;
            _position = position;
            _velocity = Point.Zero;
            _collidableObjects = new List<IGameObject>();
            _speed = speed;
        }

        public string Name { get => "Paddle"; }

        public Texture2D Mask { get { return _mask; } }

        public Point Position
        {
            get { return _position; }
            set
            {
                Debug.Assert(_position != null);
                _position = value;
            }
        }

        public Point Velocity
        {
            get { return _velocity; }
            set
            {
                Debug.Assert(_position != null);
                _velocity = value;
            }
        }

        public Rectangle RoomBounds
        {
            get { return _roomBounds; }
            set
            {
                Debug.Assert(_roomBounds != null);
                _roomBounds = value;
            }
        }

        public bool Solid { get => true; }

        public ICollection<IGameObject> CollidableGameObjects
        {
            get { return _collidableObjects; }
            set
            {
                Debug.Assert(value != null);
                _collidableObjects = value;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: Mask,
                destinationRectangle: new Rectangle(
                    location: Position,
                    size: Mask.Bounds.Size),
                color: _TRANSPARENT_MASK);
        }

        public void ServiceCollisions(GameTime gameTime)
        {
            foreach (IGameObject other in _collidableObjects)
            {
                if (GameObject.ServiceCollision(current: this, other: other))
                {

                }
            }
            if (GameObject.ServiceCollision(current: this))
            {

            }
        }

        public void UpdatePosition(GameTime gameTime)
        {
            Point velocity = new Point();

            // Determine new velocity from user input.
            KeyboardState kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.Left))
                velocity.X = (int)Math.Round(-_speed * gameTime.ElapsedGameTime.TotalSeconds);

            if (kstate.IsKeyDown(Keys.Right))
                velocity.X = (int)Math.Round(+_speed * gameTime.ElapsedGameTime.TotalSeconds);
            Velocity = velocity;

            // Update position based on velocity.
            Position += Velocity;
        }
    }
}
