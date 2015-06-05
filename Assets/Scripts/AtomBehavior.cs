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
    public Atom atom { get; set; }

    public bool followPlayer;

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
        if (!rb) {
            gameObject.tag = "AtomBond";
        } else if (stopUpdate > Time.time || !Pathfinder.player) {
            path = Vector3.zero;
            atom.color = Color.red;
            gameObject.tag = "Untagged";
        } else {
            atom.resetColor();
            gameObject.tag = "Atom";
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!rb || !grounded || stopUpdate > Time.time || !shouldPath()) {
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
        if (rb.velocity.sqrMagnitude > atom.maxSpeed * atom.maxSpeed) {
            rb.velocity = rb.velocity.normalized * atom.maxSpeed;
        }
        grounded = false;

        lastX = x;
        lastY = y;
    }

    private bool shouldPath() {
        if (!followPlayer) {
            return false;
        }

        Ray r = new Ray(transform.position, Level.player.transform.position - transform.position);
        RaycastHit info;
        //Debug.DrawRay(transform.position, Level.player.transform.position - transform.position, Color.green, .05f);
        if (Physics.Raycast(r, out info)) {
            return info.collider.CompareTag("Player") || info.collider.CompareTag("AtomBond");
        } else {
            return false;
        }
    }

    void OnCollisionStay(Collision col) {
        if (col.collider.CompareTag("Level")) {
            grounded = true;
        }
    }

}
