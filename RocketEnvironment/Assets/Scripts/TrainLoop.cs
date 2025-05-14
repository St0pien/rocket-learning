using System.Collections.Generic;
using UnityEngine;
using NEAT;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System;

public class TrainLoop : MonoBehaviour
{

    private class TrainingEnvironment
    {
        public GameObject Environment;
        public GameObject Ground;
        public Rocket Rocket;
        public LandingVelocityMetric LandingVelocity;
        public ColliderMetric GroundHit;
        public DistanceMetric[] Distances;
        public OOBMetric Bounds;
        public Genome Genome { private set; get; }
        public NeuralNetwork network;
        public bool InProgress = false;
        public Coroutine timerCoroutine;

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
    public float SessionDuration = 30f;
    public float InitialFitness = 100f;
    public float OOBPunishment = 100f;
    public float DestructiveHitPunishment = 100f;
    public float LegPlacementRewardPerSecond = 2f;
    public float LegHitVelocityPunishmentMultiplier = 5f;
    public float HeightRewardPerSecond = 1f;
    public float VelocityPunishmentPerSecond = 1f;
    public float AngularVelocityPunishmentPerSecond = 1f;
    public float AnglePunishmentPerSecond = 1f;

    [Header("Evolution")]
    public int PopulationSize = 150;
    public bool FullyConnected = false;
    public bool InitialRandomWeights = true;
    public float MinWeight = -30;
    public float MaxWeight = -30;
    public float TweakWeightProb = 0.8f;
    public float ReplaceWeightProb = 0.1f;
    public float TweakMultiplier = 1f;
    public float ConnDeleteProb = 0.012f;
    public float ConnAddProb = 0.012f;
    public float NodeAddProb = 0.2f;
    public float NodeDeleteProb = 0.009f;

    [Header("Speciation")]

    public float CompatibilityDisjointCoefficient = 1f;
    public float CompatibilityWeightCoefficient = 0.4f;
    public float CompatibilityThreshold = 5f;

    [Header("Stagnation")]
    public int MaxStagnation = 10;
    public int SpeciesElitism = 1;

    [Header("Reproduction")]
    public int Elitism = 0;
    public float SurvivalThreshold = 0.2f;

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
            PopulationSize = PopulationSize,
            genomeConfig = new GenomeConfig()
            {
                Inputs = 11,
                Outpus = 4,
                FullyConnected = FullyConnected,
                InitialRandomWeights = InitialRandomWeights,

                MinWeight = MinWeight,
                MaxWeight = MaxWeight,
                TweakWeightProb = TweakWeightProb,
                ReplaceWeightProb = ReplaceWeightProb,
                TweakMultiplier = TweakMultiplier,
                ConnDeleteProb = ConnDeleteProb,
                ConnAddProb = ConnAddProb,
                NodeAddProb = NodeAddProb,
                NodeDeleteProb = NodeDeleteProb
            },
            speciesConfig = new SpeciesConfig()
            {
                CompatibilityDisjointCoefficient = CompatibilityDisjointCoefficient,
                CompatibilityWeightCoefficient = CompatibilityWeightCoefficient,
                CompatibilityThreshold = CompatibilityThreshold,
            },
            stagnationConfig = new StagnationConfig()
            {
                MaxStagnation = MaxStagnation,
                SpeciesElitism = SpeciesElitism
            },
            reproductionConfig = new ReproductionConfig()
            {
                Elitism = Elitism,
                SurvivalThreshold = SurvivalThreshold
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
            Ground = ground,
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
                EndSession(trainingEnv);
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
            env.timerCoroutine = StartCoroutine(Timer(SessionDuration, () =>
            {
                EndSession(env);
            }));
            genome.Fitness = InitialFitness;

            if (population.Best != null && genome.Id == population.Best.Id)
            {
                env.Ground.GetComponent<MeshRenderer>().materials[0].color = Color.green;
            }
        }
    }

    private void EndSession(TrainingEnvironment env)
    {
        env.Rocket.transform.localPosition = new Vector3(0, RocketHeight, 0);
        // env.Rocket.transform.rotation = Quaternion.Euler(UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f));
        env.Rocket.transform.rotation = Quaternion.identity;
        // env.Rocket.transform.rotation = Quaternion.Euler(45, 45, 45);
        env.Rocket.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        env.InProgress = false;
        StopCoroutine(env.timerCoroutine);
        env.timerCoroutine = null;
        foreach (var e in env.Rocket.engines)
        {
            e.SetThrust(0);
        }
        env.DropGenome();
        availableEnvironments.Enqueue(env);
        env.Ground.GetComponent<MeshRenderer>().materials[0].color = Color.gray;

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
                {2, env.Rocket.transform.position.y},
                {3, env.Rocket.GetComponent<Rigidbody>().linearVelocity.x},
                {4, env.Rocket.GetComponent<Rigidbody>().linearVelocity.y},
                {5, env.Rocket.GetComponent<Rigidbody>().linearVelocity.z},
                {6, env.Rocket.transform.up.x},
                {7, env.Rocket.transform.up.y},
                {8, env.Rocket.transform.up.z},
                {9, env.Rocket.GetComponent<Rigidbody>().angularVelocity.x},
                {10, env.Rocket.GetComponent<Rigidbody>().angularVelocity.y},
                {11, env.Rocket.GetComponent<Rigidbody>().angularVelocity.z}
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
                float normalizedHeightReward = 1 / (env.Rocket.transform.position.y + 1);
                env.Genome.Fitness += normalizedHeightReward * Time.deltaTime * HeightRewardPerSecond;

                env.Genome.Fitness += LegPlacementRewardPerSecond * Time.deltaTime * env.GroundHit.LegCount * env.GroundHit.LegCount;

                env.Genome.Fitness -= env.Rocket.GetComponent<Rigidbody>().linearVelocity.magnitude * Time.deltaTime * VelocityPunishmentPerSecond;
                env.Genome.Fitness -= env.Rocket.GetComponent<Rigidbody>().angularVelocity.magnitude * Time.deltaTime * AngularVelocityPunishmentPerSecond;
                float angle = Vector3.Angle(env.Rocket.transform.up, Vector3.up);
                env.Genome.Fitness -= angle / 180 * Time.deltaTime * AnglePunishmentPerSecond;
            }
        }
    }

    private IEnumerator Timer(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }
}
