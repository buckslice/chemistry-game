using UnityEngine;
using System.Collections;

public class WeightDoor : MonoBehaviour {

	public bool isLess;
	
	void Start()
	{
		MaterialPropertyBlock mpb = new MaterialPropertyBlock ();
		mpb.SetColor ("_Color", isLess ? Color.blue : Color.red);
		transform.Find ("Model").GetComponent<MeshRenderer> ().SetPropertyBlock (mpb);
	}

	void OnCollisionEnter(Collision col) {
		if (col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("AtomBond")) {	
																		
			if((isLess && Player.atom.weight < 29) || (!isLess && Player.atom.weight > 29))
			{
				gameObject.SetActive(false);
			}

		}
	}
}
