using System.Collections.Generic;
using NEAT;
using NUnit.Framework;

public class TestNeuralNetwork
{
    [Test]
    public void SimpleTest()
    {
        NodeGene input = new NodeGene(1, NodeType.Sensor);
        NodeGene output = new NodeGene(2, NodeType.Output);
        
        ConnectionGene conn = new ConnectionGene(input.Id, output.Id);
        conn.Weight = 1f;
        
        NeuralNetwork network = new NeuralNetwork(new List<NodeGene>() {input, output}, new List<ConnectionGene>() {conn});
        
        var result = network.CalculateValues(new Dictionary<int, float>() { {1, 1}});
        Assert.That(result[2], Is.EqualTo(1)); 
    }

    [Test]
    public void SimpleTest2Outputs()
    {
        NodeGene input = new NodeGene(1, NodeType.Sensor);
        NodeGene output = new NodeGene(2, NodeType.Output);
        NodeGene output2 = new NodeGene(3, NodeType.Output);
        
        ConnectionGene conn = new ConnectionGene(input.Id, output.Id);
        ConnectionGene conn2 = new ConnectionGene(input.Id, output2.Id);
        conn.Weight = 1f;
        conn2.Weight = 2f;
        
        NeuralNetwork network = new NeuralNetwork(new List<NodeGene>() {input, output, output2}, new List<ConnectionGene>() {conn, conn2});
        
        var result = network.CalculateValues(new Dictionary<int, float>() { {1, 1}});
        Assert.That(result[2], Is.EqualTo(1)); 
        Assert.That(result[3], Is.EqualTo(2)); 
    }

    [Test]
    public void SimpleTest2Inputs()
    {
        NodeGene input = new NodeGene(1, NodeType.Sensor);
        NodeGene input2 = new NodeGene(2, NodeType.Sensor);
        NodeGene output = new NodeGene(3, NodeType.Output);
        
        ConnectionGene conn = new ConnectionGene(input.Id, output.Id);
        ConnectionGene conn2 = new ConnectionGene(input2.Id, output.Id);
        conn.Weight = 1f;
        conn2.Weight = 6f;
        
        NeuralNetwork network = new NeuralNetwork(new List<NodeGene>() {input, input2, output}, new List<ConnectionGene>() {conn, conn2});
        
        var result = network.CalculateValues(new Dictionary<int, float>() { {1, 1}, {2, 2}});
        Assert.That(result[3], Is.EqualTo(13)); 
        
    }
    [Test]
    public void SimpleTestHiddenLayer()
    {
        NodeGene input = new NodeGene(1, NodeType.Sensor);
        NodeGene hidden = new NodeGene(2, NodeType.Hidden);
        NodeGene output = new NodeGene(3, NodeType.Output);
        
        ConnectionGene conn = new ConnectionGene(input.Id, hidden.Id);
        ConnectionGene conn2 = new ConnectionGene(hidden.Id, output.Id);
        conn.Weight = 3f;
        conn2.Weight = 6f;
        
        NeuralNetwork network = new NeuralNetwork(new List<NodeGene>() {input, hidden, output}, new List<ConnectionGene>() {conn, conn2});
        
        var result = network.CalculateValues(new Dictionary<int, float>() { {1, 1} });
        Assert.That(result[3], Is.EqualTo(18)); 
    }

    [Test]
    public void ReadJSONTest()
    {
        var parser = new PopulationJSONParser(".\\Assets\\Scripts\\NeuralNetwork\\NeuralNetworkTests\\test_json.json");
        var network = parser.GetNeuralNetwork(1);

        var result = network.CalculateValues(new Dictionary<int, float>() { {1, 1}, {2, 2}, {3, 3} });
        Assert.That(result[4], Is.EqualTo(0));
        Assert.That(result[5], Is.EqualTo(0));
        Assert.That(result[6], Is.EqualTo(0));
        Assert.That(result[7], Is.EqualTo(0));
    }
}
