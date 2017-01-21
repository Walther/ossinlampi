using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class EnemyManager : Singleton<EnemyManager>
{
	[System.Serializable]
	public class EnemyPool
	{
		[Tooltip("Object pool containing GameObjects with EnemyControllerBase objects")]
		public ObjectPooler objectPool;
		[Tooltip("Probability of using the enemy from this object pool")]
		public float probability;
	}

	[SerializeField]
	[Tooltip("Enemy spawning area center")]
	private Transform _spawningAreaCenter;
	[SerializeField]
	[Tooltip("Enemy spawning area radius")]
	private float _spawningAreaRadius = 20.0f;

	[SerializeField]
	[Tooltip("Container for the enemies")]
	private Transform _enemyContainer;
	[SerializeField]
	private EnemyPool[] _enemyPools;

	private List<EnemyControllerBase> _currentEnemies;

	private List<EnemyControllerBase> CurrentEnemies
	{
		get
		{
			if (_currentEnemies == null)
			{
				_currentEnemies = new List<EnemyControllerBase> ();
			}

			return _currentEnemies;
		}
	}

	private void Awake ()
	{
		// Normalize enemy pool probabilities
		int numEnemyPools = _enemyPools.Length;
		float sum = _enemyPools.Sum (ep => ep.probability);

		for (int i = 0; i < numEnemyPools; i++)
		{
			_enemyPools[i].probability /= sum;
		}

		_enemyPools.OrderBy (ep => ep.probability);
	}

	Coroutine co;

	public void ClearEnemies ()
	{
		// Stop any possible spawning routines
		if (co != null)
		{
			StopCoroutine (co);
			co = null;
		}

		int numEnemyPools = _enemyPools.Length;

		for (int i = 0; i < numEnemyPools; ++i)
		{
			_enemyPools[i].objectPool.ResetObjectPool ();
		}

		_currentEnemies.Clear ();
	}

	public void StartSpawningEnemies ()
	{
		co = StartCoroutine (CreateNewEnemies (10, 1.0f));
	}

	private IEnumerator CreateNewEnemies (int numEnemies, float waitTime)
	{
		for (int i = 0; i < numEnemies; ++i)
		{
			CreateNewEnemy ();
			yield return new WaitForSeconds (waitTime);
		}
	}

	private EnemyControllerBase CreateNewEnemy ()
	{
		// Calculate the spawning position
		Vector2 randomOffset = Random.insideUnitCircle * _spawningAreaRadius;
		Vector3 spawningPosition = _spawningAreaCenter.position;
		spawningPosition.x += randomOffset.x;
		spawningPosition.z = randomOffset.y;

		// Get random enemy according to the probabilities
		float rand = Random.Range (0.0f, 1.0f);

		EnemyPool enemyPool = null;
		float accumulatedProbability = 0.0f;
		int numEnemyPools = _enemyPools.Length;

		for (int i = 0; i < numEnemyPools; ++i)
		{
			accumulatedProbability += _enemyPools[i].probability;

			if ((1.0f - accumulatedProbability) <= rand)
			{
				enemyPool = _enemyPools[i];
				break;
			}
		}
			
		GameObject go = enemyPool.objectPool.GetPooledObject ();
		go.transform.position = spawningPosition;
		go.transform.rotation = Quaternion.identity;
		EnemyControllerBase enemyControllerBase = go.GetComponent<EnemyControllerBase> ();

		if (enemyControllerBase != null)
		{
			CurrentEnemies.Add (enemyControllerBase);
		}

		return enemyControllerBase;
	}
}
