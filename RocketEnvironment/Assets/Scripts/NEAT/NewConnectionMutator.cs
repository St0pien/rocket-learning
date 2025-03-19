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

            var gene = new ConnectionGene(conn, getInnovationNumber(conn));
            genome.ConnectionGenes.Add(gene);
            UnityEngine.Debug.Log($"New connection mutation in genome {genome.Id}: {conn.Input} x {conn.Output}");
        }
    }
}