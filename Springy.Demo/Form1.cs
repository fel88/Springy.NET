using Springy.Lib;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Springy.Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            initGraph();
            dd = new LayoutForceDirected();
            dd.graph = graph;
            dd.getBoundingBox();
            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            Load += Form1_Load;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;
        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);

            if (e.Button == MouseButtons.Left)
            {
                isDrag = true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }



        }

        public float ZoomFactor = 1.5f;

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);
            if (!pictureBox1.ClientRectangle.IntersectsWith(new Rectangle(pos.X, pos.Y, 1, 1)))
            {
                return;
            }

            float zold = zoom;

            if (e.Delta > 0) { zoom *= ZoomFactor; } else { zoom /= ZoomFactor; }

            if (zoom < 0.0008) { zoom = 0.0008f; }
            if (zoom > 10000) { zoom = 10000f; }

            sx = -(pos.X / zold - sx - pos.X / zoom);
            sy = (pos.Y / zold + sy - pos.Y / zoom);
        }

        public float startx { get; set; }
        public float starty { get; set; }
        public float sx { get; set; } = 10;
        public float sy { get; set; } = -10;


        public float origsx, origsy;
        public bool isDrag = false;
        public float zoom { get; set; } = 40;
        public void UpdateDrag()
        {
            if (isDrag)
            {
                var p = pictureBox1.PointToClient(Cursor.Position);

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + (-(p.Y - starty) / zoom);
            }
        }
        LayoutForceDirected dd;

        PointF toScreen(Vector v)
        {
            return toScreen((float)v.x, (float)v.y);
        }

        public virtual PointF toScreen(float x, float y)
        {
            return new PointF((x + sx) * zoom, -(y + sy) * zoom);
        }

        // helpers for figuring out where to draw arrows
        public Vector intersect_line_line(Vector p1, Vector p2, Vector p3, Vector p4)
        {
            var denom = ((p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y));

            // lines are parallel
            if (denom == 0)
            {
                return null;
            }

            var ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denom;
            var ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denom;

            if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
            {
                return null;
            }

            return new Vector(p1.x + ua * (p2.x - p1.x), p1.y + ua * (p2.y - p1.y));
        }

        public Vector intersect_line_box(Vector p1, Vector p2, Vector p3, double w, double h)
        {
            var tl = new Vector() { x = p3.x, y = p3.y };
            var tr = new Vector() { x = p3.x + w, y = p3.y };
            var bl = new Vector() { x = p3.x, y = p3.y + h };
            var br = new Vector() { x = p3.x + w, y = p3.y + h };

            Vector result = null;
            if ((result = intersect_line_line(p1, p2, tl, tr)) != null) { return result; } // top
            if ((result = intersect_line_line(p1, p2, tr, br)) != null) { return result; } // right
            if ((result = intersect_line_line(p1, p2, br, bl)) != null) { return result; } // bottom
            if ((result = intersect_line_line(p1, p2, bl, tl)) != null) { return result; } // left

            return null;
        }
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            UpdateDrag();
            e.Graphics.Clear(Color.White);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            dd.Update();
            var gr = e.Graphics;
            int ww = 40;
            var bb = dd.getBoundingBox();
            double width = bb.topright.x - bb.bottomleft.x;
            //float scale = (float)(pictureBox1.Width / width);


            dd.eachEdge((p, gg) =>
            {
                var p1 = gg.point1.p;
                var p2 = gg.point2.p;
                var len = p2.subtract(p1).magnitude();
                var dir = p2.subtract(p1).normalise();
                dynamic dat = p.data;

                string colorcode = dat.color;
                uint argb = UInt32.Parse(colorcode.Replace("#", ""), NumberStyles.HexNumber);
                argb |= 0xff000000;
                Color clr = Color.FromArgb((int)argb);

                var gap1 = 0.2;
                var gap2 = 0.8;
                var pp1 = p1.add(dir.multiply(len * gap1));
                var pp2 = p1.add(dir.multiply(len * gap2));
                var size = 6;
                AdjustableArrowCap bigArrow = new AdjustableArrowCap(size, size, true);
                Pen pen1 = new Pen(clr);
                pen1.CustomEndCap = bigArrow;
                var x1 = toScreen(p1).X;
                var y1 = toScreen(p1).Y;
                var x2 = toScreen(p2).X;
                var y2 = toScreen(p2).Y;

                var direction = new Vector(x2 - x1, y2 - y1);
                var normal = direction.normal().normalise();

                var edge = p;

                var from = graph.getEdges(edge.source, edge.target);
                var to = graph.getEdges(edge.target, edge.source);
                var total = from.Length + to.Length;
                // Figure out edge's position in relation to other edges between the same nodes
                var n = 0;
                for (var i = 0; i < from.Length; i++)
                {
                    if (from[i].id == edge.id)
                    {
                        n = i;
                    }
                }

                //change default to  10.0 to allow text fit between edges
                var spacing = 12.0;

                // Figure out how far off center the line should be drawn
                var offset = normal.multiply(-((total - 1) * spacing) / 2.0 + (n * spacing));

                var paddingX = 6;
                var paddingY = 6;

                var s1 = toScreen(p1).add(offset);
                var s2 = toScreen(p2).add(offset);

                var boxWidth = edge.target.getWidth() + paddingX;
                var boxHeight = edge.target.getHeight() + paddingY;

                var intersection = intersect_line_box(s1, s2, new Vector() { x = x2 - boxWidth / 2.0, y = y2 - boxHeight / 2.0 }, boxWidth, boxHeight);

                if (intersection == null)
                {
                    intersection = s2;
                }
                var lineEnd = s2;
                //gr.DrawLine(pen1, toScreen(s1), toScreen(lineEnd));
                gr.DrawLine(pen1, toScreen(pp1), toScreen(pp2));
            });

            dd.eachNode((p, gg) =>
            {
                var sc1 = toScreen(gg.p);
                //gr.DrawEllipse(Pens.Black, -ww / 2 + sc1.X, -ww / 2 + sc1.Y, ww, ww);
                dynamic d = p.Tag;
                var lab = d.label;
                var ms = gr.MeasureString(lab, SystemFonts.DefaultFont);
                gr.DrawString(lab, SystemFonts.DefaultFont, Brushes.Black, sc1.X - ms.Width / 2, sc1.Y - ms.Height / 2);
            });


        }

        Graph graph = new Graph();
        void initGraph()
        {

            var dennis = graph.newNode(new
            {
                label = "Dennis",
            });

            var michael = graph.newNode(new { label = "Michael" });
            var jessica = graph.newNode(new { label = "Jessica" });
            var timothy = graph.newNode(new { label = "Timothy" });
            var barbara = graph.newNode(new { label = "Barbara" });
            var franklin = graph.newNode(new { label = "Franklin" });
            var monty = graph.newNode(new { label = "Monty" });
            var james = graph.newNode(new { label = "James" });
            var bianca = graph.newNode(new { label = "Bianca" });

            graph.newEdge(dennis, michael, new { color = "#00A0B0" });
            graph.newEdge(michael, dennis, new { color = "#6A4A3C" });
            graph.newEdge(michael, jessica, new { color = "#CC333F" });
            graph.newEdge(jessica, barbara, new { color = "#EB6841" });
            graph.newEdge(michael, timothy, new { color = "#EDC951" });
            graph.newEdge(franklin, monty, new { color = "#7DBE3C" });
            graph.newEdge(dennis, monty, new { color = "#000000" });
            graph.newEdge(monty, james, new { color = "#00A0B0" });
            graph.newEdge(barbara, timothy, new { color = "#6A4A3C" });
            graph.newEdge(dennis, bianca, new { color = "#CC333F" });
            graph.newEdge(bianca, monty, new { color = "#EB6841" });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }
    }
}
