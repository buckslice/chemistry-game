using UnityEngine;
using System.Collections;
using System;

public class Exit : MonoBehaviour {

    //thought I would put a light to show where the exit is. 
    //it would turn red if its the wrong element and turn green for correct element
    //and the element will move up to the light and onto the next level

    void OnTriggerEnter(Collider coll) {
        if (coll.gameObject.tag == "Player") {

            if (Level.instance.player.checkElement()) {
                //light turns green
                //goto next level
                //Application.OpenURL("http://www.google.com");
                Debug.Log("right");
                Level.instance.LoadNextLevel();
            } else {
                //light turns red
                //error for wrong choice
                Debug.Log("wrong");
            }
        }
    }

}
