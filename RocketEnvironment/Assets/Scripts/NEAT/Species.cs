using System;
using System.Collections.Generic;
using System.Linq;
using Probability;

namespace NEAT
{
    [Serializable]
    public class Species
    {
        public delegate int GenomeIdGetter();

        public List<Genome> Genomes;
        public Genome Representative { private set; get; }
        public int Id { get { return Representative.Id; } }

        private GenomeIdGetter getGenomeId;

        public Species(Genome repr, GenomeIdGetter idGetter)
        {
            Representative = repr;
            Genomes = new List<Genome>() { repr };
            getGenomeId = idGetter;
        }

        public List<Genome> ProduceOffsprings(int quantity, float survivalThreshold)
        {
            if (quantity == 0)
            {
                return new List<Genome>();
            }

            var ancestorsPool = pickAncestors(survivalThreshold);
            if (ancestorsPool.Size == 1)
            {
                return Enumerable.Range(0, quantity).Select(_ => ancestorsPool.Get().Clone(getGenomeId())).ToList();
            }

            return Enumerable.Range(0, quantity).Select(_ =>
            {
                var p1 = ancestorsPool.Get();
                var p2 = ancestorsPool.Get();
                while (p1 == p2)
                {
                    p2 = ancestorsPool.Get();
                }
                return Cross(p1, p2);
            }).ToList();
        }

        private RandomPool<Genome> pickAncestors(float survivalThreshold)
        {
            Genomes.Sort((a, b) => a.Fitness.CompareTo(b.Fitness));
            var killCount = (int)(Genomes.Count * (1 - survivalThreshold));

            return new RandomPool<Genome>(Genomes.Skip(killCount).Select(g => (g, g.Fitness)));
        }

        private Genome Cross(Genome g1, Genome g2)
        {
            var better = g1.Fitness >= g2.Fitness ? g1 : g2;
            var worse = g1.Fitness < g2.Fitness ? g1 : g2;

            var offspring = new Genome()
            {
                Id = getGenomeId(),
                NodeGenes = new List<NodeGene>(),
                ConnectionGenes = new List<ConnectionGene>()
            };

            var nodesAdded = new HashSet<int>();
            int b = 0;
            int w = 0;
            Action<ConnectionGene> passConnection = (gene) =>
            {
                offspring.ConnectionGenes.Add(gene.Clone());

                if (!nodesAdded.Contains(gene.Connection.Input))
                {
                    offspring.NodeGenes.Add(better.GetNode(gene.Connection.Input).Clone());
                    nodesAdded.Add(gene.Connection.Input);
                }
                if (!nodesAdded.Contains(gene.Connection.Output))
                {
                    offspring.NodeGenes.Add(better.GetNode(gene.Connection.Output).Clone());
                    nodesAdded.Add(gene.Connection.Output);
                }
            };

            while (b < better.ConnectionGenes.Count && w < worse.ConnectionGenes.Count)
            {
                if (better.ConnectionGenes[b].Innovation < worse.ConnectionGenes[w].Innovation)
                {
                    // Pass unmatched genes of better parent
                    var gene = better.ConnectionGenes[b++];
                    passConnection(gene);
                }
                else if (better.ConnectionGenes[b].Innovation > worse.ConnectionGenes[w].Innovation)
                {
                    // Skip unmatched genes of worse parent
                    w++;
                }
                else
                {
                    // Pass random matching genes
                    var pool = new RandomPool<ConnectionGene>(new (ConnectionGene, float)[]{
                        (better.ConnectionGenes[b], better.Fitness),
                        (worse.ConnectionGenes[w], worse.Fitness)
                    });
                    var gene = pool.Get();
                    passConnection(gene);
                    w++;
                    b++;
                }
            }

            // Finish passing unmatched genes of better parent
            while (b < better.ConnectionGenes.Count)
            {
                passConnection(better.ConnectionGenes[b++]);
            }

            offspring.NodeGenes.Sort((a, b) => a.Id - b.Id);

            return offspring;
        }
    }
}