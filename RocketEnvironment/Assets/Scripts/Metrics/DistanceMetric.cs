using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistanceMetric : MonoBehaviour, IMetric
{
    public Transform objectTransform;
    public float distance;
    public IDistanceStrategy strategy;

    public const float angleFromPerpendicular = Mathf.PI / 32.0f;
    public const int numOfRays = 4;
    List<Vector3> GetDirections()
    {
        var result = new List<Vector3>();

        var cos = Mathf.Cos(angleFromPerpendicular);
        var sin = Mathf.Sin(angleFromPerpendicular);

        if (numOfRays == 1)
        {
            result.Add(Vector3.down);
            return result;
        }

        for (int i = 0; i < numOfRays; i++)
        {
            float angle = 2 * Mathf.PI / numOfRays * i;
            result.Add(new Vector3(Mathf.Sin(angle) * sin, -cos, Mathf.Cos(angle) * sin));
        }

        return result;
    }

    void Start()
    {
        strategy = new AvgDistanceStrategy();
    }

    void Update()
    {
        distance = strategy.CalculateDistance(GetDistances());
    }

    public float GetValue()
    {
        return distance;
    }

    List<float> GetDistances()
    {
        var distances = new List<float>();
        var directions = GetDirections();
        foreach (var dir in directions)
        {
            Ray ray = new Ray(objectTransform.position, dir);
            RaycastHit hit;
            // TODO: add LayerMask
            if (Physics.Raycast(ray, out hit))
            {
                distances.Add(Vector3.Distance(objectTransform.position, hit.point));
                Debug.DrawRay(objectTransform.position, dir * 10000f, Color.green, 1f);
            }
        }

        return distances;
    }

}

public interface IDistanceStrategy
{
    public float CalculateDistance(List<float> distances);
}

public class AvgDistanceStrategy : IDistanceStrategy
{
    public float CalculateDistance(List<float> distances)
    {
        if (distances.Count == 0)
        {
            return float.MaxValue;
        }
        return distances.Average();
    }
}
