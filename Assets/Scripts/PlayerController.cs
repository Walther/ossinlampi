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
    public Transform    cannonPivot;

    public float 		controlForce 		= 4.0f;
    public float        steerAmount         = 5f;

	private float		_currentHp;

	private void Awake ()
	{
		_currentHp = maxHp;
	}

    private void Start ()
    {
        VoiceController.Instance.AboveThresholdStream.Throttle(System.TimeSpan.FromMilliseconds(100)).Subscribe(voiceEvent =>
            {
                Fire(voiceEvent);
            });
    }

	private void Update () 
    {
        float control = controlForce * CrossPlatformInputManager.GetAxis("Vertical");
        float rotate = steerAmount * CrossPlatformInputManager.GetAxis("Horizontal");
//        float keepUpright = transform.rotation.eulerAngles;
        float uprightX = 0f;

        float deltaAngle = AngleUtils.ClampAngle180(body.transform.localRotation.eulerAngles.x);

        Debug.Log(string.Format("left: ({0},{1}), right: ({2},{3})", CrossPlatformInputManager.GetAxis("Horizontal Left"), CrossPlatformInputManager.GetAxis("Vertical Left"), CrossPlatformInputManager.GetAxis("Horizontal Right"), CrossPlatformInputManager.GetAxis("Vertical Right")));




//        Debug.Log(string.Format("original: {0}, originalClamped: {1}, deltaClamped: {2}", body.transform.localRotation.eulerAngles.x, xAngle, deltaAngle));


        body.AddRelativeForce(control*Vector3.right);

        Vector3 targetEuler = body.transform.localRotation.eulerAngles;
        targetEuler.x = 0;

//        body.AddRelativeTorque(deltaAngle, 0f, 0f);
//        body.transform.localRotation = Quaternion.RotateTowards(body.transform.localRotation, Quaternion.Euler(targetEuler), 100f);
            

        body.AddTorque(0f, rotate, 0f);


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

        Fire (20f + 90f*(clamped - minFreq)/(maxFreq - minFreq), 5f + scaled);
    }

    private void Fire (float angle, float power)
    {        
		if (IsAlive () && GameManager.Instance.CurrentState == GameState.PLAYING)
		{
	        Debug.LogFormat ("PlayerController Fire: Shooting with angle {0}, power {1}", angle, power);
	        GameObject cannonball = cannonballPooler.GetPooledObject();
	        cannonball.transform.position = cannonballSpawn.position;
	        cannonball.transform.rotation = cannonballSpawn.transform.rotation;
            Vector3 force = new Vector3(power * Mathf.Cos(Mathf.Deg2Rad*angle), power * Mathf.Sin(Mathf.Deg2Rad*angle), 0);
	        cannonball.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
            Vector3 newRot = new Vector3(cannonPivot.rotation.eulerAngles.x, 0f, angle - 90f);
            cannonPivot.rotation = Quaternion.Euler(newRot);
			AudioManager.Instance.PlayClip (AudioManager.GameAudioClip.PLAYER_FIRE);
		}
    }
}
