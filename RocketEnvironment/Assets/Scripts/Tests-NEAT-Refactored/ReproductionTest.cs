using System.Linq;
using NEAT2;
using NUnit.Framework;

public class ReproductionTest
{
    [Test]
    public void InitialPopulation()
    {
        var stagnation = new StagnationModule(new StagnationConfig());
        var repr = new ReproductionModule(new ReproductionConfig(), stagnation);
        var genomeModule = new GenomeModule(new GenomeConfig());

        var pop = repr.GetInitialPopulation(genomeModule, 20);

        Assert.That(pop.Count, Is.EqualTo(20));
        Assert.That(pop.Last().Value.Id, Is.EqualTo(20));
    }
}
