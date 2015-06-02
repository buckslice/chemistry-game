using UnityEngine;
using System.Collections;

public class AtomBehavior : MonoBehaviour {

    public Rigidbody rb { get; set; }
    public float stopUpdate { get; set; }
    private bool grounded = false;
    private Vector3 path = Vector3.zero;
    private float idle;
    private float timeSincePath;
    private int x, y, lastX, lastY;
    private Atom atom;

    // Use this for initialization
    void Awake() {
        rb = GetComponent<Rigidbody>();
        atom = GetComponent<Atom>();
        atom.setElement(Element.HYDROGEN);//default
        rb.freezeRotation = true;
    }

    public void AddRigidBody() {
        if (!rb) {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.mass = .1f;
        }
    }

    void Update() {
        gameObject.layer = 0;
        gameObject.tag = "Atom";
        if (!rb) {
            //atom.color = Color.blue;
        } else if (stopUpdate > Time.time || !Pathfinder.player) {
            path = Vector3.zero;
            atom.color = Color.red;
            gameObject.tag = "Untagged";
        } else {
            atom.resetColor();
            gameObject.layer = 8;   // atom layer
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!rb || !grounded || stopUpdate > Time.time) {
            path = Vector3.zero;
            return;
        }

        x = (int)(transform.position.x / Level.tileSize);
        y = (int)(transform.position.z / Level.tileSize);

        if ((x != lastX || y != lastY || path == Vector3.zero) && timeSincePath < Time.time) {
            if (!Pathfinder.instance.insideLevel(transform.position.x, transform.position.z)) {
                Level.atoms--;
                Destroy(gameObject);
                return;
            }

            path = Pathfinder.instance.getPath(transform.position.x, transform.position.z);
            idle = path == Vector3.zero ? idle : Time.time + 5f;
            timeSincePath = Time.time + .1f;

            if (idle < Time.time && Pathfinder.player) {
                Level.atoms--;
                Destroy(gameObject);
                return;
            }
        }

        rb.AddForce(path * 10f);
        // clamp velocity to maxspeed
        float maxSpeed = 5f;
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        grounded = false;

        lastX = x;
        lastY = y;
    }

    void OnCollisionStay(Collision col) {
        if (col.collider.CompareTag("Level")) {
            grounded = true;
        }
    }

}
