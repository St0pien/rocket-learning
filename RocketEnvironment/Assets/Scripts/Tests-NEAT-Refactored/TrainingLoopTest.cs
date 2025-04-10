using NEAT2;
using NUnit.Framework;

public class TrainingLoopTest
{
    [Test]
    public void BigRandomTrainginLoop()
    {
        var population = new Population(new PopulationConfig()
        {
            PopulationSize = 200,
            genomeConfig = new GenomeConfig()
            {
                Inputs = 4,
                Outpus = 5,
                InitialRandomWeights = true,
                FullyConnected = true,
            },
            reproductionConfig = new ReproductionConfig()
            {
                Elitism = 1,
                SurvivalThreshold = 0.4f
            },
            speciesConfig = new SpeciesConfig(),
            stagnationConfig = new StagnationConfig()
            {
                SpeciesElitism = 1
            }
        });

        population.Init();

        for (int i = 0; i < 500; i++)
        {
            foreach (var g in population.GetAllGenomes())
            {
                g.Fitness += UnityEngine.Random.Range(0, 100);
            }
            population.NextGeneration();
        }
    }
}
