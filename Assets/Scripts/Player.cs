using UnityEngine;

public class Player : MonoBehaviour {

    // player public variables
    public float mouseSensitivity = .1f;
    public float accel = 15f;
    public float maxSpeed = 8f;

    // camera variables
    private Transform cam;
    private float yaw = 0f;
    private float pitch = 0f;
    private float camDistance = 8f;
    private float lyaw = 0f;
    private float lpitch = 0f;
    private float lcamDistance;
    private bool updateCamera = false;

    private Transform[] bonds;

    private Rigidbody rb;
    public LayerMask atomLayer;

    // Use this for initialization
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        bonds = new Transform[4]; // number of bonds
    }

    void FixedUpdate() {
        // apply player movement
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        rb.AddForce((transform.forward * y + transform.right * x) * accel);

        // clamp velocity to maxspeed
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        updateCamera = true;

        // attach to nearby atoms if you can
        Collider[] nearbyAtoms = Physics.OverlapSphere(transform.position, 1f, atomLayer);
        for (int c = 0; c < nearbyAtoms.Length; c++) {
            if (nearbyAtoms[c].transform.parent == transform || nearbyAtoms[c].transform == transform) {
                continue;
            }
            for (int i = 0; i < bonds.Length; i++) {
                if (bonds[i] == null) {
                    bonds[i] = nearbyAtoms[c].transform;
                    Destroy(bonds[i].GetComponent<Rigidbody>());
                    bonds[i].parent = transform;
                    break;
                }
            }
        }
    }

    void Update() {
        //Pathfinder.player = transform;

        // for each of your bonds lerp them towards their correct positions
        for (int i = 0; i < bonds.Length; i++) {
            if (!bonds[i]) {
                continue;
            }
            Vector3 localP = Vector3.up;
            switch (i) {
                case 0:
                    localP = Vector3.right;
                    break;
                case 1:
                    localP = Vector3.left;
                    break;
                case 2:
                    localP = Vector3.back;
                    break;
                case 3:
                    localP = Vector3.forward;
                    break;
            }

            bonds[i].localPosition = Vector3.Lerp(bonds[i].localPosition, localP * 1.25f, Time.deltaTime * 5f);

            //if (i == bonds.Length - 1) {
            //    Pathfinder.player = null;
            //}
        }


        // blow up nearby atoms
        if (Input.GetKeyDown(KeyCode.F)) {
            DetachConnectedAtoms();
            Collider[] cols = Physics.OverlapSphere(transform.position, 15f);
            for (int i = 0; i < cols.Length; i++) {
                FindPlayer fp = cols[i].GetComponent<FindPlayer>();
                if (fp && fp.rb) {
                    fp.rb.AddExplosionForce(40f, transform.position, 15f, 3f);
                    fp.stopUpdate = Time.time + 2f;
                }
            }

            for (int i = 0; i < bonds.Length; i++) {
                if (bonds[i]) {
                    Destroy(bonds[i].gameObject);
                }
                bonds[i] = null;
            }
        }

        // toggle attraction
        if (Input.GetKeyDown(KeyCode.Q)) {
            if (Pathfinder.player == null) {
                Pathfinder.player = transform;
            } else {
                DetachConnectedAtoms();
                Pathfinder.player = null;
            }
        }

        // fixes camera jitter by only updating after physics is processed
        if (!updateCamera) {
            return;
        }
        updateCamera = false;

        // get inputs for camera values
        yaw -= Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        pitch += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, 0.1f, Mathf.PI / 2f);
        camDistance -= Input.GetAxisRaw("Mouse ScrollWheel") * 15f;
        camDistance = Mathf.Clamp(camDistance, 2f, 20f);

        // lerp camera values for smooth movement
        lyaw = Mathf.Lerp(lyaw, yaw, .3f);
        lpitch = Mathf.Lerp(lpitch, pitch, .3f);
        lcamDistance = Mathf.Lerp(lcamDistance, camDistance, .1f);

        // determine cameras position and rotation
        Vector3 unit = new Vector3(Mathf.Cos(lyaw) * Mathf.Sin(lpitch), Mathf.Cos(lpitch), Mathf.Sin(lyaw) * Mathf.Sin(lpitch));
        unit = Vector3.Scale(unit, Vector3.one * lcamDistance);
        cam.position = unit + transform.position;
        cam.LookAt(transform.position);

        // make transform point in direction of camera
        Vector3 forward = cam.TransformDirection(Vector3.forward);
        forward.y = 0f;
        transform.rotation = Quaternion.LookRotation(forward);
    }

    private void DetachConnectedAtoms() {
        for (int i = 0; i < bonds.Length; i++) {
            if (bonds[i]) {
                bonds[i].parent = null;
                bonds[i].GetComponent<FindPlayer>().AddRigidBody();
                bonds[i] = null;
            }
        }
    }
}
