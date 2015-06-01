using UnityEngine;

public class Player : MonoBehaviour {

    // player public variables
    public float mouseSensitivity = .1f;
    public float accel = 15f;
    public float maxSpeed = 8f;
    public float bondLength = 1.25f;

    // camera variables
    private Transform cam;
    private float yaw = 0f;
    private float pitch = 0f;
    private float camDistance = 8f;
    private float lyaw = 0f;
    private float lpitch = 0f;
    private float lcamDistance;
    private bool updateCamera = false;

    private Transform[] bondedAtoms;
    private GameObject[] bonds;

    private Rigidbody rb;
    public LayerMask atomLayer;
    public Object bond;

    // Use this for initialization
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        int maxBonds = 4; // for carbon, will change depending on what atom you are
        bondedAtoms = new Transform[maxBonds];
        bonds = new GameObject[maxBonds];

        for (int i = 0; i < bonds.Length; i++) {
            GameObject go = (GameObject)Instantiate(bond);
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;

            Vector3 ls = go.transform.localScale;
            go.transform.localScale = new Vector3(ls.x, bondLength, ls.z);
            go.SetActive(false);

            bonds[i] = go;
        }
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
            Transform nearbyAtom = nearbyAtoms[c].transform;
            if (nearbyAtom.parent == transform || nearbyAtom == transform) {
                continue;
            }
            float minDistSqrd = float.MaxValue;
            int index = -1;
            for (int i = 0; i < bondedAtoms.Length; i++) {
                if (!bondedAtoms[i]) {
                    Vector3 bondedAtomPos = transform.position + transform.TransformDirection(getLocalBondDir(i)) * bondLength;
                    float sqrMag = Vector3.SqrMagnitude(bondedAtomPos - nearbyAtom.position);
                    if (sqrMag < minDistSqrd) {
                        minDistSqrd = sqrMag;
                        index = i;
                    }
                }
            }

            if (index >= 0) {
                bondedAtoms[index] = nearbyAtoms[c].transform;
                Destroy(bondedAtoms[index].GetComponent<Rigidbody>());
                bondedAtoms[index].parent = transform;
                bonds[index].SetActive(true);
            }
        }
    }

    private Vector3 getLocalBondDir(int i) {
        switch (i) {
            case 0:
                return Vector3.forward;
            case 1:
                return Vector3.right;
            case 2:
                return Vector3.back;
            case 3:
                return Vector3.left;
        }
        return Vector3.up;

    }

    void Update() {
        // for each of your bonds lerp them towards their correct positions
        for (int i = 0; i < bondedAtoms.Length; i++) {
            if (!bondedAtoms[i]) {
                continue;
            }
            if (!bonds[i].activeInHierarchy) {
                DetachAtom(i, true);
            } else {
                Vector3 fromPos = bondedAtoms[i].localPosition;
                Vector3 toPos = getLocalBondDir(i) * bondLength;
                bondedAtoms[i].localPosition = Vector3.Lerp(fromPos, toPos, Time.deltaTime * 5f);
                bonds[i].transform.localEulerAngles = new Vector3(90f, i * 90f, 0f);
            }
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
        for (int i = 0; i < bondedAtoms.Length; i++) {
            DetachAtom(i);
        }
    }

    private void DetachAtom(int i, bool stopUpdate = false) {
        if (bondedAtoms[i]) {
            bondedAtoms[i].parent = null;
            FindPlayer fp = bondedAtoms[i].GetComponent<FindPlayer>();
            fp.AddRigidBody();
            if (stopUpdate) {
                fp.stopUpdate = Time.time + 2f;
            }
            bondedAtoms[i] = null;
            bonds[i].SetActive(false);
        }
    }
}
