using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT2
{
    [Serializable]
    public class StagnationConfig
    {
        public int MaxStagnation = 15;
        public int SpeciesElitism = 0;
    }


    public class StagnationModule
    {
        private StagnationConfig config;

        public StagnationModule(StagnationConfig cfg)
        {
            config = cfg;
        }

        public Dictionary<int, bool> MarkStagnant(Dictionary<int, Species> speciesSet, int generation)
        {
            // Calculate species fitness
            foreach (var (sid, s) in speciesSet)
            {
                var prevFitness = s.FitnessHistory.Count > 0 ? s.FitnessHistory.Max() : float.MinValue;
                s.Fitness = s.Members.Values.Average(m => m.Fitness);
                s.FitnessHistory.Add(s.Fitness);
                s.AdjustedFitness = float.MinValue;

                if (prevFitness < s.Fitness)
                {
                    s.LastImproved = generation;
                }
            }

            // Mark stagnant species
            var result = new Dictionary<int, bool>();
            var speciesByFitness = speciesSet.Values.OrderBy(s => s.Fitness).ToList();
            var nonStagnant = speciesByFitness.Count;
            for (int i = 0; i < speciesByFitness.Count; i++)
            {
                var s = speciesByFitness[i];
                var stagnantDuration = generation - s.LastImproved;
                var isStagnant = false;
                if (nonStagnant > config.SpeciesElitism)
                {
                    isStagnant = stagnantDuration >= config.MaxStagnation;
                }

                if (speciesByFitness.Count - i <= config.SpeciesElitism)
                {
                    isStagnant = false;
                }

                if (isStagnant)
                {
                    nonStagnant--;
                }
                result.Add(s.Id, isStagnant);
            }

            return result;
        }
    }
}

