using System;

namespace NEAT
{
    public enum NodeType
    {
        Sensor,
        Hidden,
        Output
    }

    [Serializable]
    public class NodeGene
    {
        public int Id;
        public NodeType Type;

        public NodeGene(int id, NodeType type)
        {
            Id = id;
            Type = type;
        }

        public NodeGene Clone()
        {
            return new NodeGene(Id, Type);
        }
    }
}