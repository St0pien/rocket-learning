using UnityEngine;

namespace NEAT
{
    public class TweakWeightMutator : IMutator
    {
        public void Mutate(Genome genome)
        {
            var rand = Random.Range(0, genome.ConnectionGenes.Count);
            genome.ConnectionGenes[rand].Weight = Random.Range(0f, 1f);
        }
    }
}