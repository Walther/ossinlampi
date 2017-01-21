using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		// Destroy all children of the waveEnemyContainer
		// Create new enemies to a new wave

	}
}
