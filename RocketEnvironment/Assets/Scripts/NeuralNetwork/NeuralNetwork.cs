using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using File = System.IO.File;

namespace NEAT
{
    public class NeuralNetwork
    {
        public Dictionary<int, NodeGene> Nodes;
        public Dictionary<int, List<ConnectionGene>> Dependencies;

        public List<ConnectionGene> Connections;
        public List<NodeGene> OutputNodes;


        public NeuralNetwork(List<NodeGene> nodes, List<ConnectionGene> connections)
        {
            OutputNodes = new List<NodeGene>();
            Nodes = new Dictionary<int, NodeGene>();
            foreach (var node in nodes)
            {
                Nodes.Add(node.Id, node);
                if (node.Type == NodeType.Output)
                {
                    OutputNodes.Add(node);
                }
            }
            Connections = connections.Where(c => c.Status != ConnectionStatus.Disabled).ToList();
            // construct dependencies list
            Dependencies = new Dictionary<int, List<ConnectionGene>>();

            foreach (var conn in Connections)
            {
                int endId = conn.Connection.Output;
                if (Dependencies.ContainsKey(endId))
                {
                    Dependencies[endId].Add(conn);
                }
                else
                {
                    Dependencies.Add(endId, new List<ConnectionGene>() { conn });
                }
            }

        }

        public Dictionary<int, float> CalculateValues(Dictionary<int, float> inputs)
        {
            var memo = new Dictionary<int, float>();
            float Helper(int id)
            {
                if (memo.ContainsKey(id))
                {
                    return memo[id];
                }
                if (!Dependencies.ContainsKey(id) || Dependencies[id].Count == 0)
                {
                    if (Nodes[id].Type == NodeType.Sensor)
                    {
                        return inputs[id];
                    }
                    // case when there is a hidden layer node not connected to any previous nodes...
                    // or not connected output
                    return 0;
                }
                float result = 0;
                foreach (var dep in Dependencies[id])
                {
                    int startNode = dep.Connection.Input;
                    result += dep.Weight * Helper(startNode);
                }

                memo.Add(id, result);
                return result;
            }

            var result = new Dictionary<int, float>();
            foreach (var output in OutputNodes)
            {
                result[output.Id] = Helper(output.Id);
            }

            return result;
        }
    }

    // read a population json file and provide util to get a particular neural network by given genome id
    public class PopulationJSONParser
    {
        public List<Genome> Genomes;
        public PopulationJSONParser(string filePath)
        {
            string helper = File.ReadAllText(filePath);
            Genomes = JsonUtility.FromJson<Population>(helper).genomes;
        }

        public NeuralNetwork GetNeuralNetwork(int genomeId)
        {
            Genome found = Genomes.Find(x => x.Id == genomeId);

            return new NeuralNetwork(found.NodeGenes, found.ConnectionGenes);
        }
    }

    // read a file with singular Genome JSON representation
    public class GenomeJSONParser
    {
        public Genome genome;

        public GenomeJSONParser(string filePath)
        {
            string helper = File.ReadAllText(filePath);
            genome = JsonUtility.FromJson<Genome>(helper);
        }

        public NeuralNetwork GetNeuralNetwork()
        {
            return new NeuralNetwork(genome.NodeGenes, genome.ConnectionGenes);
        }
    }
}