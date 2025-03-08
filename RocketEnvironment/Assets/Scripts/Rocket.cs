using System;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [Serializable]
    public class Engine
    {
        public string id;
        public Transform transform;
        public ParticleSystem fireFX { private set; get; }
        private float maxParticleLifetime = 0f;

        public void AttachFireFX(ParticleSystem system)
        {
            fireFX = system;
            var main = fireFX.main;
            this.maxParticleLifetime = main.startLifetime.constant;
            main.startLifetime = 0f;
        }

        public Vector3 thrustVector
        {
            get
            {
                return transform.forward * thrust;
            }
        }
        public float thrust { private set; get; }
        public void SetThrust(float value)
        {
            thrust = value;

            if (value < 0.01)
            {
                fireFX.Stop();
                return;
            }

            fireFX.Play();

            var main = fireFX.main;
            main.startLifetime = maxParticleLifetime * thrust;
        }
    }

    public List<Engine> engines;
    public ParticleSystem firePrefab;
    public float engineThrustMultiplier = 1.0f;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        foreach (var engine in engines)
        {
            engine.AttachFireFX(Instantiate(firePrefab, engine.transform));
        }
        // StartCoroutine(RandomInput());
    }

    // Update is called once per frame
    void Update()
    {
        // TmpUserInput();
        foreach (var engine in engines)
        {
            rb.AddForceAtPosition(engine.thrustVector * engineThrustMultiplier, engine.transform.position);
        }
    }

    void TmpUserInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("Engine run");
            foreach (var engine in engines)
            {
                engine.SetThrust(1f);
            }
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            Debug.Log("Engine stop");
            foreach (var engine in engines)
            {
                engine.SetThrust(0f);
            }
        }
    }

    IEnumerator<YieldInstruction> RandomInput()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            foreach (var engine in engines)
            {
                engine.SetThrust(UnityEngine.Random.Range(0f, 1f));
            }

            yield return new WaitForSeconds(1f);

            foreach (var engine in engines)
            {
                engine.SetThrust(UnityEngine.Random.Range(0f, 1f));
            }
        }
    }
}
