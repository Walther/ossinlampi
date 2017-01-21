﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class WaveManager : Singleton<WaveManager>
{
	[SerializeField]
	[Tooltip("Enemy spawning point")]
	private Transform _enemySpawningPoint;
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
		StartCoroutine (CoCreateNewWave (5, 1.0f));
	}

	private IEnumerator CoCreateNewWave (int numEnemies, float waitTime)
	{
		for (int i = 0; i < numEnemies; ++i)
		{
			EnemyControllerBase duck = Instantiate (_enemyPrefabs[0], _waveEnemyContainer) as EnemyControllerBase;
			_currentWave.AddEnemy (duck);
			yield return new WaitForSeconds (waitTime);
		}
	}
}
