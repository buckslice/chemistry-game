using UnityEngine;
using System.Collections;

public class WeightDoor2 : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnTriggerStay(Collider col) {
		if (col.CompareTag("Player") || col.CompareTag("AtomBond")) {
			
			if(Player.atom.weight > 29)
			{
				Debug.Log ("Hey");
			}
			
		}
	}
}
