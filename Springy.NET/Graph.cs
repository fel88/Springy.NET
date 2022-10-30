using System.Collections.Generic;

namespace Springy.Lib
{
    public class Graph
    {
        int nextNodeId = 0;
        int nextEdgeId = 0;

        Dictionary<int, Dictionary<int, List<Edge>>> adjacency = new Dictionary<int, Dictionary<int, List<Edge>>>();
        // find the edges from node1 to node2
        public Edge[] getEdges(Node node1, Node node2)
        {
            if (this.adjacency.ContainsKey(node1.id)
                && this.adjacency[node1.id].ContainsKey(node2.id))
            {
                return this.adjacency[node1.id][node2.id].ToArray();
            }
            return new Edge[] { };
        }

        public Node newNode(object data)
        {
            var node = new Node(this.nextNodeId++, data);
            this.addNode(node);
            return node;
        }
        public Edge newEdge(Node source, Node target, object data)
        {
            var edge = new Edge(this.nextEdgeId++, source, target, data);
            this.addEdge(edge);
            return edge;
        }
        Edge addEdge(Edge edge)
        {
            var exists = false;
            this.edges.ForEach((e) =>
            {
                if (edge.id == e.id) { exists = true; }
            });
            if (!exists)
            {
                this.edges.Add(edge);
            }
            if (!(this.adjacency.ContainsKey(edge.source.id)))
            {
                this.adjacency[edge.source.id] = new Dictionary<int, List<Edge>>();
            }
            if (!(this.adjacency[edge.source.id].ContainsKey(edge.target.id)))
            {
                this.adjacency[edge.source.id][edge.target.id] = new List<Edge>();
            }
            exists = false;
            this.adjacency[edge.source.id][edge.target.id].ForEach((e) =>
            {
                if (edge.id == e.id) { exists = true; }
            });

            if (!exists)
            {
                this.adjacency[edge.source.id][edge.target.id].Add(edge);
            }

            this.notify();
            return edge;


        }


        public List<Node> nodes = new List<Node>();
        public List<Edge> edges = new List<Edge>();
        Dictionary<int, Node> nodeSet = new Dictionary<int, Node>();
        Node addNode(Node node)
        {
            if (!(nodeSet.ContainsKey(node.id)))
            {
                this.nodes.Add(node);
            }

            this.nodeSet[node.id] = node;

            this.notify();
            return node;
        }


        void notify()
        {

        }
    }

  

}
