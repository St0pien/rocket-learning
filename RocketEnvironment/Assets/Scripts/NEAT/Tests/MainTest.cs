using NUnit.Framework;
using NEAT;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class MainTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void MainTestSimplePasses()
    {
        var x = new Population(new PopulationConfig()
        {
            PopulationSize = 10,
            InputSize = 2,
            OutputSize = 1,
            FullyConnected = true
        });

        File.WriteAllText("../data/file.json", JsonUtility.ToJson(x));
    }

    [Test]
    public void TestWeightMutator()
    {
        var x = new Population(new PopulationConfig()
        {
            PopulationSize = 10,
            InputSize = 2,
            OutputSize = 1,
            FullyConnected = true
        });

        var mutator = new TweakWeightMutator();

        mutator.Mutate(x.Species[0].Genomes[0]);

        Debug.Log(JsonUtility.ToJson(x.Species[0].Genomes[0]));
    }

    [Test]
    public void NewConnectionTest()
    {
        var x = new Population(new PopulationConfig()
        {
            PopulationSize = 5,
            InputSize = 3,
            OutputSize = 4,
        });

        var id = 0;
        var mutator = new NewConnectionMutator((_) => ++id);
        mutator.Mutate(x.Species[0].Genomes[0]);

        File.WriteAllText("../data/file.json", JsonUtility.ToJson(x));
    }

    [Test]
    public void NewNodeTest()
    {
        var x = new Population(new PopulationConfig()
        {
            PopulationSize = 5,
            InputSize = 3,
            OutputSize = 4,
            FullyConnected = true
        });

        var nod = 7;
        var con = 12;
        var mutator = new NewNodeMutator((_) => ++nod, (_) => ++con);

        foreach (var g in x.GetAllGenomes())
        {
            mutator.Mutate(g);
        }

        File.WriteAllText("../data/file.json", JsonUtility.ToJson(x));
    }

    [Test]
    public void MutationConfigTest()
    {
        var config = new MutationConfig()
            .AddMutator(4, new TweakWeightMutator())
            .AddMutator(3, new NewConnectionMutator((_) => 0))
            .AddMutator(1, new TweakWeightMutator()).GetMutationDistribution();

        foreach (var x in config)
        {
            Debug.Log(x.Limit);
        }
    }

    [Test]
    public void CrossoverTesting()
    {
        var x = new Population(new PopulationConfig()
        {
            PopulationSize = 5,
            InputSize = 2,
            OutputSize = 1,
            GeneralMutationChance = 0.9f,
            TweakWeightMutationProb = 1,
            NewConnectionMutationProb = 10,
            NewNodeMutationProb = 5,
            SurvivalThreshold = 0.5f,
            Elitism = 1,
            FullyConnected = true
        });

        Random.InitState(0);
        foreach (var g in x.GetAllGenomes())
        {
            g.Fitness = Random.Range(0, 10f);
        }

        File.WriteAllText("../data/fit1.json", JsonUtility.ToJson(x));

        x.NextGeneration();
        File.WriteAllText("../data/cross1.json", JsonUtility.ToJson(x));

        Assert.That(x.GetAllGenomes().Count, Is.EqualTo(5));
    }

    [Test]
    public void CrossoverMultipleTesting()
    {
        var x = new Population(new PopulationConfig()
        {
            PopulationSize = 30,
            InputSize = 2,
            OutputSize = 1,
            GeneralMutationChance = 0.9f,
            TweakWeightMutationProb = 3,
            NewConnectionMutationProb = 1,
            NewNodeMutationProb = 1,
            SurvivalThreshold = 0.5f,
            Elitism = 2,
            FullyConnected = true
        });

        Random.InitState(0);

        for (int i = 0; i < 20; i++)
        {
            foreach (var g in x.GetAllGenomes())
            {
                g.Fitness = Random.Range(0, 10f);
            }
            File.WriteAllText($"../data/cross{i}.json", JsonUtility.ToJson(x));

            x.NextGeneration();
        }
    }

    private List<(float, float, float)> xorMap = new List<(float, float, float)>(){
        (0f,0f, 0f),
        (0f,1f,1f),
        (1f, 0f, 1f),
        (1f,1f,0f)
    };

    float XORFitness(Genome genome)
    {
        var net = new NeuralNetwork(genome.NodeGenes, genome.ConnectionGenes);

        var squaredError = xorMap.Select(v =>
        {
            var (input1, input2, output) = v;
            var inDict = new Dictionary<int, float>() {
                {1, input1},
                {2, input2},
                {3, 1}
            };

            var prediction = net.CalculateValues(inDict).First().Value;
            var err = output - prediction;

            return System.Math.Abs(err);
        }).Sum();

        return (4 - squaredError) * (4 - squaredError);
    }

    [Test]
    public void XORTrainingLoop()
    {
        // Random.InitState(0);
        var population = new Population(new PopulationConfig()
        {
            PopulationSize = 150,
            InputSize = 3,
            OutputSize = 1,
            FullyConnected = true,
            GeneralMutationChance = 0.7f,
            TweakWeightMutationProb = 1f,
            NewConnectionMutationProb = 1f,
            NewNodeMutationProb = 1f,
            SurvivalThreshold = .4f,
            Elitism = 20,
            CompatiblityThreshold = 1f,
            ExcessCompatiblityFactor = 1,
            DisjointCompatiblityFactor = 1,
            WeightCompatiblityFactor = 0.4f
        });

        for (int i = 1; i <= 100; i++)
        {
            foreach (var g in population.GetAllGenomes())
            {
                g.Fitness = XORFitness(g);
            }
            File.WriteAllText($"../data/xor-medium_{i}.json", JsonUtility.ToJson(population));
            population.NextGeneration();
        }

        foreach (var g in population.GetAllGenomes())
        {
            g.Fitness = XORFitness(g);
        }

        var best = population.GetAllGenomes().OrderBy(g => g.Fitness).First();

        var net = new NeuralNetwork(best.NodeGenes, best.ConnectionGenes);

        Debug.Log($"0 x 0 = {net.CalculateValues(new Dictionary<int, float>() { { 1, 0 }, { 2, 0 }, { 3, 1 } }).First().Value}");
        Debug.Log($"1 x 0 = {net.CalculateValues(new Dictionary<int, float>() { { 1, 1 }, { 2, 0 }, { 3, 1 } }).First().Value}");
    }
}
