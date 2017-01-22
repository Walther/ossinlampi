using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleExit : MonoBehaviour
{
	[SerializeField]
	private Transform _respawnPoint = null;

    private void OnTriggerExit (Collider other)
	{
		if (other.CompareTag ("Player") || other.CompareTag ("Enemy"))
		{
			// Respawn escaped players/enemies to the play area
			other.transform.position = _respawnPoint.position;
			other.transform.rotation = Quaternion.identity;
		}
		else
		{
        	// Disable everything else that leaves the trigger
			other.gameObject.SetActive (false);
		}
	}
}
