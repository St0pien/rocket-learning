using System;

namespace NEAT2
{
    [Serializable]
    public enum NodeGeneType
    {
        Sensor,
        Hidden,
        Output
    }

    [Serializable]
    public class NodeGene
    {
        public int Id;
        public NodeGeneType Type;

        public NodeGene(int id, NodeGeneType type)
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