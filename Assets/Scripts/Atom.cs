using UnityEngine;
using System.Collections;

public enum Element {
    HYDROGEN,
    CARBON,
    NITROGEN,
    OXYGEN
};

public class Atom : MonoBehaviour {

    public Element element { get; private set; }
    public int maxBonds;
    public int weight;
    public int currentBonds;
    public float maxSpeed;

    private Color elementColor;
    public Color color { get; set; }
    private MaterialPropertyBlock mpb;
    private MeshRenderer mr;

    public static readonly int[][][] bondStrengths = new int[3][][]{
        new int[4][]{
            new int[]{436,413,391,463}, //H-H H-C H-N H-O
            new int[]{413,348,293,358}, //C-H C-C C-N C-O
            new int[]{391,293,163,201}, //N-H N-C N-N N-O
            new int[]{436,358,201,146}  //O-H O-C O-N O-O
        },
        new int[4][]{
            new int[]{0,  0,  0,  0},   //--- --- --- ---
            new int[]{0,614,615,799},   //--- C=C C=N C=O
            new int[]{0,615,418,607},   //--- N=C N=N N=O
            new int[]{0,799,607,799},   //--- O=C O=N O=O
        },
        new int[4][]{
            new int[]{0,  0,  0,  0},   //--- --- --- ---
            new int[]{0,839,891,  0},   //--- C~C C~N C~O
            new int[]{0,891,941,  0},   //--- N~C N~N ---
            new int[]{0,1072, 0,  0},   //--- O~C --- ---
        },
    };

    public void setElement(Element e) {
        this.element = e;
        mr = transform.Find("Model").GetComponent<MeshRenderer>();
        mpb = new MaterialPropertyBlock();

        gameObject.name = element.ToString();
        Texture tex = Textures.hydrogen;
        switch (e) {
            case Element.HYDROGEN:
                maxBonds = 1;
                weight = 1;
                maxSpeed = 8f;
                elementColor = Color.magenta;
                tex = Textures.hydrogen;
                break;
            case Element.CARBON:
                maxBonds = 4;
                weight = 12;
                maxSpeed = 5f;
                elementColor = Color.white;
                tex = Textures.carbon;
                break;
            case Element.NITROGEN:
                maxBonds = 3;
                weight = 14;
                maxSpeed = 6f;
                elementColor = Color.green;
                tex = Textures.nitrogen;
                break;
            case Element.OXYGEN:
                maxBonds = 2;
                weight = 16;
                maxSpeed = 4f;
                elementColor = Color.blue;
                tex = Textures.oxygen;
                break;
            default: break;
        }
        color = elementColor;
        mpb.SetTexture("_MainTex", tex);
    }

    void Update() {
        mpb.SetColor("_Color", color);
        mr.SetPropertyBlock(mpb);
    }

    public void resetColor() {
        color = elementColor;
    }

    public bool canBond() {
        return currentBonds < maxBonds;
    }

    public static int getBondStrength(int numBonds, Element e1, Element e2) {
        if (numBonds > 3 || numBonds < 1) {
            return 0;
        }
        return bondStrengths[numBonds - 1][(int)e1][(int)e2];
    }
}
