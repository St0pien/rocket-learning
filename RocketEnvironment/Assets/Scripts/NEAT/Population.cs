using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT
{
    public enum MutationType
    {
        TweakWeight,
        AddNode,
        AddConnection
    }

    public struct PopulationConfig
    {
        public int PopulationSize;
        public int InputSize;
        public int OutputSize;
        public bool FullyConnected;
        public float GeneralMutationChance;
    }

    [Serializable]
    public class Population
    {
        public List<Genome> genomes;
        public int LastInnovation = 0;

        private readonly PopulationConfig config;

        public Population(PopulationConfig config)
        {
            this.config = config;
            genomes = Enumerable.Range(1, config.PopulationSize).Select(g =>
            {
                var inputs = Enumerable.Range(1, config.InputSize).Select(i => new NodeGene(i, NodeType.Sensor));
                var outputs = Enumerable.Range(config.InputSize + 1, config.OutputSize).Select(i => new NodeGene(i, NodeType.Output));
                return new Genome()
                {
                    Id = g,
                    NodeGenes = inputs.Concat(outputs).ToList(),
                    ConnectionGenes = config.FullyConnected ? getFullyConnectedNodes(inputs, outputs) : new List<ConnectionGene>()
                };
            }).ToList();
        }

        private List<ConnectionGene> getFullyConnectedNodes(IEnumerable<NodeGene> inputs, IEnumerable<NodeGene> outputs)
        {
            LastInnovation = config.InputSize * config.OutputSize;
            int n = 0;
            return outputs.SelectMany(o => inputs.Select(i => new ConnectionGene(i.Id, o.Id, ((n++) % LastInnovation) + 1))).ToList();
        }


        public void Mutate()
        {
            var genomesToMutate = new List<Genome>();
            foreach (var genome in genomes)
            {
                if (UnityEngine.Random.Range(0f, 1f) < config.GeneralMutationChance)
                {
                    genomesToMutate.Add(genome);
                }
            }

            UnityEngine.Random.Range(0, 3);
        }
    }
}