using UnityEngine;
using System.Collections;

public class WeightDoor : MonoBehaviour {

    public bool isLess;
    private Player playerScript;
    void Start() {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetColor("_Color", isLess ? Color.blue : Color.red);
        transform.Find("Model").GetComponent<MeshRenderer>().SetPropertyBlock(mpb);
    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("AtomBond")) {

            if (col.gameObject.CompareTag("Player")) {
                playerScript = col.gameObject.GetComponent<Player>();
            } else {
                playerScript = col.transform.parent.gameObject.GetComponent<Player>();
            }

            if ((isLess && playerScript.playerWeight < 29) || (!isLess && playerScript.playerWeight >= 29)) {
                gameObject.SetActive(false);
            }

        }
    }
}
