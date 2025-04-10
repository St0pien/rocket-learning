using System.Linq;
using NEAT2;
using NUnit.Framework;

public class GenomeTest
{
    [Test]
    public void GenomeInit()
    {
        var config = new GenomeConfig()
        {
            Inputs = 4,
            Outpus = 10,
            FullyConnected = true
        };

        var module = new GenomeModule(config);

        var genome = new Genome(1, module);
        genome.Init();

        Assert.That(genome.NodeGenes.Count, Is.EqualTo(14));
        Assert.That(genome.ConnectionGenes.Count, Is.EqualTo(40));
        Assert.That(genome. ConnectionGenes.Last().Value.Id, Is.EqualTo(40));
    }
}
