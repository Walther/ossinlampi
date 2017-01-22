using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Effects;

public class ExplosionPhysicsForce : MonoBehaviour
{
    public float explosionForce = 4;


    private void OnEnable()
    {
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        // wait one frame because some explosions instantiate debris which should then
        // be pushed by physics force
        yield return null;

        float multiplier = GetComponent<ParticleSystemMultiplier>().multiplier;

        float r = 10*multiplier;
        var cols = Physics.OverlapSphere(transform.position, r);
        var rigidbodies = new List<Rigidbody>();
        foreach (var col in cols)
        {
            if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody))
            {
                rigidbodies.Add(col.attachedRigidbody);
            }
        }
        foreach (var rb in rigidbodies)
        {
            rb.AddExplosionForce(explosionForce*multiplier, transform.position, r, 1*multiplier, ForceMode.Impulse);

            EnemyControllerBase enemy = rb.GetComponent<EnemyControllerBase>();
            if (enemy != null)
            {
                StartCoroutine(KillWithDelay(enemy, 0.5f));
            }
        }
    }

    private IEnumerator KillWithDelay(EnemyControllerBase enemy, float s)
    {
        yield return new WaitForSeconds(s);
        enemy.TakeDamage(float.MaxValue);
    }
}
