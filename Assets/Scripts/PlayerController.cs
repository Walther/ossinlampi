using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class PlayerController : MonoBehaviour {

    public int playerNumber;
    public Rigidbody body;

    public GameObject cannonballPrefab;

    public Transform cannonballSpawn;

    private void Start()
    {
        VoiceController.Instance.AboveThresholdStream.Throttle(System.TimeSpan.FromMilliseconds(100)).Subscribe(voiceEvent =>
            {
                Fire(voiceEvent);
            });
    }

	void Update () 
    {
        float control = Input.GetAxis(string.Format("P{0} Horizontal", playerNumber));
            
        body.AddRelativeForce(control*transform.forward);


        if (Input.GetButtonDown(string.Format("P{0} Fire", playerNumber)))
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
        float minFreq = 200f;
        float maxFreq = 700f;
//        Debug.Log(string.Format("freq: {0}, vol: {1}", voiceEvent.frequency, voiceEvent.volume));
        float clamped = Mathf.Clamp(voiceEvent.frequency, 200f, 700f);

        Fire((clamped - 200f), 100000f*voiceEvent.volume);
    }

    private void Fire(float angle, float power)
    {        
//        Debug.Log(string.Format("Shooting with angle {0}, power {1}", angle, power));
        GameObject cannonball = Instantiate(cannonballPrefab, cannonballSpawn.position, cannonballSpawn.transform.rotation);
        Vector3 force = new Vector3(0f, power * Mathf.Sin(Mathf.Deg2Rad*angle), power * Mathf.Cos(Mathf.Deg2Rad*angle));
//        Debug.Log(force);
        cannonball.GetComponent<Rigidbody>().AddRelativeForce(force, ForceMode.Impulse);
    }
}
