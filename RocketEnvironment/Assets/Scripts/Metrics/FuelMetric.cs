using UnityEngine;

public class FuelMetric : MonoBehaviour, IMetric
{
    public Rocket rocket;
    public float fuelConsumption;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fuelConsumption = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        var engines = rocket.engines;
        foreach (var eng in engines)
        {
            fuelConsumption += eng.thrust;
        }
    }

    public float GetValue()
    {
        return fuelConsumption;
    }
}
