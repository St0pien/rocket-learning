using System;
using System.Collections.Generic;
using System.Linq;

namespace NEAT2
{
    [Serializable]
    public class GenomeConfig
    {
        public int Inputs = 2;
        public int Outpus = 1;
        public float MinWeight = -1f;
        public float MaxWeight = 1f;
        public bool FullyConnected = true;
        public bool InitialRandomWeights = true;

        public float NodeAddProb = 0.2f;
        public float NodeDeleteProb = 0.1f;
        public float ConnAddProb = 0.3f;
        public float ConnDeleteProb = 0.1f;
        public float TweakWeightProb = 0.8f;
        public float TweakMultiplier = 0.1f;
        public float ReplaceWeightProb = 0.5f;
    }

    [Serializable]
    public class GenomeModule
    {
        public GenomeConfig config;

        private int LastNodeId = 0;
        private int LastConnectionId = 0;
        private Dictionary<Connection, int> introducedConnections = new Dictionary<Connection, int>();
        private Dictionary<Connection, int> introducedNodes = new Dictionary<Connection, int>();

        public GenomeModule(GenomeConfig cfg)
        {
            config = cfg;
            LastNodeId = config.Inputs + config.Outpus;
            LastConnectionId = config.FullyConnected ? config.Inputs * config.Outpus : 0;
        }

        public int GetNextNodeId(Connection conn)
        {
            if (introducedNodes.ContainsKey(conn))
            {
                return introducedNodes[conn];
            }

            introducedNodes[conn] = ++LastNodeId;
            return introducedNodes[conn];
        }

        public int GetNextConnectionId(Connection conn)
        {
            if (introducedConnections.ContainsKey(conn))
            {
                return introducedConnections[conn];
            }

            introducedConnections[conn] = ++LastConnectionId;
            return introducedConnections[conn];
        }

        public void ResetIndexers()
        {
            introducedNodes.Clear();
            introducedConnections.Clear();
        }
    }

    [Serializable]
    public class Genome
    {
        public int Id;
        public Dictionary<int, NodeGene> NodeGenes = new Dictionary<int, NodeGene>();
        public Dictionary<int, ConnectionGene> ConnectionGenes = new Dictionary<int, ConnectionGene>();
        public float Fitness = float.MinValue;

        private GenomeModule module;

        public Genome(int id, GenomeModule module)
        {
            Id = id;
            this.module = module;
        }

        public void Init()
        {
            var inputs = Enumerable.Range(1, module.config.Inputs)
                .Select(i => new NodeGene(i, NodeGeneType.Sensor));
            var outputs = Enumerable.Range(module.config.Inputs + 1, module.config.Outpus)
                .Select(i => new NodeGene(i, NodeGeneType.Output));
            NodeGenes = inputs.Concat(outputs).ToDictionary(n => n.Id);

            if (module.config.FullyConnected)
            {
                int n = 0;
                ConnectionGenes = outputs
                    .SelectMany(o => inputs.Select(i =>
                    {
                        var id = ++n;
                        var cg = new ConnectionGene(id, new Connection(i.Id, o.Id));
                        if (module.config.InitialRandomWeights)
                        {
                            cg.Weight = UnityEngine.Random.Range(module.config.MinWeight, module.config.MaxWeight);
                        }
                        return cg;
                    })).ToDictionary(c => c.Id);
            }
            else
            {
                ConnectionGenes = new Dictionary<int, ConnectionGene>();
            }
        }

        public void Mutate()
        {

            if (RandomSwitch(module.config.NodeAddProb)) MutateNodeAdd();
            if (RandomSwitch(module.config.NodeDeleteProb)) MutateNodeDelete();
            if (RandomSwitch(module.config.ConnAddProb)) MutateConnAdd();
            if (RandomSwitch(module.config.ConnDeleteProb)) MutateConnDelete();
            if (RandomSwitch(module.config.ConnDeleteProb)) MutateConnDelete();

            foreach (var cg in ConnectionGenes.Values)
            {
                if (RandomSwitch(module.config.TweakWeightProb)) MutateTweakWeight(cg.Id);
                if (RandomSwitch(module.config.ReplaceWeightProb)) MutateReplaceWeight(cg.Id);
            }
        }

        private bool RandomSwitch(float critical)
        {
            return UnityEngine.Random.Range(0f, 1f) < critical;
        }

        private void MutateNodeAdd()
        {
            if (ConnectionGenes.Count == 0) return;

            var cg = ConnectionGenes.Values.ToList()[UnityEngine.Random.Range(0, ConnectionGenes.Count)];
            var newNode = new NodeGene(module.GetNextNodeId(cg.Connection), NodeGeneType.Hidden);
            var conn1 = new Connection(cg.Connection.Input, newNode.Id);
            var cg1 = new ConnectionGene(module.GetNextConnectionId(conn1), conn1) { Weight = 1f };
            var conn2 = new Connection(newNode.Id, cg.Connection.Output);
            var cg2 = new ConnectionGene(module.GetNextConnectionId(conn2), conn2) { Weight = cg.Weight };
            cg.Status = ConnectionStatus.Disabled;
            NodeGenes.Add(newNode.Id, newNode);
            ConnectionGenes.Add(cg1.Id, cg1);
            ConnectionGenes.Add(cg2.Id, cg2);
        }

        private void MutateNodeDelete()
        {
            var available = NodeGenes.Values.Where(ng => ng.Type == NodeGeneType.Hidden).ToList();
            if (available.Count == 0) return;

            var ng = available[UnityEngine.Random.Range(0, available.Count)];
            NodeGenes.Remove(ng.Id);
            foreach (var key in ConnectionGenes.Keys.ToList())
            {
                var c = ConnectionGenes[key].Connection;
                if (c.Input == ng.Id || c.Output == ng.Id)
                {
                    ConnectionGenes.Remove(key);
                }
            }
        }

        private void MutateConnAdd()
        {
            var inputs = NodeGenes.Values.Where(ng => ng.Type != NodeGeneType.Output).ToList();
            var outputs = NodeGenes.Values.Where(ng => ng.Type != NodeGeneType.Sensor).ToList();
            var input = inputs[UnityEngine.Random.Range(0, inputs.Count)];
            var output = outputs[UnityEngine.Random.Range(0, outputs.Count)];

            var existingCg = ConnectionGenes.Values
                .FirstOrDefault(cg => cg.Connection.Input == input.Id && cg.Connection.Output == output.Id);
            if (existingCg != null)
            {
                existingCg.Status = ConnectionStatus.Enabled;
                return;
            }

            var c = new Connection(input.Id, output.Id);
            // Prevent cycles
            if (PathExists(c))
            {
                return;
            }

            var cg = new ConnectionGene(module.GetNextConnectionId(c), c);
            ConnectionGenes.Add(cg.Id, cg);
        }

        private bool PathExists(Connection conn)
        {
            var visited = new HashSet<int>();
            var stack = new Stack<int>();
            stack.Push(conn.Output);
            while (stack.Count > 0)
            {
                int v = stack.Pop();
                if (v == conn.Input)
                {
                    return true;
                }
                visited.Add(v);
                var neighbors = ConnectionGenes.Values
                    .Where(c => c.Connection.Input == v && !visited.Contains(c.Connection.Output))
                    .Select(c => c.Connection.Output);
                foreach (var n in neighbors)
                {
                    stack.Push(n);
                }
            }

            return false;
        }

        private void MutateConnDelete()
        {
            var cgs = ConnectionGenes.Values.ToList();
            var cg = cgs[UnityEngine.Random.Range(0, cgs.Count)];
            cg.Status = ConnectionStatus.Disabled;
        }

        private void MutateTweakWeight(int id)
        {
            ConnectionGenes[id].Weight += UnityEngine.Random.Range(-1f, 1f) * module.config.TweakMultiplier;
            ConnectionGenes[id].Weight = Math.Clamp(ConnectionGenes[id].Weight, module.config.MinWeight, module.config.MaxWeight);
        }

        private void MutateReplaceWeight(int id)
        {
            ConnectionGenes[id].Weight = UnityEngine.Random.Range(module.config.MinWeight, module.config.MaxWeight);
        }
    }
}