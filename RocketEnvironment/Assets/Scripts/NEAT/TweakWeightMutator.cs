using Math = System.Math;
using UnityEngine;

namespace NEAT
{
    public class TweakWeightMutator : IMutator
    {
        public void Mutate(Genome genome)
        {
            Debug.Log($"Weight mutator: genome {genome.Id}");
            var rand = Random.Range(0, genome.ConnectionGenes.Count);
            genome.ConnectionGenes[rand].Weight += Random.Range(-1f, 1f);
            genome.ConnectionGenes[rand].Weight = Math.Clamp(genome.ConnectionGenes[rand].Weight, -1, 1);
        }
    }
}