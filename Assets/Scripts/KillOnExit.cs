using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnExit : MonoBehaviour
{
    void OnTriggerExit(Collider other) {
        // Destroy everything that leaves the trigger
		other.gameObject.SetActive (false);
    }
}
