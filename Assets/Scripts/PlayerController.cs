using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour, IDamageable
{
	[Header("Player")]
    public Rigidbody 	body;
	public float 		maxHp				= 500.0f;

	[Header("Cannon")]
	public ObjectPooler cannonballPooler;
    public Transform 	cannonballSpawn;
    public float 		controlForce 		= 4.0f;

	private float		_currentHp;

    private void Start ()
    {
        VoiceController.Instance.AboveThresholdStream.Throttle(System.TimeSpan.FromMilliseconds(100)).Subscribe(voiceEvent =>
            {
                Fire(voiceEvent);
            });
    }

	private void Update () 
    {
        float control = controlForce * CrossPlatformInputManager.GetAxis("Horizontal");
        body.AddForce(control*Vector3.right);

        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
	}

	#region IDamageable

	public void TakeDamage (float damage)
	{
		_currentHp -= damage;

		if (_currentHp <= 0.0f)
		{
			gameObject.SetActive (false);
			GameManager.Instance.GoToState (GameState.GAME_OVER);
		}
	}

	#endregion

	public void Respawn ()
	{
		_currentHp = maxHp;
		gameObject.SetActive (true);
	}

	private bool IsAlive ()
	{
		return _currentHp > 0.0f;
	}

    private void Fire()
    {
		Fire (Random.Range (0f, 90f), Random.Range (5f, 30f));
    }

    private void Fire(VoiceController.VoiceEvent voiceEvent)
    {
        float minFreq = 50f;
        float maxFreq = 400f;
        float clamped = Mathf.Clamp(voiceEvent.frequency, minFreq, maxFreq);
        float scaled = voiceEvent.volume * 100f;
        Debug.Log(string.Format("freq: {0} (clamped: {2}), vol: {1} (scaled: {3})", voiceEvent.frequency, voiceEvent.volume, clamped, scaled));

        Fire(90*(clamped - minFreq)/(maxFreq - minFreq), 5f + scaled);
    }

    private void Fire(float angle, float power)
    {        
		if (IsAlive ())
		{
	        Debug.LogFormat ("PlayerController Fire: Shooting with angle {0}, power {1}", angle, power);
	        GameObject cannonball = cannonballPooler.GetPooledObject();
	        cannonball.transform.position = cannonballSpawn.position;
	        cannonball.transform.rotation = cannonballSpawn.transform.rotation;
	        Vector3 force = new Vector3(0f, power * Mathf.Sin(Mathf.Deg2Rad*angle), power * Mathf.Cos(Mathf.Deg2Rad*angle));
	        cannonball.GetComponent<Rigidbody>().AddRelativeForce(force, ForceMode.Impulse);
		}
    }
}
