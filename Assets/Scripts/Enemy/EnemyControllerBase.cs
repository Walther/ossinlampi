using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshRenderer))]
public class EnemyControllerBase : MonoBehaviour, IDamageable
{
	[Header("Movement")]
	[SerializeField]
	protected float _movementSpeed = 2.0f;

	[Header("Audio")]
	[SerializeField]
	protected AudioClip _takeDamageAudioClip;
	[SerializeField]
	protected AudioClip _giveDamageAudioClip;
	[SerializeField]
	protected AudioClip _dieAudioClip;

	[Header("Effects")]
	[SerializeField]
	protected ParticleSystem _dieEffect;

	[Header("Damage Controls")]
	[SerializeField]
	[Tooltip ("Hit points of the enemy")]
	protected float _maxHp = 1000.0f;
	[SerializeField]
	[Tooltip ("Damage taken from player shots")]
	protected float _damageTaken = 500.0f;
	[SerializeField]
	[Tooltip ("Damage given to player on collision")]
	protected float _damageGiven = 100.0f;

	[Header("Score")]
	[SerializeField]
	[Tooltip ("Score award for killing this enemy")]
	protected int _killScore = 1000;

	protected Transform _targetTransform = null;
	protected Rigidbody _enemyRigidbody = null;
	protected MeshRenderer _enemyMeshRenderer = null;
	protected float _currentHp = 0.0f;

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

	virtual protected MeshRenderer EnemyMeshRenderer
	{
		get
		{
			if (_enemyMeshRenderer == null)
			{
				_enemyMeshRenderer = GetComponent<MeshRenderer> ();
			}

			return _enemyMeshRenderer;
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

	virtual protected void OnEnable ()
	{
		_currentHp = _maxHp;
	}

	virtual protected void OnDisable ()
	{
		_currentHp = 0.0f;
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

		_currentHp -= damage;

		if (_currentHp <= 0.0f)
		{
			AudioManager.Instance.PlayClip (_dieAudioClip);
			GameManager.Instance.AddScore (_killScore);
			Die ();
		}
		else
		{
			AudioManager.Instance.PlayClip (_takeDamageAudioClip);
		}
	}

	#endregion

	virtual protected void GiveDamage (float damage)
	{
		Debug.LogFormat ("EnemyControllerBase GiveDamage: {0}", damage);
		GameManager.Instance.DamagePlayer (damage);
		AudioManager.Instance.PlayClip (_giveDamageAudioClip);
		Die ();
	}

	virtual protected void Die ()
	{
		// If there is an explosion animation play it
		if (_dieEffect != null)
		{
			StartCoroutine (DieWithExplosion ());
		}
		else
		{
			gameObject.SetActive (false);
		}
	}

	private IEnumerator DieWithExplosion ()
	{
		EnemyMeshRenderer.enabled = false;
		_dieEffect.gameObject.SetActive (true);
		_dieEffect.Emit (1);
		
		yield return new WaitForSeconds (_dieEffect.main.duration);

		_dieEffect.Clear (true);
		_dieEffect.gameObject.SetActive (true);

		gameObject.SetActive (false);
		EnemyMeshRenderer.enabled = true;
	}

	virtual public bool IsAlive ()
	{
		return _currentHp > 0f;
	}
}
