using UnityEngine;
using System.Collections;

public class Splitter : MonoBehaviour {

    public bool isWeak;

    void Start() {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", isWeak ? Color.blue : Color.red);
        transform.Find("Model").GetComponent<MeshRenderer>().SetPropertyBlock(mpb);
    }

    void OnTriggerEnter(Collider col) {
        if (col.CompareTag("Bond")) {
            Bond bond = col.GetComponent<Bond>();

            if ((isWeak && bond.strength < 430) || (!isWeak && bond.strength >= 430)) {
                bond.decrement();
            }
        }
    }
}
