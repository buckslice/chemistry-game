using UnityEngine;
using System.Collections;

public class WeightDoor2 : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerEnter(Collider col) {
		if (col.CompareTag("Player") || col.gameObject.CompareTag("AtomBond")) {	// was initially col.CompareTag 
																					// that didn't work, nor does this
			if(Player.atom.weight > 29)
			{
				Debug.Log ("Hey");				// this works when Player hits it.
			}
			
		}
	}
}
