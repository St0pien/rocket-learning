using System;
using System.Collections.Generic;
using UnityEngine;

public class ColliderMetric : MonoBehaviour
{

    public event Action OnDestructiveHit;
    public event Action OnLegHit;
    public int LegCount { private set; get; } = 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (IsNonLegHit(collision))
        {
            OnDestructiveHit?.Invoke();
        }
        else
        {
            LegCount++;
            OnLegHit?.Invoke();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!IsNonLegHit(collision))
        {
            LegCount--;
        }
    }

    private bool IsNonLegHit(Collision collision)
    {
        foreach (var point in collision.contacts)
        {
            if (!point.otherCollider.CompareTag("Leg"))
            {
                return true;
            }
        }

        return false;
    }
}
