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
                
				if(Level.currentLevel == 1)
					Application.OpenURL("https://www.youtube.com/watch?v=HvVUtpdK7xw");
				if(Level.currentLevel == 2)
					Application.OpenURL("https://www.youtube.com/watch?v=txkRCIPSsjM");
				if(Level.currentLevel == 3)
					Application.OpenURL("https://www.youtube.com/watch?v=CiP-xnOavEs");
				if(Level.currentLevel == 4)
					Application.OpenURL("https://www.youtube.com/watch?v=zbUPjHHml1E");
				if(Level.currentLevel == 5)
					Application.OpenURL("https://www.youtube.com/watch?v=FaMWxLCGY0U");
				if(Level.currentLevel == 6)
					Application.OpenURL("https://www.youtube.com/watch?v=uCwHzTsx5yY");

                Level.instance.LoadNextLevel();
                Level.instance.player.source.PlayOneShot(Level.instance.player.exitSound, .7f);
            } else {
                cooldown = 3.5f;
            }
        }
    }

}
