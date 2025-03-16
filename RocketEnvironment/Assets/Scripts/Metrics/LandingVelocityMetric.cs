using System;
using UnityEngine;

public class LandingVelocityMetric : MonoBehaviour, IMetric
{
    public const float VELOCITY_PUNISHMENT = -10f;

    public Rigidbody rb;
    public float punishment = 0f;
    public IVelocityStrategy strategy;

    void Start()
    {
        strategy = new MagnitudeVelocityStrategy();
    }
    private void OnCollisionEnter(Collision collision)
    {
        punishment = IsCollisionWithGround(collision) ? strategy.GetVelocity(rb.linearVelocity) * VELOCITY_PUNISHMENT : 0f;
    }

    public float GetValue()
    {
        return punishment;
    }

    public bool IsCollisionWithGround(Collision collision)
    {
        foreach (var point in collision.contacts)
        {
            if (point.otherCollider.CompareTag("Ground"))
            {
                return true;
            }
        }

        return false;
    }
}

public interface IVelocityStrategy
{
    public float GetVelocity(Vector3 velocity);
}

public class MagnitudeVelocityStrategy : IVelocityStrategy
{
    public float GetVelocity(Vector3 velocity)
    {
        return velocity.magnitude;
    }
}
