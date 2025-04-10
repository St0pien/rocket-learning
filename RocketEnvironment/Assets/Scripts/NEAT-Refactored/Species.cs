using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT2
{
    public class Species
    {
        public int Id;
        public int LastImproved;
        public Dictionary<int, Genome> Members;
        public Genome Representative;
        public float Fitness = float.MinValue;
        public float AdjustedFitness = float.MinValue;
        public List<float> FitnessHistory = new List<float>();

        public Species(int id, int generation)
        {
            Id = id;
            LastImproved = generation;
            Members = new Dictionary<int, Genome>();
        }

        public void SetRepresentative(Genome g)
        {
            Representative = g;
            Members.Add(g.Id, g);
        }
    }

    public class SpeciesConfig
    {
        public float CompatibilityThreshold = 3f;
        public float CompatibilityDisjointCoefficient = 1f;
        public float CompatibilityWeightCoefficient = 0.5f;
    }

    public class SpeciesModule
    {
        private SpeciesConfig config;
        private int LastSpeciesId = 0;
        public Dictionary<int, Species> Species = new Dictionary<int, Species>();

        public SpeciesModule(SpeciesConfig cfg)
        {
            config = cfg;
        }

        public void Speciate(Dictionary<int, Genome> population, int generation)
        {
            // Find new representatives
            foreach (var (id, species) in Species)
            {
                var candidates = population.Values.Select(g => (dist: GetDistance(species.Representative, g), g));
                var newRepr = candidates.OrderBy(x => x.dist).First().g;
                species.Representative = newRepr;
                species.Members = new Dictionary<int, Genome>() { { newRepr.Id, newRepr } };
                population.Remove(newRepr.Id);
            }

            // Distribute the rest to species
            if (Species.Count == 0)
            {
                var initialRepr = population.First().Value;
                population.Remove(initialRepr.Id);
                var id = ++LastSpeciesId;
                var initialSpecies = new Species(id, generation);
                initialSpecies.SetRepresentative(initialRepr);
                Species.Add(id, initialSpecies);
            }

            foreach (var (key, genome) in population)
            {
                var candidates = Species.Values.Select(s => (dist: GetDistance(s.Representative, genome), s));
                var bestFit = candidates.OrderBy(x => x.dist).First();
                if (bestFit.dist < config.CompatibilityThreshold)
                {
                    bestFit.s.Members.Add(genome.Id, genome);
                }
                else
                {
                    var id = ++LastSpeciesId;
                    var species = new Species(id, generation);
                    species.SetRepresentative(genome);
                    Species.Add(id, species);
                }
            }
        }

        public float GetDistance(Genome g1, Genome g2)
        {
            float weightDifference = 0f;
            int disjoint_nodes = g2.ConnectionGenes.Keys.Count(k => !g1.ConnectionGenes.ContainsKey(k));

            foreach (var (key, cg) in g1.ConnectionGenes)
            {
                if (g2.ConnectionGenes.ContainsKey(key))
                {
                    var cg2 = g2.ConnectionGenes[key];
                    weightDifference += Math.Abs(cg.Weight - cg2.Weight);
                    if (cg.Status != cg2.Status)
                    {
                        weightDifference++;
                    }
                }
                else
                {
                    disjoint_nodes++;
                }
            }

            int N = Math.Max(g1.ConnectionGenes.Count, g2.ConnectionGenes.Count);

            return (config.CompatibilityDisjointCoefficient * disjoint_nodes + config.CompatibilityWeightCoefficient * weightDifference) / N;
        }
    }
}
