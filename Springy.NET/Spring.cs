
namespace Springy.Lib
{

    public class Spring
    {
        public ForceDirectedPoint point1;
        public ForceDirectedPoint point2;
        public double length; // spring length at rest
        public double k; // spring constant (See Hooke's law) .. how stiff the spring is


        public Spring(ForceDirectedPoint point1, ForceDirectedPoint point2, double v1, double v2)
        {
            this.point1 = point1;
            this.point2 = point2;
            this.length = v1;
            this.k = v2;
        }
    } 

}
