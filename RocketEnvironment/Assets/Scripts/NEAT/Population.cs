using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT
{
    [Serializable]
    public class PopulationConfig
    {
        public int PopulationSize = 150;
        public SpeciesConfig speciesConfig = new SpeciesConfig();
        public ReproductionConfig reproductionConfig = new ReproductionConfig();
        public StagnationConfig stagnationConfig = new StagnationConfig();
        public GenomeConfig genomeConfig = new GenomeConfig();
    }

    public class Population
    {
        private PopulationConfig config;
        private SpeciesModule species;
        private ReproductionModule reproduction;
        private GenomeModule genomeModule;
        public Genome Best { private set; get; }
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

        public void StoreBest()
        {
            var generationBest = GetAllGenomes().OrderByDescending(g => g.Fitness).First();

            if (Best == null)
            {
                Best = generationBest;
                return;
            }

            if (generationBest.Fitness > Best.Fitness)
            {
                Best = generationBest;
            }
        }

        public void NextGeneration()
        {
            Generation++;
            var population = reproduction.Reproduce(genomeModule, species.Species, config.PopulationSize, Generation, Best.Id);
            if (!population.ContainsKey(Best.Id))
            {
                population.Add(Best.Id, Best);
            }

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
                Best = Best,
                Generation = Generation,
                Species = species.Species
            };
        }
    }
}
