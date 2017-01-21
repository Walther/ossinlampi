using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow3D : MonoBehaviour {

    public Transform Player;   
    public Vector3 Offset;
    public Vector3 LookOffset;

    private Vector3 lookAt;

//    void OnDrawGizmos()
//    {
//        Gizmos.DrawSphere(lookAt, 0.5f);
//    }

	void Update () {
        Transform transform = GetComponent<Transform>();
        transform.position = Player.position + Offset.x * Player.right + Offset.y * Player.forward;
        lookAt = Player.position + LookOffset.x * Player.right; //+ LookOffset.y * Player.up + LookOffset.z * Player.forward;
        transform.rotation = Quaternion.LookRotation(lookAt - transform.position, Vector3.up);
	}
}
