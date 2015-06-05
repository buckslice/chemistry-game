using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class Level : MonoBehaviour {

    public Object splitterPrefab;
    public Object bonderPrefab;
    public Object exploderPrefab;
    public Object weightDoorPrefab;
    public Object atomPrefab;
    public Object exitPrefab;

    public static int atoms = 0;
    public int maxAtoms = 100;
    public float spawnInterval = .5f;

    private float spawnTime = 0f;
    private int currentLevel = 1;
    private Mesh mesh;  // current level mesh

    private int[][] tiles;
    private List<int> tris = new List<int>();
    private List<Vector3> verts = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private int triNum = 0;

    public static float tileSize = 2f;

    // tile ids
    public const int GROUND = 0;
    public const int WALL = 1;
    public const int PLAYER_SPAWN = 2;
    public const int EXPLODER = 3;
    public const int WEIGHT_DOOR_LESS = 4;
    public const int WEIGHT_DOOR_MORE = 5;
    public const int SPLITTER_WEAK = 6;
    public const int SPLITTER_STRONG = 7;
    public const int BONDER = 8;
    public const int EXIT = 13; //Sean

    public const int HYDROGEN = 9;
    public const int CARBON = 10;
    public const int NITROGEN = 11;
    public const int OXYGEN = 12;

    public static Player player;

    public static string[] levelStr; //sean
    public static int curLvl = 0;

    // Use this for initialization
    void Awake() {
        levelStr = new string[10]; //change the size for the number of levels here
        levelStr[0] = "CH"; //add the wining codes here
        player = GameObject.Find("Player").GetComponent<Player>();
        Textures.load();
        LoadLevel();
        
    }

    void Update() {
        if (atoms < maxAtoms && (Input.GetKeyDown(KeyCode.R))) { //|| spawnTime < Time.time)) {
            Instantiate(atomPrefab, Pathfinder.instance.getRandomWalkable(), Quaternion.identity);
            spawnTime = Time.time + spawnInterval;
            atoms++;
        }
    }

    public void LoadLevel() {

        // load level texture and get pixel array
        Texture2D tex = (Texture2D)Resources.Load("Levels/level" + currentLevel);
        Color32[] colors = tex.GetPixels32();

        // convert colors at top into tile ids
        Dictionary<Color32, int> table = new Dictionary<Color32, int>();
        int id = 0;
        for (int i = colors.Length - tex.width; true; i++) {
            Color32 color = colors[i];
            if (!table.ContainsKey(color)) {
                table.Add(color, id++);
            } else {
                break;
            }
        }

        int width = tex.width;
        int height = tex.height - 1;

        // initialize 2D array
        tiles = new int[width][];
        for (int i = 0; i < tiles.Length; i++) {
            tiles[i] = new int[height];
        }

        // turn image into 2D tile id array
        for (int i = 0; i < width * height; i++) {
            tiles[i % width][i / width] = table[colors[i]];
        }

        // reset variables
        tris.Clear();
        verts.Clear();
        uvs.Clear();
        triNum = 0;
        if (mesh) {
            Destroy(mesh);
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {

                int h = GetHeight(x, y);

                // add 2 triangles for this tile
                verts.Add(new Vector3(x, h, y));
                verts.Add(new Vector3(x, h, y + 1));
                verts.Add(new Vector3(x + 1, h, y + 1));
                verts.Add(new Vector3(x + 1, h, y + 1));
                verts.Add(new Vector3(x + 1, h, y));
                verts.Add(new Vector3(x, h, y));

                AddUvsAndTris(tiles[x][y]);

                // add walls between low tiles and high tiles
                if (h == 0) {
                    if (GetHeight(x + 1, y) == 1) { // right wall
                        verts.Add(new Vector3(x + 1, 0, y));
                        verts.Add(new Vector3(x + 1, 0, y + 1));
                        verts.Add(new Vector3(x + 1, 1, y + 1));
                        verts.Add(new Vector3(x + 1, 1, y + 1));
                        verts.Add(new Vector3(x + 1, 1, y));
                        verts.Add(new Vector3(x + 1, 0, y));

                        AddUvsAndTris(tiles[x][y]);
                    }

                    if (GetHeight(x - 1, y) == 1) { // left wall
                        verts.Add(new Vector3(x, 0, y + 1));
                        verts.Add(new Vector3(x, 0, y));
                        verts.Add(new Vector3(x, 1, y));
                        verts.Add(new Vector3(x, 1, y));
                        verts.Add(new Vector3(x, 1, y + 1));
                        verts.Add(new Vector3(x, 0, y + 1));

                        AddUvsAndTris(tiles[x][y]);
                    }

                    if (GetHeight(x, y + 1) == 1) { // front wall
                        verts.Add(new Vector3(x + 1, 0, y + 1));
                        verts.Add(new Vector3(x, 0, y + 1));
                        verts.Add(new Vector3(x, 1, y + 1));
                        verts.Add(new Vector3(x, 1, y + 1));
                        verts.Add(new Vector3(x + 1, 1, y + 1));
                        verts.Add(new Vector3(x + 1, 0, y + 1));

                        AddUvsAndTris(tiles[x][y]);
                    }

                    if (GetHeight(x, y - 1) == 1) { // back wall
                        verts.Add(new Vector3(x, 0, y));
                        verts.Add(new Vector3(x + 1, 0, y));
                        verts.Add(new Vector3(x + 1, 1, y));
                        verts.Add(new Vector3(x + 1, 1, y));
                        verts.Add(new Vector3(x, 1, y));
                        verts.Add(new Vector3(x, 0, y));

                        AddUvsAndTris(tiles[x][y]);
                    }
                }

                // switch statement for additional tile logic (like spawning prefabs)
                Vector3 spawn = new Vector3((x + .5f) * tileSize, 0, (y + .5f) * tileSize);
                switch (tiles[x][y]) {
                    case PLAYER_SPAWN:
                        player.transform.position = spawn + Vector3.up;
                        break;
                    case SPLITTER_WEAK:
                        GameObject go = (GameObject)Instantiate(splitterPrefab, spawn, Quaternion.identity);
                        go.GetComponent<Splitter>().isWeak = true;
                        go.transform.parent = transform;
                        tiles[x][y] = GROUND;
                        break;
                    case SPLITTER_STRONG:
                        go = (GameObject)Instantiate(splitterPrefab, spawn, Quaternion.identity);
                        go.GetComponent<Splitter>().isWeak = false;
                        go.transform.parent = transform;
                        tiles[x][y] = GROUND;
                        break;
                    case BONDER:
                        go = (GameObject)Instantiate(bonderPrefab, spawn, Quaternion.identity);
                        go.transform.parent = transform;
                        tiles[x][y] = GROUND;
                        break;
                    case EXPLODER:
                        go = (GameObject)Instantiate(exploderPrefab, spawn, Quaternion.identity);
                        go.transform.parent = transform;
                        tiles[x][y] = GROUND;
                        break;
                    case EXIT:
                        go = (GameObject)Instantiate(exitPrefab, spawn, Quaternion.identity);
                        go.transform.parent = transform;
                        tiles[x][y] = GROUND;
                        break;
                    case HYDROGEN:
                        Atom a = ((GameObject)Instantiate(atomPrefab, spawn + Vector3.up, Quaternion.identity)).GetComponent<Atom>();
                        a.setElement(Element.HYDROGEN);
                        tiles[x][y] = GROUND;
                        break;
                    case CARBON:
                        a = ((GameObject)Instantiate(atomPrefab, spawn + Vector3.up, Quaternion.identity)).GetComponent<Atom>();
                        a.setElement(Element.CARBON);
                        tiles[x][y] = GROUND;
                        break;
                    case NITROGEN:
                        a = ((GameObject)Instantiate(atomPrefab, spawn + Vector3.up, Quaternion.identity)).GetComponent<Atom>();
                        a.setElement(Element.NITROGEN);
                        tiles[x][y] = GROUND;
                        break;
                    case OXYGEN:
                        a = ((GameObject)Instantiate(atomPrefab, spawn + Vector3.up, Quaternion.identity)).GetComponent<Atom>();
                        a.setElement(Element.OXYGEN);
                        tiles[x][y] = GROUND;
                        break;
                    case WEIGHT_DOOR_LESS:
                        GameObject go2 = (GameObject)Instantiate(weightDoorPrefab, spawn, Quaternion.identity);
                        go2.GetComponent<WeightDoor>().isLess = true;
                        go2.transform.parent = transform;
                        tiles[x][y] = GROUND;
                        break;
                    case WEIGHT_DOOR_MORE:
                        go2 = (GameObject)Instantiate(weightDoorPrefab, spawn, Quaternion.identity);
                        go2.GetComponent<WeightDoor>().isLess = false;
                        go2.transform.parent = transform;
                        tiles[x][y] = GROUND;
                        break;
                    default:
                        break;
                }
            }
        }

        // scale map by tileSize
        for (int i = 0; i < verts.Count; i++) {
            verts[i] *= tileSize;
        }

        // build mesh from lists
        mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

        GameObject.Find("Pathfinder").GetComponent<Pathfinder>().setTiles(tiles);
    }

    private void AddUvsAndTris(int index) {
        //Rect r = rects[index];
        Rect r = new Rect(0, 0, 1, 1);

        uvs.Add(new Vector2(r.xMin, r.yMin));
        uvs.Add(new Vector2(r.xMin, r.yMax));
        uvs.Add(new Vector2(r.xMax, r.yMax));
        uvs.Add(new Vector2(r.xMax, r.yMax));
        uvs.Add(new Vector2(r.xMax, r.yMin));
        uvs.Add(new Vector2(r.xMin, r.yMin));

        // add 6 indices
        for (int i = 0; i < 6; i++) {
            tris.Add(triNum++);
        }
    }

    private int GetHeight(int x, int y) {
        if (x < 0 || x >= tiles.Length || y < 0 || y >= tiles[0].Length) {
            return 1;
        }
        switch (tiles[x][y]) {
            case WALL:
                return 1;
            default:
                return 0;
        }
    }
}
