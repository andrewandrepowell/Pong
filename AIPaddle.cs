using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Pong
{
    internal class AIPaddle : IGameObject
    {
        private static readonly Color _TRANSPARENT_MASK = Color.White;
        private readonly Texture2D _mask;
        private readonly int _defendingY;
        private readonly int _speed;
        private Rectangle _roomBounds;
        private Line[] _roomLines;
        private ICollection<IGameObject> _collidableObjects;
        private IGameObject _ball;
        private Point _position;
        private Point _velocity;

        public AIPaddle(ContentManager content, Point position, Rectangle roomBounds, int defendingY, int speed = 200)
        {
            Debug.Assert(defendingY == 0 || defendingY == roomBounds.Height);
            Debug.Assert(speed >= 0);
            _mask = content.Load<Texture2D>("paddle");
            _roomBounds = roomBounds;
            _roomLines = Line.Lines(roomBounds);
            _position = position;
            _velocity = Point.Zero;
            _collidableObjects = new List<IGameObject>();
            _defendingY = defendingY;
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
                _roomLines = Line.Lines(value);
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

        public IGameObject Ball
        {
            get { return _ball; }
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(value.RoomBounds == RoomBounds);
                _ball = value;
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
            Debug.Assert(Ball != null);

            // Determine where the ball is going to go.
            Point? selectedPoint = null;
            {
                // Run through loop where each iteration finds what point on the room bounds
                //   where the trajectory of the ball will intersection.
                // Keep performing loop until the intersection is on the defending Y.

                // Current position represents either the current position of the ball
                //   or a position on one of the walls of the room's bounds.
                // Velocity point is the velocity, or trajection, from the current position.
                // Velocity line is the trajection represented as a line.
                Point currentPosition = Ball.Position + Ball.Mask.Bounds.Center;
                Point velocityPoint = Ball.Velocity;
                Line velocityLine = new Line(p0: currentPosition, p1: currentPosition + velocityPoint);
#if DEBUG 
                List<Tuple<Point, Point>> debugList = new List<Tuple<Point, Point>>();
                debugList.Add(Tuple.Create(currentPosition, velocityPoint));
#endif

                // The goal of the algorithm is keep finding a new current position based on where the 
                //   velocity line intersections on one of the lines that makes up the room's bounds.
                while (true)
                {
                    // Check each wall to see if the trajection intersects.
                    selectedPoint = _roomLines
                        .Where(roomLine => Line.Intersects(velocityLine, roomLine)) // Check to see if intersection is possible between trajectory and room line.
                        .Select(roomLine => Line.Intersect(velocityLine, roomLine)) // Determine the intersection.
                        .Where(intersection => GameMath.Dot(velocityPoint, intersection - currentPosition) > 0) // Check to see if direction to intersection from position is facing the same direction as trajectory.
                        .Where(intersection => intersection != currentPosition) // Check to see if intersection isn't already the current position.
                        .Where(intersection => // Check to see if intersection is on the room's bounds.
                            (intersection.X >= 0 && intersection.X <= RoomBounds.Width &&
                             intersection.Y >= 0 && intersection.Y <= RoomBounds.Height))
                        .First(); // Return the first point as the selected point. There really should only be one point in the list.

                    // If the selected point is on the defending Y, then we found what we're looking for.
                    if (selectedPoint.Value.Y == _defendingY)
                        break;

#if DEBUG
                    Debug.Assert(debugList.Count < 1000);
#endif

                    // Update the current position with the selected point.
                    currentPosition = selectedPoint.Value;

                    // Update the current trajectory. 
                    if (selectedPoint.Value.X == RoomBounds.Left)
                        velocityPoint.X = +Math.Abs(velocityPoint.X);
                    if (selectedPoint.Value.X == RoomBounds.Right)
                        velocityPoint.X = -Math.Abs(velocityPoint.X);
                    if (selectedPoint.Value.Y == RoomBounds.Top)
                        velocityPoint.Y = +Math.Abs(velocityPoint.Y);
                    if (selectedPoint.Value.Y == RoomBounds.Bottom)
                        velocityPoint.Y = -Math.Abs(velocityPoint.Y);
                    velocityLine = new Line(p0: currentPosition, p1: currentPosition + velocityPoint);
#if DEBUG
                    debugList.Add(Tuple.Create(currentPosition, velocityPoint));
#endif
                }
            }

            // Update position based on velocity.
            int direction;
            int positionDifference = selectedPoint.Value.X - (Position.X + Mask.Bounds.Center.X);
            if (Math.Abs(positionDifference) < 30)
                direction = 0;
            else if (positionDifference > 0)
                direction = 1;
            else
                direction = -1;
            Velocity = new Point(x: (int)Math.Round(direction * _speed * gameTime.ElapsedGameTime.TotalSeconds), y: 0);

            // Update the position.
            Position += Velocity;
        }
    }
}
