using UnityEngine;
using System.Collections;

public class Textures : MonoBehaviour {
    public static Texture hydrogen;
    public static Texture carbon;
    public static Texture nitrogen;
    public static Texture oxygen;

    public static void load() {
        hydrogen = (Texture)Resources.Load("H");
        carbon = (Texture)Resources.Load("C");
        nitrogen = (Texture)Resources.Load("N");
        oxygen = (Texture)Resources.Load("O");
    }
}
