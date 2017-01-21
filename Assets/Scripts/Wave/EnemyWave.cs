using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyWave
{
	[SerializeField]
	private List<EnemyControllerBase> _enemies;

	public int NumEnemies
	{
		get
		{
			return _enemies == null ? 0 : _enemies.Count;
		}
	}

	public EnemyWave ()
	{
		_enemies = new List<EnemyControllerBase> ();
	}

	/// <summary>
	/// Adds an enemy to the wave
	/// </summary>
	public void AddEnemy (EnemyControllerBase enemy)
	{
		_enemies.Add (enemy);
	}

	/// <summary>
	/// Determines whether this wave is complete - i.e. all enemies dead
	/// </summary>
	/// <returns><c>true</c> if this wave is complete; otherwise, <c>false</c>.</returns>
	public bool IsComplete ()
	{
		return !_enemies.Any (e => e.IsAlive ());
	}

	/// <summary>
	/// Clears the enemies from the wave
	/// </summary>
	public void ClearEnemies ()
	{
		int numEnemies = _enemies.Count;

		for (int i = 0; i < numEnemies; ++i)
		{
			GameObject.Destroy (_enemies[i].gameObject);
		}

		_enemies.Clear ();
	}
}
