using UnityEngine;

public class MetricBasedInput : MonoBehaviour
{
    public Rocket rocket;
    public FuelMetric fuelMetric;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var engine in rocket.engines)
        {
            if (fuelMetric.GetValue() < 1000f)
            {
                engine.SetThrust(1.0f);
            }
            else
            {
                engine.SetThrust(0f);
            }
        }
    }
}
