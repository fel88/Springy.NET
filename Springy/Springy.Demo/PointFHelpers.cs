using Springy.Lib;
using System.Drawing;

namespace Springy.Demo
{
    public static class PointFHelpers
    {
        public static Vector add(this PointF f, Vector add)
        {
            return new Vector((float)(f.X + add.x), (float)(f.Y + add.y));
        }
    }
}
