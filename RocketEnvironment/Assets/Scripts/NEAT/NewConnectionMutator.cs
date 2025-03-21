using System.Collections.Generic;
using System.Linq;

namespace NEAT
{
    public class NewConnectionMutator : IMutator
    {
        private IdGetter getInnovationNumber;

        public NewConnectionMutator(IdGetter innovationNumberGetter)
        {
            getInnovationNumber = innovationNumberGetter;
        }

        public void Mutate(Genome genome)
        {
            NodeGene i = genome.GetRandomInputNode();
            NodeGene o = genome.GetRandomOutputNode();

            if (i.Id == o.Id) return;
            var conn = new Connection(i.Id, o.Id);
            var existing = genome.GetConnectionGene(conn);
            if (existing != null)
            {
                existing.Status = ConnectionStatus.Enabled;
                return;
            }
            if (PathExists(genome, conn))
            {
                UnityEngine.Debug.Log($"Detected potential cycle genome {genome.Id}: {conn.Input} x {conn.Output}");
                return;
            }

            var gene = new ConnectionGene(conn, getInnovationNumber(conn));
            genome.ConnectionGenes.Add(gene);
            UnityEngine.Debug.Log($"New connection mutation in genome {genome.Id}: {conn.Input} x {conn.Output}");
        }

        private bool PathExists(Genome genome, Connection conn)
        {
            var visited = new HashSet<int>();
            var stack = new Stack<int>();
            stack.Push(conn.Output);
            while (stack.Count > 0)
            {
                int v = stack.Pop();
                if (v == conn.Input)
                {
                    return true;
                }
                visited.Add(v);
                var neighbors = genome.ConnectionGenes
                    .Where(c => c.Connection.Input == v && !visited.Contains(c.Connection.Output))
                    .Select(c => c.Connection.Output);
                foreach (var n in neighbors)
                {
                    stack.Push(n);
                }
            }

            return false;
        }
    }
}