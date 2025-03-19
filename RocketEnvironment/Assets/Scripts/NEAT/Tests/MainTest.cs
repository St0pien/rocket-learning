using NUnit.Framework;
using NEAT;
using UnityEngine;
using System.IO;

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
        Debug.Log(MutationType.TweakWeight == 0);
        Debug.Log(MutationType.AddConnection != 0);
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
}
