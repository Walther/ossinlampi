using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityStandardAssets.CrossPlatformInput;

public class MachineGun : MonoBehaviour
{
    public ObjectPooler BulletPool;
    public Transform BulletSpawnPoint;

    public float BulletInitialForce;

    private Vector3 lastForce = Vector3.zero;

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(GetComponent<Transform>().position, GetComponent<Transform>().position + lastForce);
    }

    public void Fire (float x, float y)
    {
//        Debug.Log(string.Format("Firing to ({0}, {1})", x, y));

        GameObject bullet = BulletPool.GetPooledObject();
        bullet.transform.position = BulletSpawnPoint.position;
        bullet.transform.rotation = BulletSpawnPoint.rotation;
        bullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
        bullet.GetComponent<TrailRenderer>().Clear();
        Vector3 direction = transform.right * y + transform.up * x;
        direction.Normalize();
//        direction += transform.forward * 0.2f;
        lastForce = direction * BulletInitialForce;
        bullet.GetComponent<Rigidbody>().AddForce(lastForce);
    }

}

