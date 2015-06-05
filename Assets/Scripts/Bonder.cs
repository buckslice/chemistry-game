using UnityEngine;
using System.Collections;

public class Bonder : MonoBehaviour {

    void Start() {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", Color.green);
        transform.Find("Model").GetComponent<MeshRenderer>().SetPropertyBlock(mpb);
    }

    void OnTriggerEnter(Collider col) {
        if (col.CompareTag("Bond")) {
            col.GetComponent<Bond>().increment();
        }
    }
}
