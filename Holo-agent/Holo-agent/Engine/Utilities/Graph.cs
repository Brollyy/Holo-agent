using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Utilities
{
    [DataContract]
    public class GraphNode<T,U>
    {
        [DataMember]
        public T Value
        {
            get; set;
        } = default(T);

        [DataMember]
        public List<GraphNode<T,U>> Neighbours
        {
            get; set;
        } = new List<GraphNode<T,U>>();

        [DataMember]
        public List<U> Costs
        {
            get; set;
        } = new List<U>();

        public GraphNode() { }
        public GraphNode(T value) { Value = value; }
    }

    [CollectionDataContract]
    public class Graph<T,U> : IEnumerable<GraphNode<T,U>>
    {
        [DataMember]
        private List<GraphNode<T,U>> nodeSet;

        public Graph() : this(null) { }
        public Graph(List<GraphNode<T,U>> nodeSet)
        {
            if (nodeSet == null)
                this.nodeSet = new List<GraphNode<T,U>>();
            else
                this.nodeSet = nodeSet;
        }

        public void AddNode(GraphNode<T,U> node)
        {
            // adds a node to the graph
            nodeSet.Add(node);
        }

        public void AddNode(T value)
        {
            // adds a node to the graph
            nodeSet.Add(new GraphNode<T,U>(value));
        }

        public void AddUndirectedEdge(GraphNode<T,U> from, GraphNode<T,U> to, U cost)
        {
            from.Neighbours.Add(to);
            from.Costs.Add(cost);

            to.Neighbours.Add(from);
            to.Costs.Add(cost);
        }

        public bool Contains(T value)
        {
            return nodeSet.Find(x => x.Value.Equals(value)) != null;
        }

        public bool Remove(T value)
        {
            // first remove the node from the nodeset
            GraphNode<T, U> nodeToRemove = nodeSet.Find(x => x.Value.Equals(value));
            if (nodeToRemove == null)
                // node wasn't found
                return false;

            // otherwise, the node was found
            nodeSet.Remove(nodeToRemove);

            // enumerate through each node in the nodeSet, removing edges to this node
            foreach (GraphNode<T,U> gnode in nodeSet)
            {
                int index = gnode.Neighbours.IndexOf(nodeToRemove);
                if (index != -1)
                {
                    // remove the reference to the node and associated cost
                    gnode.Neighbours.RemoveAt(index);
                    gnode.Costs.RemoveAt(index);
                }
            }

            return true;
        }

        public IEnumerator<GraphNode<T,U>> GetEnumerator()
        {
            return nodeSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return nodeSet.GetEnumerator();
        }

        public List<GraphNode<T,U>> Nodes
        {
            get
            {
                return nodeSet;
            }
        }

        public int Count
        {
            get { return nodeSet.Count; }
        }

        public void Add(Object go)
        {

        }
    }
}
