using UnityEngine;

public class Player : MonoBehaviour {

    // player public variables
    public float mouseSensitivity = .1f;
    public float accel = 15f;
    // camera variables
    private Transform cam;
    private float yaw = 0f;
    private float pitch = 0f;
    private float camDistance = 8f;
    private float lyaw = 0f;
    private float lpitch = 0f;
    private float lcamDistance;
    private bool updateCamera = false;
    public int playerWeight = 0;

    private Rigidbody rb;
    public Object bond;
    private Transform[] bondPositions;
    private Bond[] bonds;
    public float bondLength = 1.25f;
    public Atom atom { get; private set; }

    public static string elemStr;

    // Use this for initialization
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        atom = GetComponent<Atom>();
        atom.setElement(Element.CARBON);
        initAtom(atom); // sean
        atom.maxSpeed += 2f; // makes player slightly faster than other atoms
        gameObject.name = "Player";
        playerWeight += atom.weight;

        bondPositions = new Transform[4];
        bonds = new Bond[4];

        for (int i = 0; i < bonds.Length; i++) {
            GameObject go = (GameObject)Instantiate(bond);
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;

            Vector3 ls = go.transform.localScale;
            go.transform.localScale = new Vector3(ls.x, bondLength, ls.z);
            bonds[i] = go.GetComponent<Bond>();
        }
    }

    void FixedUpdate() {
        // apply player movement
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        rb.AddForce((transform.forward * y + transform.right * x) * accel);

        // clamp velocity to maxspeed
        if (rb.velocity.sqrMagnitude > atom.maxSpeed * atom.maxSpeed) {
            rb.velocity = rb.velocity.normalized * atom.maxSpeed;
        }
        updateCamera = true;
    }

    void Update() {
        // for each of your bonds lerp them towards their correct positions
        atom.currentBonds = 0;
        for (int i = 0; i < bondPositions.Length; i++) {
            if (bondPositions[i]) {
                if (!bonds[i].gameObject.activeInHierarchy) {
                    DetachAtom(i, true);
                } else {
                    Vector3 fromPos = bondPositions[i].localPosition;
                    Vector3 toPos = getLocalBondDir(i) * bondLength;
                    bondPositions[i].localPosition = Vector3.Lerp(fromPos, toPos, Time.deltaTime * 5f);
                    bonds[i].transform.localEulerAngles = new Vector3(90f, i * 90f, 0f);
                    atom.currentBonds += bonds[i].numberBonds;
                }
            }
        }

        // blow up nearby atoms
        if (Input.GetKeyDown(KeyCode.F)) {
            explodeAtoms();
        }

        // toggle attraction
        if (Input.GetKeyDown(KeyCode.Q)) {
            sleepAtoms();
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
        for (int i = 0; i < bondPositions.Length; i++) {
            DetachAtom(i);
        }
    }

    private void DetachAtom(int i, bool stopUpdate = false) {
        if (bondPositions[i]) {
            bondPositions[i].transform.parent = null;
            AtomBehavior ab = bondPositions[i].GetComponent<AtomBehavior>();
            ab.AddRigidBody();
            if (stopUpdate) {
                ab.stopUpdate = Time.time + 2f;
            }
            bondPositions[i] = null;
            bonds[i].disable();
            playerWeight -= ab.atom.weight;

        }
    }

    private void AttachAtom(Transform other) {
        // find closest available bond location
        float minDistSqrd = float.MaxValue;
        int index = -1;
        for (int i = 0; i < bondPositions.Length; i++) {
            if (!bondPositions[i]) {
                Vector3 bondedAtomPos = transform.position + transform.TransformDirection(getLocalBondDir(i)) * bondLength;
                float sqrMag = Vector3.SqrMagnitude(bondedAtomPos - other.position);
                if (sqrMag < minDistSqrd) {
                    minDistSqrd = sqrMag;
                    index = i;
                }
            }
        }

        if (index >= 0) {
            AtomBehavior ab = other.GetComponent<AtomBehavior>();
            Destroy(ab.rb);
            other.parent = transform;
            bondPositions[index] = other;

            bonds[index].enable(atom.element, ab.atom.element);
            playerWeight += ab.atom.weight;						// add weight

        }
    }

    void OnTriggerStay(Collider col) {
        Transform other = col.transform;
        if (other.parent == transform || !other.CompareTag("Atom") || other == transform || !atom.canBond()) {
            return;
        }

        AttachAtom(other);
    }

    public void explodeAtoms() {
        DetachConnectedAtoms();
        Collider[] cols = Physics.OverlapSphere(transform.position, 15f);
        for (int i = 0; i < cols.Length; i++) {
            AtomBehavior ab = cols[i].GetComponent<AtomBehavior>();
            if (ab && ab.rb) {
                ab.rb.AddExplosionForce(40f, transform.position, 15f, 3f);
                ab.stopUpdate = Time.time + 2f;
            }
        }
    }

    public void sleepAtoms() {
        if (Pathfinder.player == null) {
            Pathfinder.player = transform;
        } else {
            DetachConnectedAtoms();
            Pathfinder.player = null;
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

    //Sean - used to set the player element in the string
    private void initAtom(Atom tAtom) {
        Element temp = tAtom.element;
        switch (temp) {
            case Element.CARBON:
                elemStr += "C";
                break;
            case Element.HYDROGEN:
                elemStr += "H";
                break;
            case Element.NITROGEN:
                elemStr += "N";
                break;
            case Element.OXYGEN:
                elemStr += "O";
                break;
        }
    }

    private void checkElement()
    {

    }
}
