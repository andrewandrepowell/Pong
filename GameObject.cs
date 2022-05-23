using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Pong
{
    /// <summary>
    /// Defines the accessors and operations for GameObjects.
    /// </summary>
    internal interface IGameObject
    {
        public string Name { get; }
        public Texture2D Mask { get; }
        public Point Position { get; set; }
        public Point Velocity { get; set; }
        public Rectangle RoomBounds {  get; set; }
        public bool Solid { get; }
        public ICollection<IGameObject> CollidableGameObjects { get; set; }
        public void UpdatePosition(GameTime gameTime);
        public void ServiceCollisions(GameTime gameTime);
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    }

    /// <summary>
    /// Consists of important functions related to performing operations over <c>IGameObject</c>.
    /// </summary>
    internal class GameObject
    {
        /// <summary>
        /// Services a collision between a current IGameObject and an other IGameObject.
        /// If both current and other are Solid, then current's position will be moved so that
        /// it's aligned to other but no longer colliding.
        /// </summary>
        /// <param name="current">Refers to the IGameObject that's doing the colliding.</param>
        /// <param name="other">Refers to the IGameObject getting collided into.</param>
        /// <returns>
        /// A bool where true indicates a collision occurred between current and other, otherwise false.
        /// </returns>
        public static bool ServiceCollision(IGameObject current, IGameObject other)
        {
            Debug.Assert(current != null);
            Debug.Assert(other != null);
            Debug.Assert(current != other);

            // Determine bounding boxes.
            Rectangle currentBounds = new Rectangle(
                location: current.Position,
                size: current.Mask.Bounds.Size);
            Rectangle otherBounds = new Rectangle(
                location: other.Position,
                size: other.Mask.Bounds.Size);

            // Check for bounding box intersection.
            if (currentBounds.Intersects(otherBounds))
            {
                // Find intersections boxes in bounding boxes.
                Rectangle intersection = Rectangle.Intersect(currentBounds, otherBounds);
                Rectangle currentIntersection = new Rectangle(
                    x: intersection.X - currentBounds.X,
                    y: intersection.Y - currentBounds.Y,
                    width: intersection.Width,
                    height: intersection.Height);
                Rectangle otherIntersection = new Rectangle(
                    x: intersection.X - otherBounds.X,
                    y: intersection.Y - otherBounds.Y,
                    width: intersection.Width,
                    height: intersection.Height);

                // Grab the data from the masks themselves.
                Color[] currentData = new Color[currentIntersection.Width * currentIntersection.Height];
                Color[] otherData = new Color[otherIntersection.Width * otherIntersection.Height];

                current.Mask.GetData(
                    level: 0,
                    rect: currentIntersection,
                    data: currentData,
                    startIndex: 0,
                    elementCount: currentData.Length);
                other.Mask.GetData(
                    level: 0,
                    rect: otherIntersection,
                    data: otherData,
                    startIndex: 0,
                    elementCount: otherData.Length);

                // Create collision mask and determine whether a collision occurred.
                bool[] collisionMask = new bool[currentData.Length];
                for (int index = 0; index < currentData.Length; index++)
                    collisionMask[index] = currentData[index] != Color.Transparent && otherData[index] != Color.Transparent;
                bool collisionOccurred = collisionMask.Contains(true);

                // Correct position of solid objects on collision.
                if (collisionOccurred && current.Solid && other.Solid)
                {
                    // Not the best position correction algorithm.
                    // Will probably need to get updated in future games.

                    // Determine max column and row intersections.
                    int[] colCounts = new int[intersection.Width];
                    int[] rowCounts = new int[intersection.Height];
                    for (int row = 0; row < intersection.Height; row++)
                        for (int col = 0; col < intersection.Width; col++)
                            if (collisionMask[col + row * intersection.Width])
                            {
                                colCounts[col]++;
                                rowCounts[row]++;
                            }
                    int colMax = GameMath.Max(colCounts);
                    int rowMax = GameMath.Max(rowCounts);

                    // Determine where the collisions occurred.
                    bool topCollision = otherBounds.Top == intersection.Top;
                    bool bottomCollision = otherBounds.Bottom == intersection.Bottom;
                    bool leftCollision = otherBounds.Left == intersection.Left;
                    bool rightCollision = otherBounds.Right == intersection.Right;

                    // Make the corrections based on the smallest amount of distance needed.
                    if (colMax < rowMax)
                    {
                        if (topCollision)
                        {
                            current.Position -= new Point(x: 0, y: colMax);
                        }
                        if (bottomCollision)
                        {
                            current.Position += new Point(x: 0, y: colMax);
                        }
                    }
                    else
                    {
                        if (leftCollision)
                        {
                            current.Position -= new Point(x: rowMax, y: 0);
                        }
                        if (rightCollision)
                        {
                            current.Position += new Point(x: rowMax, y: 0);
                        }
                    }   
                }

                // Return whether or not the collision occurred.
                return collisionOccurred;
            }

            // If there's not even a bounding box detection, always return false.
            return false;
        }

        /// <summary>
        /// Services a collision between a current IGameObject and the walls of a room where current is assumed to be in.
        /// </summary>
        /// <param name="current">Refers to the IGameObject that's doing the colliding.</param>
        /// <param name="roomBounds">Refers to the Rectangle that represents the bounding box of the room.</param>
        /// <returns>
        /// A bool where true indicates a collision occurred between current and the wall of the room, otherwise false.
        /// </returns>
        public static bool ServiceCollision(IGameObject current)
        {
            Debug.Assert(current != null);

            Rectangle currentBounds = new Rectangle(
                location: current.Position,
                size: current.Mask.Bounds.Size);
            Rectangle intersection = Rectangle.Intersect(currentBounds, current.RoomBounds);

            // Detecting collision with the room bounds is simply
            // just checking to see if the intersection is equal to current bounds.
            if (intersection != currentBounds)
            {
                // The position correction is based entirely on the bounding box,
                // SO THIS DEFINITELY NEEDS TO GET UPDATED.
                // Should be fine for the pong game though.

                int colMax = currentBounds.Width - intersection.Width;
                int rowMax = currentBounds.Height - intersection.Height;

                // Determine where the collisions occurred.
                bool topCollision = current.RoomBounds.Top == intersection.Top;
                bool bottomCollision = current.RoomBounds.Bottom == intersection.Bottom;
                bool leftCollision = current.RoomBounds.Left == intersection.Left;
                bool rightCollision = current.RoomBounds.Right == intersection.Right;

                // Make the corrections based on the smallest amount of distance needed.
                if (topCollision)
                {
                    current.Position += new Point(x: 0, y: rowMax);
                }
                if (bottomCollision)
                {
                    current.Position -= new Point(x: 0, y: rowMax);
                }
                if (leftCollision)
                {
                    current.Position += new Point(x: colMax, y: 0);
                }
                if (rightCollision)
                {
                    current.Position -= new Point(x: colMax, y: 0);
                }
                return true;
            }
            return false;
        }
    }
}
