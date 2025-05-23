using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using NEAT;
using System.Collections.Generic;
using System.Linq;

public class GenomeInput : MonoBehaviour
{
    public Rocket rocket;
    public string JsonPath;

    private NeuralNetwork network;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var content = File.ReadAllText(JsonPath);
        var snapshot = JsonConvert.DeserializeObject<GenerationSnapshot>(content);
        var bestGenome = snapshot.Best;
        network = new NeuralNetwork(bestGenome.NodeGenes, bestGenome.ConnectionGenes);
        Debug.Log($"GenomeInput: Loaded Genome {bestGenome.Id} from {JsonPath}");

        rb = rocket.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        var outputs = network.Activate(new Dictionary<int, float>(){
            {1, 1},
            {2, rocket.transform.position.y},
            {3, rb.linearVelocity.y}
        });

        var thrust = outputs.First().Value;
        foreach (var engine in rocket.engines)
        {
            engine.SetThrust(thrust);
        }
    }
}
