using UnityEngine;
using System.Collections;
using System;

public class Exit : MonoBehaviour {

    //thought I would put a light to show where the exit is. 
    //it would turn red if its the wrong element and turn green for correct element
    //and the element will move up to the light and onto the next level

    //private string corStr = "CH";
    //private string playStr;
       
	private Level l;

	void Start()
	{
		l = GameObject.Find("Level").GetComponent<Level>();
	}

    void OnTriggerEnter(Collider coll)
    {
		if (coll.gameObject.tag == "Player")
        {
            //playStr = Player.elemStr;
            if (Player.corElem)
            {
                //light turns green
                //goto next level
                Debug.Log("right");
				Player player = coll.gameObject.GetComponent<Player>();
				player.DetachConnectedAtoms();
				//Application.OpenURL("http://www.google.com");
				l.LoadNextLevel();
				Destroy(this);
            }
            else
            {
                //light turns red
                //error for wrong choice
                Debug.Log("wrong");
            }
        }
    }


   

}
