using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NEAT
{
    public struct PopulationConfig
    {
        public int PopulationSize;
        public int InputSize;
        public int OutputSize;
        public bool FullyConnected;
        public float GeneralMutationChance;
        public float TweakWeightMutationProb;
        public float NewConnectionMutationProb;
        public float NewNodeMutationProb;
    }

    [Serializable]
    public class Population
    {
        public List<Genome> genomes;
        public int LastInnovation = 0;
        public int LastNode = 0;

        private readonly PopulationConfig config;
        private List<MutationPick> mutationDistribution;
        private Dictionary<Connection, int> introducedNodes = new Dictionary<Connection, int>(); // in last generation
        private Dictionary<Connection, int> introducedConnections = new Dictionary<Connection, int>(); // in last generation

        public Population(PopulationConfig config)
        {
            this.config = config;
            LastNode = config.InputSize + config.OutputSize;
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
            ConfigureMutations();
        }

        private List<ConnectionGene> getFullyConnectedNodes(IEnumerable<NodeGene> inputs, IEnumerable<NodeGene> outputs)
        {
            LastInnovation = config.InputSize * config.OutputSize;
            int n = 0;
            return outputs.SelectMany(o => inputs.Select(i => new ConnectionGene(i.Id, o.Id, (n++ % LastInnovation) + 1))).ToList();
        }

        private void ConfigureMutations()
        {
            var mutConfig = new MutationConfig()
                .AddMutator(config.TweakWeightMutationProb, new TweakWeightMutator())
                .AddMutator(config.NewConnectionMutationProb, new NewConnectionMutator(GetNextInnovation))
                .AddMutator(config.NewNodeMutationProb, new NewNodeMutator(GetNextNodeId, GetNextInnovation));

            mutationDistribution = mutConfig.GetMutationDistribution();
        }

        public void Mutate()
        {
            introducedNodes.Clear();
            introducedConnections.Clear();
            var genomesToMutate = new List<Genome>();
            foreach (var genome in genomes)
            {
                if (UnityEngine.Random.Range(0f, 1f) < config.GeneralMutationChance)
                {
                    genomesToMutate.Add(genome);
                }
            }

            foreach (var genome in genomesToMutate)
            {
                var rnd = UnityEngine.Random.Range(0f, 1f);
                var index = 0;
                while (rnd > mutationDistribution[index].Limit) index++;
                var mutator = mutationDistribution[index].Mutator;
                mutator.Mutate(genome);
            }
        }

        private int GetNextNodeId(Connection connection)
        {
            if (introducedNodes.ContainsKey(connection))
            {
                UnityEngine.Debug.Log("hit Node");
                return introducedNodes[connection];
            }

            introducedNodes[connection] = ++LastNode;
            return LastNode;
        }

        private int GetNextInnovation(Connection connection)
        {
            if (introducedConnections.ContainsKey(connection))
            {
                UnityEngine.Debug.Log("hit innovation");
                return introducedConnections[connection];
            }

            introducedConnections[connection] = ++LastInnovation;
            return LastInnovation;
        }
    }
}