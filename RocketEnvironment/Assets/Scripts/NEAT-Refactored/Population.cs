using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT2
{
    [Serializable]
    public class PopulationConfig
    {
        public int PopulationSize = 150;
        public SpeciesConfig speciesConfig;
        public ReproductionConfig reproductionConfig;
        public StagnationConfig stagnationConfig;
        public GenomeConfig genomeConfig;
    }

    public class Population
    {
        private PopulationConfig config;
        private SpeciesModule species;
        private ReproductionModule reproduction;
        private GenomeModule genomeModule;
        public int Generation = 0;

        public Population(PopulationConfig cfg)
        {
            config = cfg;
            species = new SpeciesModule(cfg.speciesConfig);
            reproduction = new ReproductionModule(cfg.reproductionConfig, new StagnationModule(cfg.stagnationConfig));
            genomeModule = new GenomeModule(cfg.genomeConfig);
        }

        public void Init()
        {
            var population = reproduction.GetInitialPopulation(genomeModule, config.PopulationSize);
            species.Speciate(population, Generation);
        }

        public void NextGeneration()
        {
            Generation++;
            var population = reproduction.Reproduce(genomeModule, species.Species, config.PopulationSize, Generation);

            if (species.Species.Count == 0)
            {
                throw new Exception("Population extinction");
            }

            species.Speciate(population, Generation);
        }

        public IEnumerable<Genome> GetAllGenomes()
        {
            return species.Species.Values.SelectMany(s => s.Members.Values);
        }

        public GenerationSnapshot Snapshot()
        {
            return new GenerationSnapshot()
            {
                config = config,
                Generation = Generation,
                Species = species.Species
            };
        }
    }
}
