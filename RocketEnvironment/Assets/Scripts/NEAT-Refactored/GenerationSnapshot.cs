using System;
using System.Collections.Generic;

namespace NEAT2
{
    [Serializable]
    public class GenerationSnapshot
    {
        public int Generation;
        public Dictionary<int, Species> Species;
        public PopulationConfig config;
    }
}