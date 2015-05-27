using UnityEngine;
using System.Collections;

// makes this game objects forward vector point at camera position
public class FaceCamera : MonoBehaviour {

    private Transform mainCam;

    // Use this for initialization
    void Start() {
        mainCam = Camera.main.transform;
    }

    // Update is called once per frame
    void FixedUpdate() {
        // look at cameras position
        //transform.rotation = Quaternion.LookRotation(mainCam.position - transform.position);

        // look in opposite direction of camera
        transform.rotation = Quaternion.LookRotation(-mainCam.forward);
    }
}
