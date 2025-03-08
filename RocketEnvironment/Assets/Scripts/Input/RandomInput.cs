using System.Collections.Generic;
using UnityEngine;

public class RandomInput : MonoBehaviour
{
    public Rocket rocket;
    public float HoldInput = 1f;
    public float HoldIdle = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(GetInput());
    }

    IEnumerator<YieldInstruction> GetInput()
    {
        while (true)
        {
            yield return new WaitForSeconds(HoldIdle);
            foreach (var engine in rocket.engines)
            {
                engine.SetThrust(Random.Range(0f, 1f));
            }
            yield return new WaitForSeconds(HoldInput);
            foreach (var engine in rocket.engines)
            {
                engine.SetThrust(0);
            }
        }
    }
}
