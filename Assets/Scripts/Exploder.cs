using UnityEngine;
using System.Collections;

public class Exploder : MonoBehaviour {

    private float cooldown;
    private ParticleSystem ps;

    // Use this for initialization
    void Start() {
        ps = transform.Find("Model").GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update() {
        cooldown -= Time.deltaTime;
        if (cooldown < 0f) {
            ps.startColor = Color.white;
        } else {
            ps.startColor = Color.black;
        }
    }

    void OnTriggerStay(Collider col) {
        if (cooldown < 0f && col.CompareTag("Player")) {
            Level.player.explodeAtoms();
            cooldown = 5f;
        }
    }
}
