using System;
using System.Collections.Generic;

namespace Springy.Lib
{
    public class LayoutForceDirected
    {

        // returns [bottomleft, topright]
        public BBox getBoundingBox()
        {
            var bottomleft = new Vector(-2, -2);
            var topright = new Vector(2, 2);

            this.eachNode((n, point) =>
            {
                if (point.p.x < bottomleft.x)
                {
                    bottomleft.x = point.p.x;
                }
                if (point.p.y < bottomleft.y)
                {
                    bottomleft.y = point.p.y;
                }
                if (point.p.x > topright.x)
                {
                    topright.x = point.p.x;
                }
                if (point.p.y > topright.y)
                {
                    topright.y = point.p.y;
                }
            });

            var padding = topright.subtract(bottomleft).multiply(0.07); // ~5% padding

            return new BBox() { bottomleft = bottomleft.subtract(padding), topright = topright.add(padding) };
        }

        public void tick(double timestep)
        {
            this.applyCoulombsLaw();
            this.applyHookesLaw();
            this.attractToCentre();
            this.updateVelocity(timestep);
            this.updatePosition(timestep);
        }
        void updatePosition(double timestep)
        {
            this.eachNode((node, point) =>
            {
                // Same question as above; along with updateVelocity, is this all of
                // your integration code?
                point.p = point.p.add(point.v.multiply(timestep));
            });
        }
        // callback should accept one argument: Spring
        void eachSpring(Action<Spring> callback)
        {
            var t = this;
            this.graph.edges.ForEach((e) =>
            {
                callback(t.spring(e));
            });
        }

        Dictionary<int, Spring> edgeSprings = new Dictionary<int, Spring>();
        private Spring spring(Edge edge)
        {

            if (!(this.edgeSprings.ContainsKey(edge.id)))
            {
                //var length = (edge.data.length !== undefined) ? edge.data.length : 1.0;
                var length = 1;
                Spring existingSpring = null;

                var from = this.graph.getEdges(edge.source, edge.target);
                foreach (var e in from)
                {
                    if (existingSpring == null && this.edgeSprings.ContainsKey(e.id))
                    {
                        existingSpring = this.edgeSprings[e.id];
                    }
                }

                if (existingSpring != null)
                {
                    return new Spring(existingSpring.point1, existingSpring.point2, 0.0, 0.0);
                }
                var to = this.graph.getEdges(edge.target, edge.source);
                foreach (var e in from)
                {
                    if (existingSpring == null && this.edgeSprings.ContainsKey(e.id))
                    {
                        existingSpring = this.edgeSprings[e.id];
                    }
                }
                if (existingSpring != null)
                {
                    return new Spring(existingSpring.point2, existingSpring.point1, 0.0, 0.0);
                }
                this.edgeSprings[edge.id] = new Spring(
                this.point(edge.source), this.point(edge.target), length, this.stiffness
            );


            }

            return this.edgeSprings[edge.id];
        }

        double stiffness = 400;

        // Calculate the total kinetic energy of the system
        double totalEnergy()
        {
            var energy = 0.0;
            this.eachNode((node, point) =>
            {
                var speed = point.v.magnitude();
                energy += 0.5 * point.m * speed * speed;
            });

            return energy;
        }

        bool _started;
        bool _stop;
        double minEnergyThreshold = 0.00001;
        public void Update()
        {

            tick(0.03);

            /*if (render !== undefined)
            {
                render();
            }*/

            // stop simulation when energy of the system goes below a threshold
            if (_stop || totalEnergy() < minEnergyThreshold)
            {
                _started = false;
                //if (onRenderStop !== undefined) { onRenderStop(); }
            }
            else
            {
                //Springy.requestAnimationFrame(step);
            }
        }

        void applyHookesLaw()
        {
            this.eachSpring((spring) =>
            {
                var d = spring.point2.p.subtract(spring.point1.p); // the direction of the spring
                var displacement = spring.length - d.magnitude();
                var direction = d.normalise();

                // apply force to each end point
                spring.point1.applyForce(direction.multiply(spring.k * displacement * -0.5));
                spring.point2.applyForce(direction.multiply(spring.k * displacement * 0.5));
            });
        }

        void attractToCentre()
        {
            this.eachNode((node, point) =>
            {
                var direction = point.p.multiply(-1.0);
                point.applyForce(direction.multiply(this.repulsion / 50.0));
            });
        }

        void updateVelocity(double timestep)
        {
            this.eachNode((node, point) =>
            {
                // Is this, along with updatePosition below, the only places that your
                // integration code exist?
                point.v = point.v.add(point.a.multiply(timestep)).multiply(this.damping);
                if (point.v.magnitude() > this.maxSpeed)
                {
                    point.v = point.v.normalise().multiply(this.maxSpeed);
                }
                point.a = new Vector(0, 0);
            });
        }

        public Dictionary<int, ForceDirectedPoint> nodePoints = new Dictionary<int, ForceDirectedPoint>();
        double repulsion = 400;
        double damping = 0.5;
        double maxSpeed = double.PositiveInfinity; //maxSpeed || Infinity; // nodes aren't allowed to exceed this speed
        ForceDirectedPoint point(Node node)
        {
            if (!(this.nodePoints.ContainsKey(node.id)))
            {
                //var mass = (node.data.mass !== undefined) ? node.data.mass : 1.0;
                var mass = 1.0;
                this.nodePoints[node.id] = new ForceDirectedPoint(Vector.random(), mass);
            }

            return this.nodePoints[node.id];
        }

        public Graph graph;
        // callback should accept two arguments: Node, Point
        public void eachNode(Action<Node, ForceDirectedPoint> callback)
        {
            var t = this;
            this.graph.nodes.ForEach((n) =>
            {
                callback(n, t.point(n));
            });
        }
        // callback should accept two arguments: Edge, Spring
        public void eachEdge(Action<Edge, Spring> callback)
        {
            var t = this;
            this.graph.edges.ForEach((e) =>
            {
                callback(e, t.spring(e));
            });
        }

        public void applyCoulombsLaw()
        {
            this.eachNode((n1, point1) =>
            {
                this.eachNode((n2, point2) =>
                {
                    //if (point1 !== point2)  ref or value????
                    if (point1 != point2)
                    {
                        var d = point1.p.subtract(point2.p);
                        var distance = d.magnitude() + 0.1; // avoid massive forces at small distances (and divide by zero)
                        var direction = d.normalise();

                        // apply force to each end point
                        point1.applyForce(direction.multiply(this.repulsion).divide(distance * distance * 0.5));
                        point2.applyForce(direction.multiply(this.repulsion).divide(distance * distance * -0.5));
                    }
                });
            });
        }
    }

  

}
