using NEAT2;
using NUnit.Framework;

public class SpeciesTest
{
    [Test]
    public void CompatiblityDistance()
    {
        var genomeModule = new GenomeModule(new GenomeConfig()
        {
            Inputs = 4,
            Outpus = 2,
            FullyConnected = true,
            InitialRandomWeights = false
        });
        var speciesModule = new SpeciesModule(new SpeciesConfig());

        var genome1 = new Genome(1, genomeModule);
        var genome2 = new Genome(2, genomeModule);
        genome1.Init();
        genome2.Init();

        Assert.That(speciesModule.GetDistance(genome1, genome2), Is.EqualTo(0));

        genomeModule = new GenomeModule(new GenomeConfig()
        {
            Inputs = 4,
            Outpus = 2,
            FullyConnected = true,
            InitialRandomWeights = true
        });

        genome1 = new Genome(1, genomeModule);
        genome2 = new Genome(2, genomeModule);
        genome1.Init();
        genome2.Init();
        var dist1 = speciesModule.GetDistance(genome1, genome2);
        genome2.ConnectionGenes.Remove(2);
        var dist2 = speciesModule.GetDistance(genome1, genome2);

        Assert.That(dist1, Is.GreaterThan(0));
        Assert.That(dist2, Is.GreaterThan(dist1));
    }
}
