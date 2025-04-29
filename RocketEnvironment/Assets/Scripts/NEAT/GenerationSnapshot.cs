using System;
using System.Collections.Generic;

namespace NEAT
{
    [Serializable]
    public class GenerationSnapshot
    {
        public int Generation;
        public Genome Best;
        public Dictionary<int, Species> Species;
        public PopulationConfig config;
    }
}