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
    public int maxBonds { get; private set; }
    public int weight;
    public int currentBonds { get; set; }

    private Color elementColor;
    public Color color { get; set; }
    private MaterialPropertyBlock mpb;
    private MeshRenderer mr;

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
                elementColor = Color.magenta;
                tex = Textures.hydrogen;
                break;
            case Element.CARBON:
                maxBonds = 4;
				weight = 12;
                elementColor = Color.white;
                tex = Textures.carbon;
                break;
            case Element.NITROGEN:
                maxBonds = 3;
				weight = 14;
                elementColor = Color.green;
                tex = Textures.nitrogen;
                break;
            case Element.OXYGEN:
                maxBonds = 2;
				weight = 16;
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

        //transform.localPosition = Vector3.Lerp(transform.localPosition, localPos, Time.time);
    }

    public void resetColor() {
        color = elementColor;
    }

    public bool canBond() {
        return currentBonds < maxBonds;
    }

    //void OnTriggerEnter(Collider col) {
    //    if (col.CompareTag("Atom")) {
    //        player.connectToAtom(col.GetComponent<Atom>(), gridX, gridY);
    //    }
    //}

    //public Vector3 localPos { get; set; }
    //public int gridX;
    //public int gridY;
}
