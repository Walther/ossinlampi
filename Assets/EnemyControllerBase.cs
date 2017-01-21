using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControllerBase : MonoBehaviour
{
	[Header("Audio")]
	[SerializeField]
	protected AudioSource _audioSource;
	[SerializeField]
	protected AudioClip _takeDamageAudioClip;
	[SerializeField]
	protected AudioClip _giveDamageAudioClip;
	[SerializeField]
	protected AudioClip _dieAudioClip;

	[Header("Damage Controls")]
	[SerializeField]
	[Tooltip ("Hit points of the enemy")]
	protected float _hp = 1000.0f;
	[SerializeField]
	[Tooltip ("Damage taken from player shots")]
	protected float _damageTaken = 500.0f;
	[SerializeField]
	[Tooltip ("Damage given to player on collision")]
	protected float _damageGiven = 100.0f;

	virtual protected void OnCollisionEnter (Collision other)
	{
		Debug.LogFormat ("EnemyControllerBase OnCollisionEnter: {0}", other.gameObject.tag);

		// If the enemy collided with a projectile take damage
		if (other.gameObject.CompareTag ("Projectile"))
		{
			Debug.LogFormat ("Take damage");
			_hp -= _damageTaken;

			if (_hp <= 0.0f)
			{
				Die ();
			}
		}
		// If the enemy collided with a player give damage
		else if (other.gameObject.CompareTag ("Player"))
		{
			Debug.Log ("Cause damage");
		}
	}

	virtual protected void Die ()
	{
		Debug.Log ("Dieing");
		GameObject.Destroy (this);
	}
}
