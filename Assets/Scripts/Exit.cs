using UnityEngine;
using System.Collections;
using System;

public class Exit : MonoBehaviour {

    //thought I would put a light to show where the exit is. 
    //it would turn red if its the wrong element and turn green for correct element
    //and the element will move up to the light and onto the next level

    private string corStr = "CH";
     private string playStr;
       
    

    void OnTriggerEnter(Collider coll)
    {
        
        if (coll.gameObject.tag == "Player")
        {
            playStr = Player.elemStr;
            if (checkBond(playStr, corStr))
            {
                //light turns green
                //goto next level
                Debug.Log("right");
            }
            else
            {
                //light turns red
                //error for wrong choice
                Debug.Log("wrong");
            }
        }
    }

    //would get a molecule string from the player script and compare it too the string for that level.
    static bool checkBond(string s1, string s2)
    {
        Debug.Log(s1);
        Debug.Log(s2);
        char[] a  = s1.ToCharArray();
        char[] a2 = s2.ToCharArray();

        Array.Sort(a);
        Array.Sort(a2);

        s1 = new string(a);
        s2 = new string(a2);

        return s1.Equals(s2);
    }

   

}
