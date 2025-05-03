using System;
using UnityEngine;

public class ColliderMetric : MonoBehaviour
{

    public event Action OnDestructiveHit;
    public event Action OnLegHit;

    private void OnCollisionEnter(Collision collision)
    {
        if (IsNonLegHit(collision))
        {
            OnDestructiveHit?.Invoke();
        }
        else
        {
            OnLegHit?.Invoke();
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
