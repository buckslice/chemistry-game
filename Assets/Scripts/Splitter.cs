using UnityEngine;
using System.Collections;

public class Splitter : MonoBehaviour {

	public bool isWeak;

	void Start()
	{
		MaterialPropertyBlock mpb = new MaterialPropertyBlock ();
		mpb.SetColor ("_Color", isWeak ? Color.blue : Color.red);
		transform.Find ("Model").GetComponent<MeshRenderer> ().SetPropertyBlock (mpb);
	}

    void OnTriggerEnter(Collider col) {
		if (col.CompareTag("Bond0") || col.CompareTag("Bond1") || col.CompareTag("Bond2") || col.CompareTag("Bond3")) {
            
			// How to figure out which bond is being split here ? It needs to get the bond strength.
			GameObject b = col.gameObject;
			Player p = col.transform.parent.GetComponent<Player>();
			int index = -1;
			if(b.CompareTag("Bond0"))
				index = 0;
			else if(b.CompareTag("Bond1"))
				index = 1;
			else if(b.CompareTag("Bond2"))
				index = 2;
			else if(b.CompareTag("Bond3"))
				index = 3;

			int strength = p.getStrength(index);

			Debug.Log (strength);

			if((isWeak && strength < 475) || (!isWeak && strength >= 475))
				col.gameObject.SetActive(false);

        }
    }
}
