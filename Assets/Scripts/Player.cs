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
    public int totalPlayerWeight = 0;

    private Rigidbody rb;
    public Object bond;
    private AtomBehavior[] bondedAtoms;
    private Bond[] bonds;
    public float bondLength = 1.25f;
    public Atom atom { get; private set; }

    private TargetMoleculeIndicator tmi;

    public AudioClip explodeSound;
    public AudioClip bondSound;
    public AudioClip splitSound;
    public AudioClip exitSound;
    public AudioSource source { get; set; }

    // Use this for initialization
    void Awake() {
        //transform.position = start;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        source = GetComponent<AudioSource>();
        cam = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        bondedAtoms = new AtomBehavior[4];
        bonds = new Bond[4];

        for (int i = 0; i < bonds.Length; i++) {
            GameObject go = (GameObject)Instantiate(bond);
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;

            Vector3 ls = go.transform.localScale;
            go.transform.localScale = new Vector3(ls.x, bondLength, ls.z);
            bonds[i] = go.GetComponent<Bond>();
        }
        tmi = GameObject.Find("Target").GetComponent<TargetMoleculeIndicator>();
    }

    public void ResetPlayer() {
        atom = GetComponent<Atom>();

        if (Level.currentLevel == 1 || Level.currentLevel == 6) {
            atom.setElement(Element.NITROGEN);
        } else if (Level.currentLevel == 2 || Level.currentLevel == 4 || Level.currentLevel == 5) {
            atom.setElement(Element.CARBON);
        } else if (Level.currentLevel == 3) {
            atom.setElement(Element.HYDROGEN);
        }
        atom.maxSpeed += 1f; // makes player slightly faster than other atoms
        totalPlayerWeight = atom.weight;
        gameObject.name = "Player";

        for (int i = 0; i < 4; i++) {
            if (bondedAtoms[i]) {
                Destroy(bondedAtoms[i].gameObject);
            }
            bondedAtoms[i] = null;
            bonds[i].disable();
        }
        tmi.setup();
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
        for (int i = 0; i < bondedAtoms.Length; i++) {
            if (bondedAtoms[i]) {
                if (!bonds[i].gameObject.activeInHierarchy) {
                    DetachAtom(i, true);
                } else {
                    Vector3 fromPos = bondedAtoms[i].transform.localPosition;
                    Vector3 toPos = getLocalBondDir(i) * bondLength;
                    bondedAtoms[i].transform.localPosition = Vector3.Lerp(fromPos, toPos, Time.deltaTime * 5f);
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

    public void DetachConnectedAtoms() {
        for (int i = 0; i < bondedAtoms.Length; i++) {
            DetachAtom(i);
        }
    }

    private void DetachAtom(int i, bool stopUpdate = false) {
        if (bondedAtoms[i]) {
            AtomBehavior ab = bondedAtoms[i];
            ab.transform.parent = Level.instance.transform;
            ab.AddRigidBody();
            if (stopUpdate) {
                ab.stopUpdate = Time.time + 2f;
            }
            bondedAtoms[i] = null;
            bonds[i].disable();
            totalPlayerWeight -= ab.atom.weight;

        }
    }

    private void AttachAtom(Transform other) {
        // find closest available bond location
        float minDistSqrd = float.MaxValue;
        int index = -1;
        for (int i = 0; i < bondedAtoms.Length; i++) {
            if (!bondedAtoms[i]) {
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
            bondedAtoms[index] = ab;

            bonds[index].enable(atom.element, ab.atom.element);
            totalPlayerWeight += ab.atom.weight;						// add weight

            source.PlayOneShot(bondSound, .7f);
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
        source.PlayOneShot(explodeSound, .7f);
    }

    public void sleepAtoms() {
        if (Pathfinder.player == null) {
            Pathfinder.player = transform;
        } else {
            DetachConnectedAtoms();
            Pathfinder.player = null;
        }
    }

    public static Vector3 getLocalBondDir(int i) {
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

    public bool checkElement() {
        for (int i = 0; i < 4; i++) {
            int m = Level.levelMolecs[Level.currentLevel - 1][i];
            int b = Level.bondNumbers[Level.currentLevel - 1][i];
            // if no bonded atom in this slot but there should be
            if (!bondedAtoms[i] && m != 0) {
                return false;
            } else if (bondedAtoms[i] && (int)bondedAtoms[i].atom.element != m - 1) {
                return false;
            } else if (bonds[i].numberBonds != b) {
                return false;
            }
        }

        return true;
    }
}
