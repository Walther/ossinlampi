using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInactiveAfter : MonoBehaviour {

    public float time = 10f;

    private float startTime;
	
	void OnEnable () {
        startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time - startTime > time)
        {
            gameObject.SetActive(false);
        }
	}
}
