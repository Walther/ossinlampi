using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeOnCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        GameObject explosion = ExplosionManager.Instance.GetExplosion();
        explosion.transform.position = transform.position;
        gameObject.SetActive(false);
    }
}
