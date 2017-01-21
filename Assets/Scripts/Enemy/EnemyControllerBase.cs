﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyControllerBase : MonoBehaviour, IDamageable
{
	[Header("Movement")]
	[SerializeField]
	protected float _movementSpeed = 2.0f;

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

	protected Transform _targetTransform = null;
	protected Rigidbody _enemyRigidbody = null;

	virtual protected Transform TargetTransform
	{
		get
		{
			if (_targetTransform == null)
			{
				_targetTransform = GameManager.Instance.CurrentPlayer.transform;
			}

			return _targetTransform;
		}
	}

	virtual protected Rigidbody EnemyRigidbody
	{
		get
		{
			if (_enemyRigidbody == null)
			{
				_enemyRigidbody = GetComponent<Rigidbody> ();
			}

			return _enemyRigidbody;
		}
	}

	virtual protected void FixedUpdate ()
	{
		if (IsAlive ())
		{
			Vector3 targetPosition = TargetTransform.position;
			Vector3 movementDir = (targetPosition - transform.position).normalized;
			transform.LookAt (targetPosition);
			EnemyRigidbody.AddForce (_movementSpeed * movementDir);
		}
	}

	virtual protected void OnCollisionEnter (Collision other)
	{
		// If the enemy collided with a projectile take damage
		if (other.gameObject.CompareTag ("Projectile"))
		{
			TakeDamage (_damageTaken);
		}
		// If the enemy collided with a player give damage
		else if (other.gameObject.CompareTag ("Player"))
		{
			GiveDamage (_damageGiven);
		}
	}

	#region IDamageable

	virtual public void TakeDamage (float damage)
	{
		Debug.LogFormat ("EnemyControllerBase TakeDamage: {0}", damage);

		_hp -= damage;

		if (_hp <= 0.0f)
		{
			Die ();
		}
	}

	#endregion

	virtual protected void GiveDamage (float damage)
	{
		Debug.LogFormat ("EnemyControllerBase GiveDamage: {0}", damage);
		GameManager.Instance.CurrentPlayer.TakeDamage (damage);
		GameObject.Destroy (this.gameObject);
	}

	virtual protected void Die ()
	{
		GameObject.Destroy (this.gameObject);
	}

	public bool IsAlive ()
	{
		return _hp > 0f;
	}
}
