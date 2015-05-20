using UnityEngine;
using System.Collections;

public class FindPlayer : MonoBehaviour {

    public Rigidbody rb { get; set; }
    public float stopUpdate { get; set; }
    private MeshRenderer mr;
    private MaterialPropertyBlock mpb;
    private bool grounded = false;
    private Vector3 path = Vector3.zero;
    private float idle;
    private float timeSincePath;

    private int x, y, lastX, lastY;

    // Use this for initialization
    void Start() {
        mr = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
        mpb = new MaterialPropertyBlock();
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
        if (!rb) {
            mpb.SetColor("_Color", Color.blue);
        } else if (stopUpdate > Time.time || !Pathfinder.player) {
            mpb.SetColor("_Color", Color.red);
        } else {
            mpb.SetColor("_Color", Color.yellow);
            gameObject.layer = 8;   // atom layer
        }

        mr.SetPropertyBlock(mpb);
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!rb || !grounded || stopUpdate > Time.time) {
            path = Vector3.zero;
            return;
        }

        x = (int)(transform.position.x / LevelLoader.tileSize);
        y = (int)(transform.position.z / LevelLoader.tileSize);

        if ((x != lastX || y != lastY || path == Vector3.zero) && timeSincePath < Time.time) {
            if (!Pathfinder.instance.InsideLevel(transform.position.x, transform.position.z)) {
                Pathfinder.instance.enemies--;
                Destroy(gameObject);
                return;
            }

            path = Pathfinder.instance.GetPath(transform.position.x, transform.position.z);
            idle = path == Vector3.zero ? idle : Time.time + 5f;
            timeSincePath = Time.time + .1f;

            if (idle < Time.time && Pathfinder.player) {
                Pathfinder.instance.enemies--;
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
        if (col.collider.tag == "Level") {
            grounded = true;
        }
    }

}
