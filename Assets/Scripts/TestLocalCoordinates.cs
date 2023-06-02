using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLocalCoordinates : MonoBehaviour {
    // public Transform target;

    void Start() {
        Debug.Log(transform.localPosition);
        Debug.Log(transform.position); 
        /*Camera camera = GetComponent<Camera>();
        Vector3 viewPos = camera.WorldToViewportPoint(target.position);
        Debug.Log(viewPos);*/
        
    }
}
