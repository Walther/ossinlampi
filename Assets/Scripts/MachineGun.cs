using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityStandardAssets.CrossPlatformInput;

public class MachineGun : MonoBehaviour
{
    public ObjectPooler BulletPool;
    public Transform BulletSpawnPoint;

    public void Fire (float x, float y)
    {
        Debug.Log(string.Format("Firing to ({0}, {1})", x, y));

        GameObject bullet = BulletPool.GetPooledObject();
        bullet.transform.position = BulletSpawnPoint.position;
        bullet.transform.rotation = BulletSpawnPoint.rotation;
        bullet.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(x, y));
    }

}

