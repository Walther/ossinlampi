using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public int playerNumber;
    public Rigidbody body;

    public GameObject cannonballPrefab;

    public Transform cannonballSpawn;

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

    public void Fire(float angle, float power)
    {        
        Debug.Log(string.Format("Shooting with angle {0}, power {1}", angle, power));
        GameObject cannonball = Instantiate(cannonballPrefab, cannonballSpawn.position, cannonballSpawn.transform.rotation);
        Vector3 force = new Vector3(0f, power * Mathf.Sin(Mathf.Deg2Rad*angle), power * Mathf.Cos(Mathf.Deg2Rad*angle));
        Debug.Log(force);
        cannonball.GetComponent<Rigidbody>().AddRelativeForce(force, ForceMode.Impulse);
    }
}
