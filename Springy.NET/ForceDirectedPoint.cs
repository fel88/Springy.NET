using System;

namespace Springy.Lib
{    
    public class ForceDirectedPoint
    {
        public void applyForce(Vector force)
        {
            this.a = this.a.add(force.divide(this.m));
        }

        public ForceDirectedPoint(Vector position, double mass)
        {
            p = position;
            m = mass;
        }
        public Vector p; // position
        public double m; // mass
        public Vector v = new Vector(0, 0); // velocity
        public Vector a = new Vector(0, 0); // acceleration

        internal void applyForce(object p)
        {
            //throw new NotImplementedException();
        }
    }

  

}
