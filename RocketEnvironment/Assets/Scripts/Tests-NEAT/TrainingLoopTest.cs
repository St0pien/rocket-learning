using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NEAT;
using Newtonsoft.Json;
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
        foreach (var g in population.GetAllGenomes())
        {
            g.Fitness = UnityEngine.Random.Range(0, 100);
        }

        File.WriteAllText("../data/bigrandom_0.json", JsonConvert.SerializeObject(population.Snapshot()));

        for (int i = 1; i <= 500; i++)
        {
            population.NextGeneration();

            foreach (var g in population.GetAllGenomes())
            {
                g.Fitness += UnityEngine.Random.Range(0, 100);
            }

            File.WriteAllText($"../data/bigrandom_{i}.json", JsonConvert.SerializeObject(population.Snapshot()));
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

        var err = xorMap.Select(v =>
        {
            var (input1, input2, output) = v;
            var inDict = new Dictionary<int, float>() {
                {1, input1},
                {2, input2},
                {3, 1}
            };

            var prediction = net.Activate(inDict).First().Value;
            var err = output - prediction;

            return System.Math.Abs(err);
        }).Sum();

        return (4 - err) * (4 - err);
    }

    [Test]
    public void XORTest()
    {
        var population = new Population(new PopulationConfig()
        {
            PopulationSize = 300,
            genomeConfig = new GenomeConfig()
            {
                Inputs = 3,
                Outpus = 1,
                FullyConnected = true,
                InitialRandomWeights = true,

                MinWeight = -30f,
                MaxWeight = 30f,
                TweakWeightProb = 0.9f,
                ReplaceWeightProb = 0.1f,
                TweakMultiplier = 2f,
                ConnDeleteProb = 0.2f,
                ConnAddProb = 0.3f,
                NodeAddProb = 0.05f,
                NodeDeleteProb = 0.05f
            },
            speciesConfig = new SpeciesConfig()
            {
                CompatibilityDisjointCoefficient = 1f,
                CompatibilityWeightCoefficient = 0.5f,
                CompatibilityThreshold = 4f,
            },
            stagnationConfig = new StagnationConfig()
            {
                MaxStagnation = 2,
                SpeciesElitism = 1
            },
            reproductionConfig = new ReproductionConfig()
            {
                Elitism = 0,
                SurvivalThreshold = 0.2f
            }
        });

        population.Init();
        foreach (var g in population.GetAllGenomes())
        {
            g.Fitness = XORFitness(g);
        }
        File.WriteAllText("../data/xor_0.json", JsonConvert.SerializeObject(population.Snapshot()));

        for (int i = 0; i < 100; i++)
        {
            population.NextGeneration();

            foreach (var g in population.GetAllGenomes())
            {
                g.Fitness = XORFitness(g);
            }
            population.StoreBest();
            File.WriteAllText($"../data/xor_{i}.json", JsonConvert.SerializeObject(population.Snapshot()));

            var best = population.Best;
            UnityEngine.Debug.Log($"Best fitness: {best.Fitness}");
            var bestNet = new NeuralNetwork(best.NodeGenes, best.ConnectionGenes);
            UnityEngine.Debug.Log($"Generation {i}");
            UnityEngine.Debug.Log($"0 x 0  = {bestNet.Activate(new Dictionary<int, float>() { { 1, 0f }, { 2, 0f }, { 3, 1f } }).First().Value}");
            UnityEngine.Debug.Log($"0 x 1  = {bestNet.Activate(new Dictionary<int, float>() { { 1, 0f }, { 2, 1f }, { 3, 1f } }).First().Value}");
            UnityEngine.Debug.Log($"1 x 0  = {bestNet.Activate(new Dictionary<int, float>() { { 1, 1f }, { 2, 0f }, { 3, 1f } }).First().Value}");
            UnityEngine.Debug.Log($"1 x 1  = {bestNet.Activate(new Dictionary<int, float>() { { 1, 1f }, { 2, 1f }, { 3, 1f } }).First().Value}");
        }
    }

    public float f(float x)
    {
        return (x - 0.25f) * (x - 0.25f);
    }
    public float RationalFitness(Genome genome)
    {
        var net = new NeuralNetwork(genome.NodeGenes, genome.ConnectionGenes);

        var err = 0f;
        var count = 0;
        for (float i = -1; i < 1; i += 0.01f)
        {
            var pred = net.Activate(new Dictionary<int, float>() {
                    {1, 1},
                    {2, i}
                }).First().Value;
            var e = f(i) - pred;
            err += e * e;
            count++;
        }

        return -(float)Math.Sqrt(err / count);
    }

    [Test]
    public void RationalTest()
    {
        var population = new Population(new PopulationConfig()
        {

            PopulationSize = 100,
            genomeConfig = new GenomeConfig()
            {
                Inputs = 2,
                Outpus = 1,
                FullyConnected = true,
                InitialRandomWeights = true,

                MinWeight = -30f,
                MaxWeight = 30f,
                TweakWeightProb = 0.9f,
                ReplaceWeightProb = 0.1f,
                TweakMultiplier = 2f,
                ConnDeleteProb = 0.2f,
                ConnAddProb = 0.3f,
                NodeAddProb = 0.05f,
                NodeDeleteProb = 0.05f
            },
            speciesConfig = new SpeciesConfig()
            {
                CompatibilityDisjointCoefficient = 1f,
                CompatibilityWeightCoefficient = 0.5f,
                CompatibilityThreshold = 4f,
            },
            stagnationConfig = new StagnationConfig()
            {
                MaxStagnation = 2,
                SpeciesElitism = 1
            },
            reproductionConfig = new ReproductionConfig()
            {
                Elitism = 0,
                SurvivalThreshold = 0.2f
            }

        });

        population.Init();
        foreach (var g in population.GetAllGenomes())
        {
            g.Fitness = RationalFitness(g);
        }
        File.WriteAllText("../data/rational_0.json", JsonConvert.SerializeObject(population.Snapshot()));

        for (int i = 0; i < 100; i++)
        {
            population.NextGeneration();
            foreach (var g in population.GetAllGenomes())
            {
                g.Fitness = RationalFitness(g);
            }
            population.StoreBest();
            File.WriteAllText($"../data/rational_{i}.json", JsonConvert.SerializeObject(population.Snapshot()));
        }


        var net = new NeuralNetwork(population.Best.NodeGenes, population.Best.ConnectionGenes);
        var xs = new List<float>();
        var ys = new List<float>();
        for (float i = -1; i < 1; i += 0.01f)
        {
            var pred = net.Activate(new Dictionary<int, float>() {
                    {1, 1},
                    {2, i}
                }).First().Value;
            xs.Add(i);
            ys.Add(pred);
        }


        File.WriteAllText("../data/plots.json", JsonConvert.SerializeObject(new Dictionary<string, List<float>>()
        {
            {"x", xs},
            {"y", ys}
         }));
    }
}
