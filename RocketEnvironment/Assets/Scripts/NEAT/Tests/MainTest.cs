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

        File.WriteAllText("file.json", JsonUtility.ToJson(x));
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

        mutator.Mutate(x.genomes[0]);

        Debug.Log(JsonUtility.ToJson(x.genomes[0]));
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
        mutator.Mutate(x.genomes[0]);

        File.WriteAllText("file.json", JsonUtility.ToJson(x));
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

        foreach (var g in x.genomes)
        {
            mutator.Mutate(g);
        }

        File.WriteAllText("file.json", JsonUtility.ToJson(x));
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
    public void FullMutationTest()
    {
        var x = new Population(new PopulationConfig
        {
            PopulationSize = 5,
            InputSize = 2,
            OutputSize = 1,
            GeneralMutationChance = 0.9f,
            TweakWeightMutationProb = 1,
            NewConnectionMutationProb = 5,
            NewNodeMutationProb = 5,
            FullyConnected = true,
        });
        Random.InitState(0);

        x.Mutate();
        File.WriteAllText("../data/uno.json", JsonUtility.ToJson(x));
        Debug.Log("uno");
        x.Mutate();
        File.WriteAllText("../data/dos.json", JsonUtility.ToJson(x));
        Debug.Log("dos");
        x.Mutate();
        File.WriteAllText("../data/tres.json", JsonUtility.ToJson(x));
        Debug.Log("tres");
        x.Mutate();
        File.WriteAllText("../data/quatro.json", JsonUtility.ToJson(x));
        Debug.Log("quatro");
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
        x.Mutate();
        foreach (var g in x.genomes)
        {
            g.Fitness = Random.Range(0, 10f);
        }

        x.genomes.Sort((a, b) => (int)(a.Fitness - b.Fitness));
        File.WriteAllText("../data/fit1.json", JsonUtility.ToJson(x));


        x.NextGeneration();
        File.WriteAllText("../data/cross1.json", JsonUtility.ToJson(x));

        Assert.That(x.genomes.Count, Is.EqualTo(5));
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
            x.Mutate();
            foreach (var g in x.genomes)
            {
                g.Fitness = Random.Range(0, 10f);
            }

            x.genomes.Sort((a, b) => (int)(a.Fitness - b.Fitness));
            File.WriteAllText($"../data/fit{i}.json", JsonUtility.ToJson(x));

            x.NextGeneration();
            File.WriteAllText($"../data/cross{i}.json", JsonUtility.ToJson(x));
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
                {2, input2}
            };

            var prediction = net.CalculateValues(inDict).First().Value;
            var err = output - prediction;

            return err * err;
        }).Sum();

        return -squaredError;
    }

    [Test]
    public void XORTrainingLoop()
    {
        Random.InitState(0);
        var population = new Population(new PopulationConfig()
        {
            PopulationSize = 40,
            InputSize = 2,
            OutputSize = 1,
            GeneralMutationChance = 0.8f,
            TweakWeightMutationProb = 10f,
            NewConnectionMutationProb = 2f,
            NewNodeMutationProb = 2f,
            SurvivalThreshold = .8f,
            Elitism = 3,
            FullyConnected = true
        });
        population.Mutate();
        foreach (var g in population.genomes)
        {
            g.Fitness = XORFitness(g);
        }
        population.genomes.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));
        File.WriteAllText($"../data/xor_{0}.json", JsonUtility.ToJson(population));

        for (int i = 1; i <= 1000; i++)
        {
            population.NextGeneration();
            population.Mutate();
            foreach (var g in population.genomes)
            {
                g.Fitness = XORFitness(g);
            }
            population.genomes.Sort((a, b) => b.Fitness.CompareTo(a.Fitness));

            File.WriteAllText($"../data/xor_{i}.json", JsonUtility.ToJson(population));
        }
    }
}
