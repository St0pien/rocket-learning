using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;

namespace NEAT
{
    [Serializable]
    public class ReproductionConfig
    {
        public int Elitism = 0;
        public float SurvivalThreshold = 0.2f;
        public int MinSpeciesSize = 1;
    }

    public class ReproductionModule
    {
        private int LastGenomeId = 0;

        private ReproductionConfig config;
        private StagnationModule stagnation;

        public ReproductionModule(ReproductionConfig cfg, StagnationModule stagnation)
        {
            config = cfg;
            this.stagnation = stagnation;
        }

        public void SetLastGenomeId(int id)
        {
            LastGenomeId = id;
        }

        public Dictionary<int, Genome> GetInitialPopulation(GenomeModule genomeModule, int size)
        {
            return Enumerable.Range(0, size).Select(_ =>
            {
                var g = new Genome(GetNextGenomeId(), genomeModule);
                g.Init();
                return g;
            }).ToDictionary(g => g.Id);
        }

        public Dictionary<int, Genome> Reproduce(GenomeModule genomeModule, Dictionary<int, Species> species, int popSize, int generation, int BestId)
        {
            // Remove stagnant species
            var stagnant = stagnation.MarkStagnant(species, generation);
            foreach (var (id, isStagnant) in stagnant)
            {
                if (isStagnant)
                {
                    species.Remove(id);
                }
            }

            if (species.Count == 0)
            {
                return new Dictionary<int, Genome>();
            }

            var minFitness = species.Values.Min(s => s.Fitness);
            var maxFitness = species.Values.Max(s => s.Fitness);
            var fitnessRange = Math.Max(1f, maxFitness - minFitness);

            // Compute adjusted fitness
            foreach (var (sid, s) in species)
            {
                var avg = s.Members.Values.Average(g => g.Fitness);
                var avgNorm = (avg - minFitness) / fitnessRange;
                s.AdjustedFitness = avgNorm;
            }

            var spawnAmounts = ComputeSpawns(species, popSize, config.MinSpeciesSize);
            var result = new Dictionary<int, Genome>();

            genomeModule.ResetIndexers();
            foreach (var (sid, s) in species)
            {
                var spawn = Math.Max(spawnAmounts[sid], config.Elitism);

                var ancestors = s.Members.Values.OrderByDescending(m => m.Fitness).ToList();
                s.Members = new Dictionary<int, Genome>();

                // Preserve elites
                if (config.Elitism > 0)
                {
                    foreach (var g in ancestors.Take(config.Elitism))
                    {
                        result.Add(g.Id, g);
                        spawn--;
                    }
                }

                if (spawn <= 0) continue;

                var killCutoff = (int)Math.Ceiling(config.SurvivalThreshold * ancestors.Count);
                ancestors = ancestors.Take(killCutoff).ToList();

                for (int i = 0; i < spawn; i++)
                {
                    var p1 = ancestors[UnityEngine.Random.Range(0, ancestors.Count)];
                    var p2 = ancestors[UnityEngine.Random.Range(0, ancestors.Count)];

                    var child = Crossover(p1, p2, genomeModule);
                    if (child.Id != BestId)
                    {
                        child.Mutate();
                    }
                    result.Add(child.Id, child);
                }
            }

            return result;
        }

        private int GetNextGenomeId()
        {
            return ++LastGenomeId;
        }

        private Dictionary<int, int> ComputeSpawns(Dictionary<int, Species> species, int popSize, int minSpeciesSize)
        {
            var adjSum = species.Values.Sum(s => s.AdjustedFitness);

            var result = new Dictionary<int, int>();
            foreach (var (sid, s) in species)
            {
                var proportion = adjSum > 0 ? Math.Max(minSpeciesSize, s.AdjustedFitness / adjSum * popSize) : minSpeciesSize;

                var diff = (proportion - s.Members.Count) * 0.5;
                var diffInt = (int)Math.Round(diff);
                var spawn = s.Members.Count;
                if (Math.Abs(diffInt) > 0)
                {
                    spawn += diffInt;
                }
                else if (diff > 0)
                {
                    spawn++;
                }
                else if (diff < 0)
                {
                    spawn--;
                }
                result.Add(sid, spawn);
            }

            var totalSpawn = result.Values.Sum();
            double norm = popSize / totalSpawn;

            foreach (var key in result.Keys.ToList())
            {
                result[key] = Math.Max(minSpeciesSize, (int)Math.Round(result[key] * norm));
            }

            return result;
        }

        private Genome Crossover(Genome g1, Genome g2, GenomeModule genomeModule)
        {
            if (g1.Fitness < g2.Fitness)
            {
                return Crossover(g2, g1, genomeModule);
            }

            var child = new Genome(GetNextGenomeId(), genomeModule);

            foreach (var (key, cg1) in g1.ConnectionGenes)
            {
                if (g2.ConnectionGenes.ContainsKey(key))
                {
                    // Mix matching genes
                    var cg2 = g2.ConnectionGenes[key];
                    child.ConnectionGenes.Add(key, new ConnectionGene(key, cg1.Connection)
                    {
                        Weight = UnityEngine.Random.Range(0f, 1f) > 0.5 ? cg1.Weight : cg2.Weight,
                        Status = UnityEngine.Random.Range(0f, 1f) > 0.5 ? cg1.Status : cg2.Status
                    });
                }
                else
                {
                    // Clone from fittest pattern
                    child.ConnectionGenes.Add(key, cg1.Clone());
                }
            }

            child.NodeGenes = g1.NodeGenes.Values.Select(ng => ng.Clone()).ToDictionary(ng => ng.Id);

            return child;
        }
    }
}