using UnityEngine;
using System.Collections;

public class Splitter : MonoBehaviour {

    void OnTriggerEnter(Collider col) {
        if (col.CompareTag("Bond")) {
            col.gameObject.SetActive(false);
        }
    }
}
