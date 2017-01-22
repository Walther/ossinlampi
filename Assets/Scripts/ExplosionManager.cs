using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : Singleton<ExplosionManager>
{
    public ObjectPooler ExplosionPooler;

    public GameObject GetExplosion()
    {
        return ExplosionPooler.GetPooledObject();
    }
}
