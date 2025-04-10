using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT2
{
    public class Activations
    {
        public static float Sigmoid(float x)
        {
            return 1 / (1 + MathF.Exp(-4.9f * x));
        }
    }

    public class NeuralNetwork
    {
        public Dictionary<int, NodeGene> Nodes;
        public Dictionary<int, List<ConnectionGene>> Dependencies;
        public List<ConnectionGene> Connections;
        public List<NodeGene> OutputNodes;


        public NeuralNetwork(Dictionary<int, NodeGene> nodes, Dictionary<int, ConnectionGene> connections)
        {
            Nodes = nodes;
            OutputNodes = nodes.Values.Where(n => n.Type == NodeGeneType.Output).ToList();
            Connections = connections.Values.Where(c => c.Status != ConnectionStatus.Disabled).ToList();
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

        public Dictionary<int, float> Activate(Dictionary<int, float> inputs)
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
                    if (Nodes[id].Type == NodeGeneType.Sensor)
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

                result = Activations.Sigmoid(result);
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
}