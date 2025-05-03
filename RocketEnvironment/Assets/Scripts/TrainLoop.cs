using System.Collections.Generic;
using UnityEngine;
using NEAT;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

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
        public bool InProgress = false;

        public void AssignGenome(Genome genome)
        {
            Genome = genome;
            network = new NeuralNetwork(Genome.NodeGenes, Genome.ConnectionGenes);
            Debug.Log($"Assigned genome {genome.Id} to {Environment.name}");
        }

        public void DropGenome()
        {
            if (Genome == null) return;

            Debug.Log($"Removing genome {Genome.Id} from {Environment.name}. Fitness: {Genome.Fitness}");
            Genome = null;
        }
    }

    public string Label = "env";

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

    [Header("Fitness")]
    public float InitialFitness = 100f;
    public float OOBPunishment = 100f;
    public float DestructiveHitPunishment = 100f;
    public float LegHitReward = 300f;
    public float LegHitVelocityPunishmentMultiplier = 5f;
    public float HeightRewardPerSecond = 1f;

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
            PopulationSize = 100,
            genomeConfig = new GenomeConfig()
            {
                Inputs = 5,
                Outpus = 4,
                FullyConnected = false,
                InitialRandomWeights = true,

                MinWeight = -30f,
                MaxWeight = 30f,
                TweakWeightProb = 0.8f,
                ReplaceWeightProb = 0.1f,
                TweakMultiplier = 1f,
                ConnDeleteProb = 0.012f,
                ConnAddProb = 0.5f,
                NodeAddProb = 0.2f,
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
                MaxStagnation = 2,
                SpeciesElitism = 0
            },
            reproductionConfig = new ReproductionConfig()
            {
                Elitism = 0,
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
            // Prevent multiple event triggers from the same collision
            if (trainingEnv.InProgress)
            {
                trainingEnv.Genome.Fitness -= OOBPunishment;
                trainingEnv.InProgress = false;
                EndSession(trainingEnv);
            }
        };
        trainingEnv.GroundHit.OnDestructiveHit += () =>
        {
            // Prevent multiple event triggers from the same collision
            if (trainingEnv.InProgress)
            {
                trainingEnv.Genome.Fitness -= DestructiveHitPunishment;
                trainingEnv.InProgress = false;
                EndSession(trainingEnv);
            }
        };
        trainingEnv.GroundHit.OnLegHit += () =>
        {
            if (trainingEnv.InProgress)
            {
                trainingEnv.Genome.Fitness += LegHitReward / trainingEnv.Rocket.GetComponent<Rigidbody>().linearVelocity.magnitude;
            }
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
        UpdateFitness();
    }

    private void AssignGenomesToEnvironments()
    {
        while (genomesToEvaluate.Count > 0 && availableEnvironments.Count > 0)
        {
            var genome = genomesToEvaluate.Dequeue();
            var env = availableEnvironments.Dequeue();
            env.AssignGenome(genome);
            env.Rocket.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            env.InProgress = true;
            genome.Fitness = InitialFitness;
        }
    }

    private void EndSession(TrainingEnvironment env)
    {
        env.Rocket.transform.localPosition = new Vector3(0, RocketHeight, 0);
        env.Rocket.transform.rotation = Quaternion.identity;
        env.Rocket.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        env.InProgress = false;
        foreach (var e in env.Rocket.engines)
        {
            e.SetThrust(0);
        }
        env.DropGenome();
        availableEnvironments.Enqueue(env);

        if (genomesToEvaluate.Count == 0 && availableEnvironments.Count == environments.Count)
        {
            population.StoreBest();
            Debug.Log($"Generation {population.Generation} tested. Best fitness: {population.Best.Fitness}. Producing next generation");
            File.WriteAllText($"../data/{Label}_{population.Generation}.json", JsonConvert.SerializeObject(population.Snapshot()));
            population.NextGeneration();
            genomesToEvaluate = new Queue<Genome>(population.GetAllGenomes());
            Debug.Log($"New generation: {genomesToEvaluate.Count} genomes");
        }
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

    private void UpdateFitness()
    {
        foreach (var env in environments)
        {
            if (env.Genome != null)
            {
                env.Genome.Fitness += HeightRewardPerSecond * Time.deltaTime / (env.Rocket.transform.position.y / RocketHeight + 1);
                if (env.Genome.Fitness > 1000)
                {
                    EndSession(env);
                }
            }
        }
    }
}
