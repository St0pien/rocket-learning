using System;
using UnityEngine;

public class ColliderMetric : MonoBehaviour, IMetric
{
    public bool isTouchingGround = false;
    public float hitPunishment = 0f;
    public const float BAD_HIT_PUNISHMENT = -1000f;

    public float GetValue()
    {
        return hitPunishment;
    }
    private void OnCollisionEnter(Collision collision)
    {
        hitPunishment = IsNonLegHit(collision) ? BAD_HIT_PUNISHMENT : 0f;
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
