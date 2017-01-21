using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour
{

    public Rigidbody body;

    public GameObject cannonballPrefab;

    public Transform cannonballSpawn;

    public float controlForce = 4f;

    private void Start()
    {
        VoiceController.Instance.AboveThresholdStream.Throttle(System.TimeSpan.FromMilliseconds(100)).Subscribe(voiceEvent =>
            {
                Fire(voiceEvent);
            });
    }

	void Update () 
    {
        
        float control = controlForce * CrossPlatformInputManager.GetAxis("Horizontal");
//        Debug.Log(string.Format("h {0}, v {1}", CrossPlatformInputManager.GetAxis("Horizontal"), CrossPlatformInputManager.GetAxis("Vertical")));
//        Debug.Log(string.Format("CPIM: {0}, regular: {1}", control, Input.GetAxis("Horizontal")));    
        body.AddForce(control*Vector3.right);

        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
	}

    private void Fire()
    {
        Debug.Log("Fire!");
        Fire(Random.Range(0f, 90f), Random.Range(5f, 30f));
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
        Debug.Log(string.Format("Shooting with angle {0}, power {1}", angle, power));
        GameObject cannonball = Instantiate(cannonballPrefab, cannonballSpawn.position, cannonballSpawn.transform.rotation);
        Vector3 force = new Vector3(0f, power * Mathf.Sin(Mathf.Deg2Rad*angle), power * Mathf.Cos(Mathf.Deg2Rad*angle));
//        Debug.Log(force);
        cannonball.GetComponent<Rigidbody>().AddRelativeForce(force, ForceMode.Impulse);
    }
}
