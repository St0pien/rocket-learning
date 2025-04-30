using System.Collections.Generic;
using UnityEngine;
using NEAT;
using System.Linq;

public class TrainLoop : MonoBehaviour
{

    private class TrainingEnvironment
    {
        public GameObject Environment;
        public Rocket Rocket;
        public LandingVelocityMetric LandingVelocity;
        public ColliderMetric GroundHit;
        public DistanceMetric[] Distances;
        public OOBMetric Bounds;
        public Genome Genome { private set; get; }
        public NeuralNetwork network;

        public void AssignGenome(Genome genome)
        {
            Genome = genome;
            network = new NeuralNetwork(Genome.NodeGenes, Genome.ConnectionGenes);
            Debug.Log($"Assigned genome {genome.Id} to {Environment.name}");
        }

        public void DropGenome()
        {
            if (Genome == null) return;

            Debug.Log($"Removing genome {Genome.Id} to {Environment.name}. Fitness: {Genome.Fitness}");
            Genome = null;
        }
    }

    [Header("Prefabs")]
    public GameObject rocketPrefab;
    public GameObject groundPrefab;
    public GameObject oobPrefab;

    [Header("Env placing")]
    public int EnvWidth = 2;
    public int EnvHeight = 2;
    public float HorizontalOffset = 150f;
    public float VerticalOffset = 150f;

    [Header("Initial settings")]
    public float RocketHeight = 20f;

    private List<TrainingEnvironment> environments = new List<TrainingEnvironment>();
    private Population population;
    private Queue<Genome> genomesToEvaluate;
    private Queue<TrainingEnvironment> availableEnvironments = new Queue<TrainingEnvironment>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupEnvironments();
        SetupPopulation();
    }

    private void SetupEnvironments()
    {

        for (int r = 0; r < EnvHeight; r++)
        {
            for (int c = 0; c < EnvWidth; c++)
            {
                var pos = new Vector3(r * VerticalOffset, 0, c * HorizontalOffset);
                var env = SpawnEnvironment(r * EnvWidth + c, pos);
                environments.Add(env);
            }
        }

        availableEnvironments = new Queue<TrainingEnvironment>(environments);
        Debug.Log($"Training start: {EnvWidth * EnvHeight} environments initialized");
    }

    private void SetupPopulation()
    {
        population = new Population(new PopulationConfig()
        {
            PopulationSize = 5,
            genomeConfig = new GenomeConfig()
            {
                Inputs = 5,
                Outpus = 4,
                FullyConnected = true,
                InitialRandomWeights = true,

                MinWeight = -30f,
                MaxWeight = 30f,
                TweakWeightProb = 0.8f,
                ReplaceWeightProb = 0.1f,
                TweakMultiplier = 1f,
                ConnDeleteProb = 0.012f,
                ConnAddProb = 0.05f,
                NodeAddProb = 0.02f,
                NodeDeleteProb = 0.009f
            },
            speciesConfig = new SpeciesConfig()
            {
                CompatibilityDisjointCoefficient = 1f,
                CompatibilityWeightCoefficient = 0.4f,
                CompatibilityThreshold = 5f,
            },
            stagnationConfig = new StagnationConfig()
            {
                MaxStagnation = 20,
                SpeciesElitism = 1
            },
            reproductionConfig = new ReproductionConfig()
            {
                Elitism = 2,
                SurvivalThreshold = 0.2f
            }
        });
        population.Init();

        genomesToEvaluate = new Queue<Genome>(population.GetAllGenomes());
        Debug.Log($"Initial population spawned: {genomesToEvaluate.Count} genomes");
    }

    private TrainingEnvironment SpawnEnvironment(int id, Vector3 position)
    {
        var env = new GameObject($"Environment - {id}");
        env.transform.position = position;
        var rocket = SpawnRocket(env.transform);
        var ground = SpawnGround(env.transform);
        var bounds = SpawnBounds(env.transform);
        bounds.Track(rocket);

        var trainingEnv = new TrainingEnvironment()
        {
            Environment = env,
            Rocket = rocket.GetComponent<Rocket>(),
            LandingVelocity = rocket.GetComponent<LandingVelocityMetric>(),
            GroundHit = ground.GetComponent<ColliderMetric>(),
            Distances = rocket.GetComponentsInChildren<DistanceMetric>(),
            Bounds = bounds
        };

        trainingEnv.Bounds.OnOutOfBounds += () =>
        {
            EndSession(trainingEnv);
        };

        return trainingEnv;
    }

    private GameObject SpawnRocket(Transform parent)
    {
        var rocket = Instantiate(rocketPrefab, parent);
        rocket.transform.localPosition = new Vector3(0, RocketHeight, 0);

        return rocket;
    }

    private GameObject SpawnGround(Transform parent)
    {
        var ground = Instantiate(groundPrefab, parent);
        ground.transform.localPosition = new Vector3(0, 0, 0);

        return ground;
    }

    private OOBMetric SpawnBounds(Transform parent)
    {
        var bounds = Instantiate(oobPrefab, parent);
        bounds.transform.localPosition = new Vector3(0, 0, 0);

        return bounds.GetComponent<OOBMetric>();
    }

    // Update is called once per frame
    void Update()
    {
        AssignGenomesToEnvironments();
        SteerRockets();
    }

    private void AssignGenomesToEnvironments()
    {
        while (genomesToEvaluate.Count > 0 && availableEnvironments.Count > 0)
        {
            var genome = genomesToEvaluate.Dequeue();
            var env = availableEnvironments.Dequeue();
            env.AssignGenome(genome);
            env.Rocket.transform.localPosition = new Vector3(0, RocketHeight, 0);
            genome.Fitness = 0;
        }
    }

    private void EndSession(TrainingEnvironment env)
    {
        env.DropGenome();
        availableEnvironments.Enqueue(env);
    }

    private void SteerRockets()
    {
        foreach (var env in environments)
        {
            if (env.Genome == null || env.network == null) continue;

            var outputs = env.network.Activate(new Dictionary<int, float>(){
                {1, 1},
                {2, env.Distances[0].GetValue()},
                {3, env.Distances[1].GetValue()},
                {4, env.Distances[2].GetValue()},
                {5, env.Distances[3].GetValue()},
            });

            int i = 0;
            foreach (var output in outputs.OrderBy(o => o.Key))
            {
                env.Rocket.engines[i].SetThrust(output.Value);
                i++;
            }
        }
    }
}
