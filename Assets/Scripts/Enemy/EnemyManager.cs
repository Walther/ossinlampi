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
	[Tooltip("How often is the difficulty increased (sec)?")]
	private float _difficultyIncreaseInterval = 30.0f;
	[SerializeField]
	[Tooltip("How often is the number of minimum ducks checked (sec)?")]
	private float _enemyMinCheckInterval = 3.0f;
	[SerializeField]
	[Tooltip("What is the minimum time between duck spawns?")]
	private float _enemyMinSpawnInterval = 0.3f;

	[SerializeField]
	[Tooltip("Container for the enemies")]
	private Transform _enemyContainer;
	[SerializeField]
	private EnemyPool[] _enemyPools;

	private int _currentDifficultyLevel = 1; 	// Current difficulty level
	private int _minEnemies = 5;				// Minimum amount of enemies in the scene
	private int _maxEnemies = 7;				// Maximum amount of enemies in the scene

	private float _lastDifficultyIncreaseTime;
	private float _lastMinCheckTime;

	private List<Coroutine> _coroutines;
	private bool _spawning = false;

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

    public Vector3 GetClosestEnemyPosition(Vector3 position)
    {
        if (CurrentEnemies.Any(e => e.IsAlive()))           
        {
            return CurrentEnemies.Where(e => e.IsAlive()).Select(e => e.transform.position).OrderBy(pos => (pos - position).sqrMagnitude).First();
        }
        else
        {
            return position;
        }
    }

	private List<Coroutine> Coroutines
	{
		get
		{
			if (_coroutines == null)
			{
				_coroutines = new List<Coroutine> ();
			}

			return _coroutines;
		}
	}

	private int NumAliveEnemies
	{
		get
		{
			return CurrentEnemies.Count (ecb => ecb != null && ecb.IsAlive ());
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
		
	public void ClearAndStopSpawningEnemies ()
	{
		// Mark as not spawning
		_spawning = false;

		// Stop any possible spawning routines
		if (Coroutines != null)
		{
			int numCoroutines = Coroutines.Count;

			for (int i = 0; i < numCoroutines; ++i)
			{
				if (Coroutines[i] != null)
				{
					StopCoroutine (Coroutines[i]);
				}
			}
		}

		Coroutines.Clear ();

		// Reset all enemies
		int numEnemyPools = _enemyPools.Length;

		for (int i = 0; i < numEnemyPools; ++i)
		{
			_enemyPools[i].objectPool.ResetObjectPool ();
		}

		_currentEnemies.Clear ();
	}

	public void StartSpawningEnemies ()
	{
		// Mark as spawning
		_spawning = true;

		// Set initial parameters
		_currentDifficultyLevel = 1;
		_minEnemies = 5;
		_maxEnemies = 7;
		_lastDifficultyIncreaseTime = Time.time;
		_lastMinCheckTime = Time.time;

		// Create the initial enemies
		Coroutines.Add (StartCoroutine (CreateNewEnemies (_maxEnemies, _enemyMinSpawnInterval)));

		// Start the difficulty increase interval
		Coroutines.Add (StartCoroutine (IncreaseDifficulty ()));

		// Start the min check interval
		Coroutines.Add (StartCoroutine (CheckMinimumNumberOfEnemies ()));
	}

	/// <summary>
	/// Increases the difficulty i.e. increases the min and max number of enemies
	/// and spawns the enemies
	/// </summary>
	/// <returns>The difficulty.</returns>
	private IEnumerator IncreaseDifficulty ()
	{
		while (_spawning)
		{
			yield return new WaitForSeconds (_difficultyIncreaseInterval);

			_currentDifficultyLevel++;
			_minEnemies = _minEnemies + 1;
			_maxEnemies = _maxEnemies + 3;

			int numEnemiesToSpawn = _maxEnemies - NumAliveEnemies;

			if (numEnemiesToSpawn > 0)
			{
				StartCoroutine (CreateNewEnemies (numEnemiesToSpawn, _enemyMinSpawnInterval));
			}

			_lastDifficultyIncreaseTime = Time.time;
		}
	}

	/// <summary>
	/// Ensures on regular intervals that there is a minimum number of enemies
	/// in the scene.
	/// </summary>
	/// <returns>The minimum number of enemies.</returns>
	private IEnumerator CheckMinimumNumberOfEnemies ()
	{
		while (_spawning)
		{
			yield return new WaitForSeconds (_enemyMinCheckInterval);

			int numAliveEnemies = NumAliveEnemies;

			if (numAliveEnemies < _minEnemies)
			{
				for (int i = 0; i < _minEnemies-numAliveEnemies; ++i)
				{
					CreateNewEnemy ();
				}
			}

			_lastMinCheckTime = Time.time;
		}
	}

	/// <summary>
	/// Creates a number of new enemies with the specified spawn interval
	/// </summary>
	/// <returns>The new enemies.</returns>
	/// <param name="numEnemies">Number enemies.</param>
	/// <param name="waitTime">Wait time between spawns.</param>
	private IEnumerator CreateNewEnemies (int numEnemies, float waitTime)
	{
		for (int i = 0; i < numEnemies; ++i)
		{
			if (!_spawning)
			{
				break;
			}

			CreateNewEnemy ();
			yield return new WaitForSeconds (waitTime);
		}
	}

	/// <summary>
	/// Creates new enemy immediately
	/// </summary>
	private void CreateNewEnemy ()
	{
		if (NumAliveEnemies >= _maxEnemies || !_spawning)
		{
			return;
		}

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
		float scale = Random.Range (0.5f, 3.0f);
		enemyControllerBase.ScaleValues (scale);

		if (enemyControllerBase != null)
		{
			// Do not add duplicates to CurrentEnemies list
			if (!CurrentEnemies.Contains (enemyControllerBase))
			{
				CurrentEnemies.Add (enemyControllerBase);
			}
		}
	}
}
