using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class WaveManager : Singleton<WaveManager>
{
	
	[SerializeField]
	[Tooltip("Enemy spawning area center")]
	private Transform _spawningAreaCenter;
	[SerializeField]
	[Tooltip("Enemy spawning area radius")]
	private float _spawningAreaRadius = 20.0f;

	[SerializeField]
	[Tooltip("Container for the enemies in the wave")]
	private Transform _waveEnemyContainer;
	[SerializeField]
	private EnemyControllerBase[] _enemyPrefabs;

	private EnemyWave _currentWave;

	public bool HasActiveWave
	{
		get
		{
			return _currentWave != null && !_currentWave.IsComplete ();
		}
	}

	public void CreateNewWave ()
	{
		// Destroy all enemeis from the current wave
		if (_currentWave != null)
		{
			_currentWave.ClearEnemies ();
		}

		_currentWave = new EnemyWave ();
		StartCoroutine (CreateNewWave (5, 1.0f));
	}

	public void ClearWaves ()
	{
		// Stop any possible spawning routines
		StopAllCoroutines ();

		// Clear all enemies from waves
		if (_currentWave != null)
		{
			_currentWave.ClearEnemies ();
		}

		_currentWave = null;
	}

	private IEnumerator CreateNewWave (int numEnemies, float waitTime)
	{
		for (int i = 0; i < numEnemies; ++i)
		{
			Vector2 randomOffset = Random.insideUnitCircle * _spawningAreaRadius;
			Vector3 spawningPosition = _spawningAreaCenter.position;

			spawningPosition.x += randomOffset.x;
			spawningPosition.z = randomOffset.y;

			EnemyControllerBase duck = Instantiate (_enemyPrefabs[0], spawningPosition, Quaternion.identity, _waveEnemyContainer) as EnemyControllerBase;
			_currentWave.AddEnemy (duck);
			yield return new WaitForSeconds (waitTime);
		}
	}
}
