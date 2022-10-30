namespace Springy.Lib
{
    public class Node
    {
        public int id;
        public object Tag;
        public Node(int id, object tag)
        {
            this.id = id;
            Tag = tag;
        }

        public int getHeight()
        {
            return 15;
        }

        public int getWidth()
        {
            return 15;
        }
    }

  

}
