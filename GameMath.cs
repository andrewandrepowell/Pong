using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Pong
{
    /// <summary>
    /// Represents a line using the standard form Ax + By = C.
    /// The main intent is to determine the intersection between lines.
    /// </summary>
    internal struct Line
    {
        // https://stackoverflow.com/questions/4543506/algorithm-for-intersection-of-2-lines
        public int A, B, C;

        /// <summary>
        /// Creates a line from two points. 
        /// The order of the two points don't matter since a line doesn't have direction.
        /// </summary>
        /// <param name="p0">One of the two points.</param>
        /// <param name="p1">One of the two points.</param>
        public Line(Point p0, Point p1)
        {
            A = p1.Y - p0.Y;
            B = p0.X - p1.X;
            C = A * p0.X + B * p0.Y;
        }

        /// <summary>
        /// Creates a line with the standard coefficients A, B, and C.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        public Line(int A, int B, int C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        /// <summary>
        /// Creates a line two points.
        /// The order of the two points don't matter since a line doesn't have direction.
        /// </summary>
        /// <param name="x0">X component of point 0.</param>
        /// <param name="y0">Y component of point 0.</param>
        /// <param name="x1">X component of point 1.</param>
        /// <param name="y1">Y component of point 1.</param>
        public Line(int x0, int y0, int x1, int y1)
        {
            A = y0 - y1;
            B = x0 - x1;
            C = A * x0 + B * y0;
        }

        /// <summary>
        /// Determines the determinant between two lines.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static int Determinant(Line current, Line other)
        {
            return current.A * other.B - other.A * current.B;
        }

        /// <summary>
        /// Determines whether the two lines actually intersect.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns>True if intersection does occur, false is parallel lines.</returns>
        public static bool Intersects(Line current, Line other)
        {
            return Determinant(current, other) != 0;
        }

        /// <summary>
        /// Determines the point of intersection between two lines.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns>Intersection as a point.</returns>
        public static Point Intersect(Line current, Line other)
        {
            int determinant = Determinant(current, other);
            Debug.Assert(determinant != 0);
            Point intersection = new Point(
                x: (int)Math.Round((float)(other.B * current.C - current.B * other.C) / determinant),
                y: (int)Math.Round((float)(current.A * other.C - other.A * current.C) / determinant));
            return intersection;
        }

        /// <summary>
        /// Creates 4 lines of a rectangle.
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns>An array of length 4. Index 0 is the top line, 1 is the right line, 2 is the bottom ine, and 3 is the left line.</returns>
        public static Line[] Lines(Rectangle rectangle)
        { // 0 - top, 1 - right, 2 - bottom, 3 - left
            Line[] lines = new Line[]
            {
                new Line(
                    x0: rectangle.Left,
                    y0: rectangle.Top,
                    x1: rectangle.Right,
                    y1: rectangle.Top),
                new Line(
                    x0: rectangle.Right,
                    y0: rectangle.Top,
                    x1: rectangle.Right,
                    y1: rectangle.Bottom),
                new Line(
                    x0: rectangle.Right,
                    y0: rectangle.Bottom,
                    x1: rectangle.Left,
                    y1: rectangle.Bottom),
                new Line(
                    x0: rectangle.Left,
                    y0: rectangle.Bottom,
                    x1: rectangle.Left,
                    y1: rectangle.Top)
            };
            return lines;
        }
     }

    /// <summary>
    /// Defines several math-related functions useful for game development.
    /// </summary>
    internal class GameMath
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Determines the minimum of a list and the index thereof.
        /// </summary>
        /// <param name="values">List of values.</param>
        /// <param name="min">Minimum of values.</param>
        /// <param name="index">Index where the minimum is located.</param>
        public static void Min<T>(IList<T> values, out T min, out int index) where T : IComparable<T>
        {
            Debug.Assert(values!.Count > 0);

            min = values[0];
            index = 0;
            for (int i = 1; i < values.Count; i++)
            {
                if (min.CompareTo(values[i]) > 0)
                {
                    index = i;
                    min = values[i];
                }
            }
            Debug.Assert(index >= 0);
        }

        /// <summary>
        /// Determines the minimum of a list.
        /// </summary>
        /// <param name="values">List of values.</param>
        /// <returns>Minimum of values.</returns>
        public static T Min<T>(IList<T> values) where T : IComparable<T>
        {
            Debug.Assert(values!.Count > 0);

            int index;
            T min;
            Min(values, out min, out index);
            return min;
        }

        /// <summary>
        /// Determines the maximum of a list and the index thereof.
        /// </summary>
        /// <param name="values">List of values.</param>
        /// <param name="max">Maximum of values.</param>
        /// <param name="index">Index where the maximum is located.</param>
        public static void Max<T>(IList<T> values, out T max, out int index) where T : IComparable<T>
        {
            Debug.Assert(values!.Count > 0);

            max = values[0];
            index = 0;
            for (int i = 1; i < values.Count; i++)
            {
                if (max.CompareTo(values[i]) < 0)
                {
                    index = i;
                    max = values[i];
                }
            }
            Debug.Assert(index >= 0);
        }

        /// <summary>
        /// Determines the maximum of a list.
        /// </summary>
        /// <param name="values">List of values.</param>
        /// <returns>Maximum of values.</returns>
        public static T Max<T>(IList<T> values) where T : IComparable<T>
        {
            Debug.Assert(values!.Count > 0);

            int index;
            T max;
            Max(values, out max, out index);
            return max;
        }

        /// <summary>
        /// Randomly mixes up the order of a list.
        /// </summary>
        /// <param name="values">List of values.</param>
        public static void Shuffle<T>(IList<T> list)
        {
            // https://stackoverflow.com/questions/273313/randomize-a-listt
            int n = list!.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Finds the cross-product between two vectors represented as Points.
        /// </summary>
        /// <param name="point0">First vector Point.</param>
        /// <param name="point1">Second vector Point.</param>
        /// <returns>Cross-product of point0 and point1.</returns>
        public static int Cross(Point point0, Point point1)
        {
            return point0.X * point1.Y - point1.X * point0.Y;
        }

        /// <summary>
        /// Finds the dot-product between two vectors represented as Points.
        /// </summary>
        /// <param name="point0">First vector Point.</param>
        /// <param name="point1">Second vector Point.</param>
        /// <returns>Dot-product of point0 and point1.</returns>
        public static int Dot(Point point0, Point point1)
        {
            return point0.X * point1.X + point0.Y * point1.Y;
        }
    }
}
