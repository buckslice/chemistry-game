using UnityEngine;
using System.Collections;
using System;

public class Exit : MonoBehaviour {

    private float cooldown;
    private ParticleSystem ps;

    // Use this for initialization
    void Start() {
        ps = transform.Find("Model").GetComponent<ParticleSystem>();
    }

    void Update() {
        cooldown -= Time.deltaTime;
        if (cooldown < 0f) {
            ps.startColor = Color.green;
        } else {
            ps.startColor = Color.red;
        }
    }

    void OnTriggerEnter(Collider coll) {
        if (coll.gameObject.tag == "Player") {

            if (Level.instance.player.checkElement()) {
                //Application.OpenURL("http://www.google.com");
                Level.instance.LoadNextLevel();
            } else {
                cooldown = 3.5f;
            }
        }
    }

}
