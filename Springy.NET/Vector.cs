using System;

namespace Springy.Lib
{
    public class Vector
    {
        public Vector()
        {

        }
        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public double x;
        public double y;
        public Vector normal()
        {
            return new Vector(-this.y, this.x);
        }
        static Random mathRandom = new Random();
        internal static Vector random()
        {
            return new Vector(10.0 * (mathRandom.NextDouble() - 0.5), 10.0 * (mathRandom.NextDouble() - 0.5));
        }

        public Vector subtract(Vector v2)
        {
            return new Vector(this.x - v2.x, this.y - v2.y);
        }

        public double magnitude()
        {
            return Math.Sqrt(this.x * this.x + this.y * this.y);
        }
        public Vector divide(double n)
        {
            return new Vector((this.x / n), (this.y / n)); // Avoid divide by zero errors..
        }

        public Vector normalise()
        {
            return this.divide(this.magnitude());
        }

        public Vector add(Vector v2)
        {
            return new Vector(this.x + v2.x, this.y + v2.y);
        }

        public Vector multiply(double n)
        {
            return new Vector(this.x * n, this.y * n);
        }
    }

  

}
