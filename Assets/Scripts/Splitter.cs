using UnityEngine;
using System.Collections;

public class Splitter : MonoBehaviour {

    void OnTriggerEnter(Collider col) {
        if (col.CompareTag("Bond")) {
            
			// How to figure out which bond is being split here ? It's needed to get the bond strength.

			col.gameObject.SetActive(false);



        }
    }
}
