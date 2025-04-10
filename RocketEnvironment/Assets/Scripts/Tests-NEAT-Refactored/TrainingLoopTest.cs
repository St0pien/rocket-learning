using System.IO;
using NEAT2;
using NUnit.Framework;
using UnityEngine;

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
        foreach (var g in population.GetAllGenomes())
        {
            g.Fitness += UnityEngine.Random.Range(0, 100);
        }

        for (int i = 1; i <= 500; i++)
        {
            population.NextGeneration();

            foreach (var g in population.GetAllGenomes())
            {
                g.Fitness += UnityEngine.Random.Range(0, 100);
            }
        }
    }
}
