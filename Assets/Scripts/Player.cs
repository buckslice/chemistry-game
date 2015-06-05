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

    public static string elementStr = ""; //Sean

    private Rigidbody rb;
    public Object bond;
    private Transform[] bondPositions;
    public int[] bondStrengths;
    private GameObject[] bonds;
    public float bondLength = 1.25f;
    public static Atom atom;					// made it static so that it can be accessed in WeightDoor scripts.

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
        //initialWeight = playerWeight;

        bondPositions = new Transform[4];
        bondStrengths = new int[4];

        bonds = new GameObject[4];

        for (int i = 0; i < bonds.Length; i++) {
            GameObject go = (GameObject)Instantiate(bond);
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;

            Vector3 ls = go.transform.localScale;
            go.transform.localScale = new Vector3(ls.x, bondLength, ls.z);
            go.SetActive(false);

            bonds[i] = go;
            bonds[i].tag = "Bond" + i;
            bondStrengths[i] = 0;
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
        for (int i = 0; i < bondPositions.Length; i++) {
            if (bondPositions[i]) {
                if (!bonds[i].activeInHierarchy) {
                    DetachAtom(i, true);
                } else {
                    Vector3 fromPos = bondPositions[i].localPosition;
                    Vector3 toPos = getLocalBondDir(i) * bondLength;
                    bondPositions[i].localPosition = Vector3.Lerp(fromPos, toPos, Time.deltaTime * 5f);
                    bonds[i].transform.localEulerAngles = new Vector3(90f, i * 90f, 0f);
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

    public int getStrength(int i) {
        return bondStrengths[i];
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
            bonds[i].SetActive(false);

            //Atom atomScript = other.GetComponent<Atom>();			
            //Element e2;
            //e2 = atomScript.element;
            Element e1 = atom.element;
            int strength = bondStrengths[i];
            switch (e1) {
                case Element.HYDROGEN:
                    elementStr += "H"; //Sean
                    atom.currentBonds--;
                    break;
                case Element.CARBON:
                    elementStr += "C"; //Sean
                    switch (strength) {
                        case 413:
                            atom.currentBonds--;
                            break;
                        case 347:
                            atom.currentBonds--;
                            break;
                        case 891:
                            atom.currentBonds -= 3;
                            break;
                        case 745:
                            atom.currentBonds -= 2;
                            break;
                        default: break;
                    }
                    break;
                case Element.NITROGEN:
                    elementStr += "N"; //Sean
                    switch (strength) {
                        case 391:
                            atom.currentBonds--;
                            break;
                        case 891:
                            atom.currentBonds -= 3;
                            break;
                        case 945:
                            atom.currentBonds -= 3;
                            break;
                        case 201:
                            atom.currentBonds--;
                            break;
                        default: break;
                    }
                    break;
                case Element.OXYGEN:
                    elementStr += "O"; //Sean
                    switch (strength) {
                        case 467:
                            atom.currentBonds--;
                            break;
                        case 745:
                            atom.currentBonds -= 2;
                            break;
                        case 201:
                            atom.currentBonds--;
                            break;
                        case 498:
                            atom.currentBonds -= 2;
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }



            //atom.currentBonds--;

            Atom atomScript = ab.GetComponent<Atom>();			// reduce weight
            playerWeight -= atomScript.weight;

        }
    }

    void OnTriggerEnter(Collider col) {
        Transform other = col.transform;
        if (other.parent == transform || !other.CompareTag("Atom") || other == transform || !atom.canBond()) {
            return;
        }

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
            Destroy(other.GetComponent<Rigidbody>());
            other.parent = transform;
            bondPositions[index] = other;

            Atom atomScript = other.GetComponent<Atom>();
            Element e2;
            e2 = atomScript.element;
            Element e1 = atom.element;

            // Bond energies for different bonds - thought of having 2 splitters; one for strong and one for weak bonds.

            switch (e1) {
                case Element.HYDROGEN:
                    switch (e2) {
                        case Element.HYDROGEN:
                            elementStr += "H";
                            bondStrengths[index] = 432;
                            break;
                        case Element.CARBON:
                            bondStrengths[index] = 413;
                            break;
                        case Element.NITROGEN:
                            bondStrengths[index] = 391;
                            break;
                        case Element.OXYGEN:
                            bondStrengths[index] = 467;
                            break;
                        default: break;
                    }
                    atom.currentBonds++;
                    break;
                case Element.CARBON:
                    switch (e2) {
                        case Element.HYDROGEN:
                            bondStrengths[index] = 413;
                            atom.currentBonds++;
                            break;
                        case Element.CARBON:
                            bondStrengths[index] = 347;	// single bond only
                            atom.currentBonds++;
                            break;
                        case Element.NITROGEN:
                            bondStrengths[index] = 891; // triple bond (for cyanide)
                            atom.currentBonds += 3;
                            break;
                        case Element.OXYGEN:
                            bondStrengths[index] = 745; // single bond is 358
                            atom.currentBonds += 2;
                            break;
                        default: break;
                    }
                    break;
                case Element.NITROGEN:
                    switch (e2) {
                        case Element.HYDROGEN:
                            bondStrengths[index] = 391;
                            atom.currentBonds++;
                            break;
                        case Element.CARBON:
                            bondStrengths[index] = 891; // triple bond (for cyanide)
                            atom.currentBonds += 3;
                            break;
                        case Element.NITROGEN:
                            bondStrengths[index] = 945; // single bond is 160
                            atom.currentBonds += 3;
                            break;
                        case Element.OXYGEN:
                            bondStrengths[index] = 201;
                            atom.currentBonds++;
                            break;
                        default: break;
                    }
                    break;
                case Element.OXYGEN:
                    switch (e2) {
                        case Element.HYDROGEN:
                            bondStrengths[index] = 467;
                            atom.currentBonds++;
                            break;
                        case Element.CARBON:
                            bondStrengths[index] = 745; // single bond is 358
                            atom.currentBonds += 2;
                            break;
                        case Element.NITROGEN:
                            bondStrengths[index] = 201;
                            atom.currentBonds++;
                            break;
                        case Element.OXYGEN:
                            bondStrengths[index] = 498; // single bond is 204
                            atom.currentBonds += 2;
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }


            bonds[index].SetActive(true);
            //atom.currentBonds++;

            playerWeight += atomScript.weight;						// add weight

        }
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
    private void initAtom(Atom tAtom)
    {
        Element temp = tAtom.element;
        switch (temp)
        {
            case Element.CARBON: 
                elementStr += "C";
                break;
            case Element.HYDROGEN:
                elementStr += "H";
                break;
            case Element.NITROGEN:
                elementStr += "N";
                break;
            case Element.OXYGEN:
                elementStr += "O";
                break;
        }

        

            
    }
}
