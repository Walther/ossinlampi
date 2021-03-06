using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityStandardAssets.CrossPlatformInput;
using WaterBuoyancy;
using DG.Tweening;

public class PlayerController : MonoBehaviour, IDamageable
{
	[Header("Player")]
	public FloatingObject 	floatingObject;
    public Rigidbody 		body;
	public float 			maxHp					= 500.0f;

	[Header("Cannon")]
	public ObjectPooler 	cannonballPooler;
    public Transform 		cannonballSpawn;
    public Transform  	 	cannonPivot;

    public float 			controlForce 			= 4.0f;
    public float    	    steerAmount         	= 5f;

	private float			_currentHp;

    [Header("Machine guns")]
    public MachineGun  		_leftGun;
    public MachineGun  		_rightGun;

	[Header("Effects")]
	public ParticleSystem 	_smokeParticleSystem;
	public int 				_maxSmokeParticles 		= 200;

	private float			_originalDensity 		= 0.0f;
	private bool			_deadAnimationPlayed 	= false;

	public float CurrentHp
	{
		get
		{
			return _currentHp;
		}
	}

	private void Awake ()
	{
		_currentHp = maxHp;
		_originalDensity = floatingObject.Density;
	}

    private void Start ()
    {
        VoiceController.Instance.AboveThresholdStream.Throttle(System.TimeSpan.FromMilliseconds(100)).Subscribe(voiceEvent =>
            {
                Fire(voiceEvent);
            });
    }

    private void FixedUpdate ()
    {
		// Handle controls if alive
		if (IsAlive ())
		{
			// Machine guns
	        if (CrossPlatformInputManager.GetButton("Fire Left"))
	        {
	            _leftGun.Fire(CrossPlatformInputManager.GetAxis("Horizontal Left"), CrossPlatformInputManager.GetAxis("Vertical Left"));
	        }

	        if (CrossPlatformInputManager.GetButton("Fire Right"))
	        {
	            _rightGun.Fire(CrossPlatformInputManager.GetAxis("Horizontal Right"), CrossPlatformInputManager.GetAxis("Vertical Right"));
	        }

			// Acceleration
			float control = controlForce * CrossPlatformInputManager.GetAxis("Vertical");
			body.AddRelativeForce(control*Vector3.right); 

			// Steering
			float rotate = steerAmount * CrossPlatformInputManager.GetAxis("Horizontal");
			body.AddTorque(0f, rotate, 0f);

			if (Input.GetKey(KeyCode.RightCommand))
			{
				Fire();
			}
		}
    }

	#region IDamageable

	public void TakeDamage (float damage)
	{
		_currentHp -= damage;

		if (_currentHp <= 0.0f)
		{
			Die ();				
		}

		if (_smokeParticleSystem != null)
		{
			_smokeParticleSystem.gameObject.SetActive (CurrentHp != maxHp);
			_smokeParticleSystem.maxParticles = (int)Mathf.Lerp (0.0f, (float)_maxSmokeParticles, 1.0f - (CurrentHp / maxHp));
			_smokeParticleSystem.Play ();
		}
	}

	#endregion

	public void Respawn ()
	{
		_currentHp = maxHp;
		_deadAnimationPlayed = false;
		floatingObject.Density = _originalDensity;

		if (_smokeParticleSystem != null)
		{
			_smokeParticleSystem.gameObject.SetActive (false);
			_smokeParticleSystem.maxParticles = 0;
			_smokeParticleSystem.Clear ();
		}

		gameObject.SetActive (true);
	}
		
	private void Die ()
	{
		if (_deadAnimationPlayed)
		{
			return;
		}

		_deadAnimationPlayed = true;

		DOTween.To (() => floatingObject.Density, x => floatingObject.Density = x, 1.0f, 2.5f)
			.OnStart (() => {
				AudioManager.Instance.PauseBackgroundClip ();
				AudioManager.Instance.PlayClip (AudioManager.GameAudioClip.PLAYER_DEAD);
			})
			.OnComplete (() => {
				GameManager.Instance.GoToState (GameState.GAME_OVER);
				AudioManager.Instance.UnPauseBackgroundClip ();

				if (_smokeParticleSystem != null)
				{
					_smokeParticleSystem.gameObject.SetActive (false);
					_smokeParticleSystem.Clear ();
				}
			});

		//gameObject.SetActive (false);
	}

	private bool IsAlive ()
	{
		return _currentHp > 0.0f;
	}

    private void Fire ()
    {
		Fire (Random.Range (0f, 90f), Random.Range (5f, 30f));
    }

    private void Fire(VoiceController.VoiceEvent voiceEvent)
    {
        float minFreq = 50f;
        float maxFreq = 400f;
        float clamped = Mathf.Clamp(voiceEvent.frequency, minFreq, maxFreq);
        float scaled = voiceEvent.volume * 200f;
        Debug.Log(string.Format("freq: {0} (clamped: {2}), vol: {1} (scaled: {3})", voiceEvent.frequency, voiceEvent.volume, clamped, scaled));

        Fire (90f*(clamped - minFreq)/(maxFreq - minFreq), 5f + scaled);
    }

    private void Fire (float angle, float power)
    {        
		if (IsAlive () && GameManager.Instance.CurrentState == GameState.PLAYING)
		{
	        Debug.LogFormat ("PlayerController Fire: Shooting with angle {0}, power {1}", angle, power);
            Vector3 enemyDirection = EnemyManager.Instance.GetClosestEnemyPosition(body.transform.position) - body.transform.position;
            enemyDirection.y = 0;
            enemyDirection.Normalize();
            enemyDirection *= Mathf.Cos(Mathf.Deg2Rad * angle);
            enemyDirection.y = Mathf.Sin(Mathf.Deg2Rad * angle);                

	        GameObject cannonball = cannonballPooler.GetPooledObject();
	        cannonball.transform.position = cannonballSpawn.position;
	        cannonball.transform.rotation = cannonballSpawn.transform.rotation;

//            Vector3 force = new Vector3(power * Mathf.Cos(Mathf.Deg2Rad*angle), power * Mathf.Sin(Mathf.Deg2Rad*angle), 0);
            Rigidbody rb = cannonball.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;

            rb.AddForce(enemyDirection * power, ForceMode.Impulse);
//            Vector3 newRot = new Vector3(cannonPivot.rotation.eulerAngles.x, 0f, angle - 90f);
            cannonPivot.rotation = Quaternion.Euler(enemyDirection);
			AudioManager.Instance.PlayClip (AudioManager.GameAudioClip.PLAYER_CANNON_FIRE);
		}
    }
}
