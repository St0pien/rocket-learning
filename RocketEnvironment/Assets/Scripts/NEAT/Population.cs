using System;
using System.Collections.Generic;
using System.Linq;
using Probability;
using UnityEditor;

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
        public float SurvivalThreshold;
        public int Elitism;
    }

    [Serializable]
    public class Population
    {
        public List<Genome> genomes;
        private int LastInnovation = 0;
        private int LastNode = 0;
        private int LastGenomeId = 0;

        private readonly PopulationConfig config;
        private RandomPool<IMutator> mutationPool;
        private Dictionary<Connection, int> introducedNodes = new Dictionary<Connection, int>(); // in last generation
        private Dictionary<Connection, int> introducedConnections = new Dictionary<Connection, int>(); // in last generation
        private HashSet<int> Elite = new HashSet<int>();

        public Population(PopulationConfig config)
        {
            this.config = config;
            LastNode = config.InputSize + config.OutputSize;
            LastGenomeId = config.PopulationSize;
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
            mutationPool = new RandomPool<IMutator>(new (IMutator, float)[] {
                (new TweakWeightMutator(), config.TweakWeightMutationProb),
                (new NewConnectionMutator(GetNextInnovation), config.NewConnectionMutationProb),
                (new NewNodeMutator(GetNextNodeId, GetNextInnovation), config.NewConnectionMutationProb),
             });
        }

        public void Mutate()
        {
            genomes.Sort((a, b) => a.Fitness.CompareTo(b.Fitness));
            introducedNodes.Clear();
            introducedConnections.Clear();
            var genomesToMutate = new List<Genome>();
            foreach (var genome in genomes)
            {
                if (!Elite.Contains(genome.Id) && UnityEngine.Random.Range(0f, 1f) < config.GeneralMutationChance)
                {
                    genomesToMutate.Add(genome);
                }
            }

            foreach (var genome in genomesToMutate)
            {
                mutationPool.Get().Mutate(genome);
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

        public void NextGeneration()
        {
            genomes.Sort((a, b) => a.Fitness.CompareTo(b.Fitness));
            KillWorst();
            var nextGeneration = new List<Genome>(genomes.TakeLast(config.Elitism));
            Elite = new HashSet<int>(nextGeneration.Select(g => g.Id));
            UnityEngine.Debug.Log($"Passing {config.Elitism} genomes without crossover");
            nextGeneration.AddRange(Reproduce(genomes, config.PopulationSize - config.Elitism));

            genomes = nextGeneration;
            introducedNodes.Clear();
            introducedConnections.Clear();
        }

        private void KillWorst()
        {
            int cutOff = (int)(genomes.Count * (1 - config.SurvivalThreshold));
            UnityEngine.Debug.Log($"Killing {cutOff} genomes");
            genomes.RemoveRange(0, cutOff);
        }

        // Assume parents sorted by Fitness
        private List<Genome> Reproduce(List<Genome> parents, int size)
        {
            var pool = new RandomPool<Genome>(parents.Select(p => (p, p.Fitness)));

            return Enumerable.Range(0, size).Select((_) =>
            {
                var p1 = pool.Get();
                var p2 = pool.Get();
                while (p1 == p2) p2 = pool.Get();
                var offspring = GetOffspring(p1, p2);
                UnityEngine.Debug.Log($"Crossing {p1.Id} with {p2.Id} to produce {offspring.Id}");
                return offspring;
            }).ToList();
        }

        private Genome GetOffspring(Genome g1, Genome g2)
        {
            var better = g1.Fitness >= g2.Fitness ? g1 : g2;
            var worse = g1.Fitness < g2.Fitness ? g1 : g2;

            var offspring = new Genome()
            {
                Id = ++LastGenomeId,
                NodeGenes = new List<NodeGene>(),
                ConnectionGenes = new List<ConnectionGene>()
            };

            var nodesAdded = new HashSet<int>();
            int b = 0;
            int w = 0;
            Action<ConnectionGene> passConnection = (gene) =>
            {
                offspring.ConnectionGenes.Add(gene.Clone());

                if (!nodesAdded.Contains(gene.Connection.Input))
                {
                    offspring.NodeGenes.Add(better.GetNode(gene.Connection.Input).Clone());
                    nodesAdded.Add(gene.Connection.Input);
                }
                if (!nodesAdded.Contains(gene.Connection.Output))
                {
                    offspring.NodeGenes.Add(better.GetNode(gene.Connection.Output).Clone());
                    nodesAdded.Add(gene.Connection.Output);
                }
            };

            while (b < better.ConnectionGenes.Count && w < worse.ConnectionGenes.Count)
            {
                if (better.ConnectionGenes[b].Innovation < worse.ConnectionGenes[w].Innovation)
                {
                    // Pass unmatched genes of better parent
                    var gene = better.ConnectionGenes[b++];
                    passConnection(gene);
                }
                else if (better.ConnectionGenes[b].Innovation > worse.ConnectionGenes[w].Innovation)
                {
                    // Skip unmatched genes of worse parent
                    w++;
                }
                else
                {
                    // Pass random matching genes
                    var pool = new RandomPool<ConnectionGene>(new (ConnectionGene, float)[]{
                        (better.ConnectionGenes[b], better.Fitness),
                        (worse.ConnectionGenes[w], worse.Fitness)
                    });
                    var gene = pool.Get();
                    passConnection(gene);
                    w++;
                    b++;
                }
            }

            // Finish passing unmatched genes of better parent
            while (b < better.ConnectionGenes.Count)
            {
                passConnection(better.ConnectionGenes[b++]);
            }

            offspring.NodeGenes.Sort((a, b) => a.Id - b.Id);

            return offspring;
        }
    }
}