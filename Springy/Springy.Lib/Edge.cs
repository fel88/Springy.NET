namespace Springy.Lib
{
    public class Edge
    {
        public int id;
        public object data;
        public Edge(int id, Node source, Node target, object data)
        {
            this.id = id;
            this.source = source;
            this.target = target;
            this.data = data;
        }
        public Node source;
        public Node target;
    } 

}
