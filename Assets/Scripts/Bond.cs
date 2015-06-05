using UnityEngine;
using System.Collections;

public class Bond : MonoBehaviour {

    public int strength;// { get; private set; }
    public int numberBonds;// { get; private set; }

    private Element e1;
    private Element e2;

    private float cooldown;

    void Start() {
        disable();
    }

    void Update() {
        float scale;
        switch (numberBonds) {
            case 1:
                scale = .2f;
                break;
            case 2:
                scale = .4f;
                break;
            case 3:
                scale = .6f;
                break;
            default:
                scale = .2f;
                break;
        }

        float y = transform.localScale.y;
        transform.localScale = new Vector3(scale, y, scale);
    }

    public void enable(Element e1, Element e2) {
        gameObject.SetActive(true);
        this.e1 = e1;
        this.e2 = e2;
        numberBonds = 1;
        strength = Atom.getBondStrength(numberBonds, e1, e2);
    }

    public void increment() {
        if (cooldown > Time.time || !Level.instance.player.atom.canBond()) {
            return;
        }
        int s = Atom.getBondStrength(numberBonds + 1, e1, e2);
        if (s == 0) {
            return; // if can't bond anymore then ignore
        } else {
            numberBonds++;
            strength = s;
        }
        cooldown = Time.time + .5f;
        Level.instance.player.source.PlayOneShot(Level.instance.player.bondSound, .7f);
    }

    public void decrement() {
        if (cooldown > Time.time) {
            return;
        }
        if (numberBonds <= 1) {
            disable();
        } else {
            strength = Atom.getBondStrength(--numberBonds, e1, e2);
        }
        cooldown = Time.time + .5f;
        Level.instance.player.source.PlayOneShot(Level.instance.player.splitSound, .7f);
    }

    public void disable() {
        strength = 0;
        numberBonds = 0;
        gameObject.SetActive(false);
    }
}
