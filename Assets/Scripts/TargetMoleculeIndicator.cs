using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TargetMoleculeIndicator : MonoBehaviour {

    public Sprite[] elements;

    private Image center;
    private Image[] bondedAtoms;
    private Image[] bonds;

    private Text weightText;

    // Use this for initialization
    void Awake() {
        bondedAtoms = new Image[4];
        bondedAtoms[0] = transform.Find("Top").GetComponent<Image>();
        bondedAtoms[1] = transform.Find("Right").GetComponent<Image>();
        bondedAtoms[2] = transform.Find("Bottom").GetComponent<Image>();
        bondedAtoms[3] = transform.Find("Left").GetComponent<Image>();
        bonds = new Image[4];
        bonds[0] = transform.Find("TopBond").GetComponent<Image>();
        bonds[1] = transform.Find("RightBond").GetComponent<Image>();
        bonds[2] = transform.Find("BottomBond").GetComponent<Image>();
        bonds[3] = transform.Find("LeftBond").GetComponent<Image>();

        center = transform.Find("Center").GetComponent<Image>();
        weightText = GameObject.Find("WeightText").GetComponent<Text>();
    }

    public void setup() {
        center.gameObject.SetActive(true);
        center.sprite = elements[(int)Level.instance.player.atom.element];

        for (int i = 0; i < 4; i++) {
            int m = Level.levelMolecs[Level.currentLevel - 1][i];
            int b = Level.bondNumbers[Level.currentLevel - 1][i];
            if (m == 0) {
                bondedAtoms[i].gameObject.SetActive(false);
            } else {
                bondedAtoms[i].sprite = elements[m - 1];
                bondedAtoms[i].gameObject.SetActive(true);
            }

            if (b == 0) {
                bonds[i].gameObject.SetActive(false);
            } else {
                int w = b == 1 ? 15 : b == 2 ? 30 : 50;
                bool lr = i % 2 == 0;
                float x = lr ? w : 75f;
                float y = lr ? 75f : w;
                bonds[i].rectTransform.sizeDelta = new Vector2(x, y);
                bonds[i].gameObject.SetActive(true);
            }

        }
    }

    void Update() {
        weightText.text = "Current Weight: " + Level.instance.player.totalPlayerWeight.ToString();
    }
}
