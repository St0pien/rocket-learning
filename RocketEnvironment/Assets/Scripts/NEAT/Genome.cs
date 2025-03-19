using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT
{
    [Serializable]
    public class Genome
    {
        public int Id;
        public List<NodeGene> NodeGenes;
        public List<ConnectionGene> ConnectionGenes;
        public float Fitness = 0f;

        public NodeGene GetNode(int id)
        {
            return NodeGenes.Find(n => n.Id == id);
        }

        public NodeGene GetRandomInputNode()
        {
            var ins = NodeGenes.Where(n => n.Type != NodeType.Output).ToList();
            return ins[UnityEngine.Random.Range(0, ins.Count)];
        }

        public NodeGene GetRandomOutputNode()
        {
            var outs = NodeGenes.Where(n => n.Type != NodeType.Sensor).ToList();
            return outs[UnityEngine.Random.Range(0, outs.Count)];
        }

        public ConnectionGene GetConnectionGene(Connection connection)
        {
            return ConnectionGenes.Find(c => c.Connection.Equals(connection));
        }

        public ConnectionGene GetRandomActiveConnectionGene()
        {
            var active = ConnectionGenes.Where(c => c.Status == ConnectionStatus.Enabled).ToList();
            return active[UnityEngine.Random.Range(0, active.Count)];
        }
    }
}
