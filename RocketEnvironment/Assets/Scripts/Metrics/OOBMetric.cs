using System;
using UnityEngine;

public class OOBMetric : MonoBehaviour
{
    public event Action OnOutOfBounds;
    private GameObject target;

    public void Track(GameObject target)
    {
        this.target = target;
    }

    public void OnTriggerExit(Collider other)
    {
        OnOutOfBounds?.Invoke();
    }
}
