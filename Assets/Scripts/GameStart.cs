using UnityEngine;
using System.Collections;

public class GameStart : MonoBehaviour {

    private bool started = false;

    // Use this for initialization
    void Awake() {
        Time.timeScale = 0f;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && !started) {
            started = true;
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }
    }
}
