using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pong
{
    internal class Ball : IGameObject
    {
        private static readonly Color _TRANSPARENT_MASK = Color.White;
        private static readonly Random _random = new Random();
        private readonly Texture2D _mask;
        private readonly Point[] _bounceDirections;
        private ICollection<IGameObject> _collidableObjects;
        private Point _position;
        private Point _velocity;
        private Rectangle _roomBounds;

        public Ball(ContentManager content, Point position, Rectangle roomBounds, int speed = 200)
        {
            Debug.Assert(content != null);
            Debug.Assert(position != null);
            Debug.Assert(roomBounds != null);
            Debug.Assert(speed > 0);

            _mask = content.Load<Texture2D>("ball");
            _position = position;
            _roomBounds = roomBounds;
            _collidableObjects = new List<IGameObject>();

            double directionDegrees;
            do
            {
                double directionRadians = _random.NextDouble() * 2 * Math.PI;
                directionDegrees = directionRadians / Math.PI * 180;
                double xVelocity = Math.Round(speed * Math.Cos(directionRadians));
                double yVelocity = Math.Round(speed * Math.Sin(directionRadians));
                _velocity = new Point(x: (int)xVelocity, y: (int)yVelocity);
            } while ((directionDegrees >= 0 && directionDegrees <= 60) ||
                     (directionDegrees >= 120 && directionDegrees <= 360));

            int bounceLength = 32;
            _bounceDirections = Enumerable
                .Range(0, bounceLength)
                .Select(value => new Point(
                    x: (int)Math.Round(speed * Math.Cos(2 * Math.PI * value / bounceLength)), 
                    y: (int)Math.Round(speed * Math.Sin(2 * Math.PI * value / bounceLength))))
                .Where(value => value.Y != 0)
                .ToArray();
            
        }
        public string Name { get => "Ball"; }
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
                texture: _mask, 
                destinationRectangle: new Rectangle(
                    location: _position,
                    size: _mask.Bounds.Size), 
                color: _TRANSPARENT_MASK); // As far as I can tell, the color mask doesn't do anything : /
        }

        public void ServiceCollisions(GameTime gameTime)
        {
            foreach (IGameObject other in _collidableObjects)
            {
                if (GameObject.ServiceCollision(current: this, other: other))
                {
                    Point currentCenter = Position + Mask.Bounds.Center;
                    Point otherCenter = other.Position + other.Mask.Bounds.Center;
                    Point collisionDirection = otherCenter - currentCenter;

                    float[] bounceDistances = _bounceDirections
                        .Select(bounceDirection => (collisionDirection - bounceDirection)
                        .ToVector2()
                        .LengthSquared())
                        .ToArray();
                    float minimumDistance;
                    int minimumIndex;
                    GameMath.Min(values: bounceDistances, out minimumDistance, out minimumIndex);

                    Velocity = _bounceDirections[minimumIndex] * new Point(x: -1, y: -1);
                }
            }
            if (GameObject.ServiceCollision(current: this))
            {
                int leftDistance = Position.X;
                int rightDistance = RoomBounds.Width - (Position.X + Mask.Bounds.Right);
                int topDistance = Position.Y;
                int bottomDistance = RoomBounds.Height - (Position.Y + Mask.Bounds.Bottom);

                Debug.Assert(leftDistance >= 0);
                Debug.Assert(rightDistance >= 0);
                Debug.Assert(topDistance >= 0);
                Debug.Assert(bottomDistance >= 0);

                int index;
                int min;
                GameMath.Min(
                    values: new int[] { leftDistance, rightDistance, topDistance, bottomDistance }, 
                    min: out min, 
                    index: out index);
                Debug.Assert(index >= 0 && index <= 3);

                if (index == 0 || index == 1)
                    Velocity *= new Point(x: -1, y: 1);
                else
                    Velocity *= new Point(x: 1, y: -1);
            }
        }

        public void UpdatePosition(GameTime gameTime)
        {
            Position += new Point(
                x: (int)Math.Round(Velocity.X * gameTime.ElapsedGameTime.TotalSeconds),
                y: (int)Math.Round(Velocity.Y * gameTime.ElapsedGameTime.TotalSeconds));
        }
    }
}
