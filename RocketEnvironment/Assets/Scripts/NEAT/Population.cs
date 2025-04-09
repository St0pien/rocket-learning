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
        public float CompatiblityThreshold;
        public float ExcessCompatiblityFactor;
        public float DisjointCompatiblityFactor;
        public float WeightCompatiblityFactor;
    }

    [Serializable]
    public class Population
    {
        public List<Species> Species;
        private int lastInnovation = 0;
        private int lastNode = 0;
        private int lastGenomeId = 0;

        private readonly PopulationConfig config;
        private RandomPool<IMutator> mutationPool;
        private Dictionary<Connection, int> introducedNodes = new Dictionary<Connection, int>(); // in last generation
        private Dictionary<Connection, int> introducedConnections = new Dictionary<Connection, int>(); // in last generation
        private HashSet<int> elite = new HashSet<int>();

        public Population(PopulationConfig config)
        {
            this.config = config;
            lastNode = config.InputSize + config.OutputSize;
            lastGenomeId = config.PopulationSize;
            var initialGenomes = Enumerable.Range(1, config.PopulationSize).Select(g =>
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
            Mutate(initialGenomes);
            Species = GroupIntoSpecies(initialGenomes);
        }

        private List<ConnectionGene> getFullyConnectedNodes(IEnumerable<NodeGene> inputs, IEnumerable<NodeGene> outputs)
        {
            lastInnovation = config.InputSize * config.OutputSize;
            int n = 0;
            return outputs.SelectMany(o => inputs.Select(i => new ConnectionGene(i.Id, o.Id, (n++ % lastInnovation) + 1))).ToList();
        }

        private void ConfigureMutations()
        {
            mutationPool = new RandomPool<IMutator>(new (IMutator, float)[] {
                (new TweakWeightMutator(), config.TweakWeightMutationProb),
                (new NewConnectionMutator(GetNextInnovation), config.NewConnectionMutationProb),
                (new NewNodeMutator(GetNextNodeId, GetNextInnovation), config.NewNodeMutationProb),
             });
        }

        private void Mutate(List<Genome> genomes)
        {
            introducedNodes.Clear();
            introducedConnections.Clear();
            var genomesToMutate = new List<Genome>();
            foreach (var genome in genomes)
            {
                if (!elite.Contains(genome.Id) && UnityEngine.Random.Range(0f, 1f) < config.GeneralMutationChance)
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

            introducedNodes[connection] = ++lastNode;
            return lastNode;
        }

        private int GetNextInnovation(Connection connection)
        {
            if (introducedConnections.ContainsKey(connection))
            {
                UnityEngine.Debug.Log("hit innovation");
                return introducedConnections[connection];
            }

            introducedConnections[connection] = ++lastInnovation;
            return lastInnovation;
        }

        private List<Species> GroupIntoSpecies(List<Genome> genomes)
        {
            var species = new List<Species>();
            foreach (var g in genomes)
            {
                PutToCompatibleSpecies(species, g);
            }

            return species;
        }

        private Species PutToCompatibleSpecies(List<Species> species, Genome genome)
        {
            foreach (var s in species)
            {
                if (CompatibilityDistance(s.Representative, genome) < config.CompatiblityThreshold)
                {
                    s.Genomes.Add(genome);
                    return s;
                }
            }

            var newSpecies = new Species(genome, () => ++lastGenomeId);
            species.Add(newSpecies);
            return newSpecies;
        }

        private float CompatibilityDistance(Genome g1, Genome g2)
        {
            int i = 0;
            int j = 0;
            int disjoint = 0;
            float weightDifferenceSum = 0;
            while (i < g1.ConnectionGenes.Count && j < g2.ConnectionGenes.Count)
            {
                if (g1.ConnectionGenes[i].Innovation < g2.ConnectionGenes[j].Innovation)
                {
                    disjoint++;
                    i++;
                    continue;
                }
                if (g1.ConnectionGenes[i].Innovation > g2.ConnectionGenes[j].Innovation)
                {
                    disjoint++;
                    j++;
                    continue;
                }
                weightDifferenceSum += Math.Abs(g1.ConnectionGenes[i].Weight - g2.ConnectionGenes[j].Weight);
                i++;
                j++;
            }
            int excess = i == g1.ConnectionGenes.Count ? g2.ConnectionGenes.Count - j : g1.ConnectionGenes.Count - i;
            // int N = Math.Max(g1.ConnectionGenes.Count, g2.ConnectionGenes.Count);
            int N = 1;

            return (config.ExcessCompatiblityFactor * excess / N) + (config.DisjointCompatiblityFactor * disjoint / N) + (config.WeightCompatiblityFactor * weightDifferenceSum / N);
        }

        public IEnumerable<Genome> GetAllGenomes()
        {
            return Species.SelectMany(s => s.Genomes).OrderBy(g => g.Fitness);
        }

        public void NextGeneration()
        {
            var nextGeneration = new List<Genome>(GetAllGenomes().TakeLast(config.Elitism));
            elite = new HashSet<int>(nextGeneration.Select(g => g.Id));

            var speciesWithAdjustedFitness = Species.Select(s => (species: s, fitness: s.Genomes.Sum(g => g.Fitness) / s.Genomes.Count));
            var overallFitness = speciesWithAdjustedFitness.Sum(s => s.Item2);

            foreach (var (species, fitness) in speciesWithAdjustedFitness)
            {
                int quantity = Math.Max(0, (int)(fitness / overallFitness * (config.PopulationSize - config.Elitism)));
                nextGeneration.AddRange(species.ProduceOffsprings(quantity, config.SurvivalThreshold));
            }
            var bestSpecies = speciesWithAdjustedFitness.OrderByDescending(p => p.fitness).First().species;
            nextGeneration.AddRange(bestSpecies.ProduceOffsprings(config.PopulationSize - nextGeneration.Count, config.SurvivalThreshold));

            Species = GroupIntoSpecies(nextGeneration);
            introducedNodes.Clear();
            introducedConnections.Clear();

            Mutate(GetAllGenomes().ToList());
        }
    }
}