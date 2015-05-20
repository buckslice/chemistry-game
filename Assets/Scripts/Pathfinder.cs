using UnityEngine;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour {

    public static Pathfinder instance { get; private set; }

    public static Transform player;
    private int pX, pY; // which tile the player is in

    private int[][] tiles;
    private int[][] paths;

    private Queue<Node> frontier;
    private int lastX;
    private int lastY;
    private int pathsGenerated = 0;
    private int greatestCost = 0;
    private float tS;

    public Object enemyPrefab;
    public int enemies = 0;
    private float spawnTime = 0f;

    void Awake() {
        instance = this;
    }

    // Use this for initialization
    void Start() {
        tS = LevelLoader.tileSize;
        player = GameObject.Find("Player").transform;
        frontier = new Queue<Node>();
    }

    // Update is called once per frame
    void Update() {
        if (enemies < 100 && (Input.GetKeyDown(KeyCode.R) || spawnTime < Time.time)) {
            int xe, ye;
            do {
                xe = Random.Range(0, tiles.Length);
                ye = Random.Range(0, tiles[0].Length);
            } while (paths[xe][ye] < 0);

            Vector3 spawn = new Vector3((xe + .5f) * tS, .5f, (ye + .5f) * tS);
            Instantiate(enemyPrefab, spawn, Quaternion.identity);
            spawnTime = Time.time + .5f;
            enemies++;
        }

        if (!player) {
            return;
        }

        pX = (int)(player.position.x / tS);
        pY = (int)(player.position.z / tS);
        // only generate path if player has changed tile position
        if (pX != lastX || pY != lastY) {
            GeneratePath(pX, pY);
            pathsGenerated++;
            //Debug.Log(tiles[x][y] + " " + x + " " + y + " " + pathsGenerated);
        }
        lastX = pX;
        lastY = pY;
    }

    public Vector3 GetPath(float xPos, float yPos) {
        int x = (int)(xPos / tS);
        int y = (int)(yPos / tS);

        Vector3 dir = Vector3.zero;
        if (!IsWalkable(x, y) || paths[x][y] < 0 || !player) {
            return dir;
        }
        if (x == pX && y == pY) {
            return Vector3.down;
        }

        int shortest = paths[x][y];

        if (IsWalkable(x + 1, y) && paths[x + 1][y] < shortest) {
            shortest = paths[x + 1][y];
            dir = getRandomPointInTile(x + 1, y);
        }
        if (IsWalkable(x - 1, y) && paths[x - 1][y] < shortest) {
            shortest = paths[x - 1][y];
            dir = getRandomPointInTile(x - 1, y);
        }
        if (IsWalkable(x, y + 1) && paths[x][y + 1] < shortest) {
            shortest = paths[x][y + 1];
            dir = getRandomPointInTile(x, y + 1);
        }
        if (IsWalkable(x, y - 1) && paths[x][y - 1] < shortest) {
            shortest = paths[x][y - 1];
            dir = getRandomPointInTile(x, y - 1);
        }
        dir -= new Vector3(xPos, 0f, yPos);
        return dir.normalized;
    }

    private Vector3 getRandomPointInTile(int x, int y) {
        if (!InsideLevel(x, y)) {
            return new Vector3(x * tS, 0f, y * tS);
        }
        return new Vector3(x * tS + Random.value * tS, 0f, y * tS + Random.value * tS);
    }

    private void GeneratePath(int x, int y) {
        // clear path
        for (int i = 0; i < paths.Length; i++) {
            for (int j = 0; j < paths[i].Length; j++) {
                paths[i][j] = -1;
            }
        }

        frontier.Clear();
        frontier.Enqueue(new Node(x, y));
        paths[x][y] = 0;
        while (frontier.Count > 0) {
            Node n = frontier.Dequeue();
            greatestCost = Mathf.Max(greatestCost, paths[n.x][n.y]);
            // right neigbor
            if (IsWalkable(n.x + 1, n.y) && paths[n.x + 1][n.y] < 0) {
                frontier.Enqueue(new Node(n.x + 1, n.y));
                paths[n.x + 1][n.y] = paths[n.x][n.y] + 1;
            }
            // left neighbor
            if (IsWalkable(n.x - 1, n.y) && paths[n.x - 1][n.y] < 0) {
                frontier.Enqueue(new Node(n.x - 1, n.y));
                paths[n.x - 1][n.y] = paths[n.x][n.y] + 1;
            }
            // front neighbor
            if (IsWalkable(n.x, n.y + 1) && paths[n.x][n.y + 1] < 0) {
                frontier.Enqueue(new Node(n.x, n.y + 1));
                paths[n.x][n.y + 1] = paths[n.x][n.y] + 1;
            }
            // back neighbor
            if (IsWalkable(n.x, n.y - 1) && paths[n.x][n.y - 1] < 0) {
                frontier.Enqueue(new Node(n.x, n.y - 1));
                paths[n.x][n.y - 1] = paths[n.x][n.y] + 1;
            }
        }
    }

    private struct Node {
        public int x;
        public int y;

        public Node(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }

    private bool IsWalkable(int x, int y) {
        // if inside level, on a walkable tile
        return InsideLevel(x, y) && tiles[x][y] == 0;
    }

    public bool InsideLevel(int x, int y) {
        return x >= 0 && x < tiles.Length && y >= 0 && y < tiles[x].Length;
    }

    public bool InsideLevel(float x, float y) {
        return x >= 0f && x <= tiles.Length * tS && y >= 0f && y <= tiles[0].Length * tS;
    }

    public Vector3 ClampInside(Vector3 v) {
        return new Vector3(Mathf.Clamp(v.x, 0f, tiles.Length * tS), v.y, Mathf.Clamp(v.z, 0f, tiles[0].Length * tS));
    }

    // set and build arrays
    public void SetTiles(int[][] t) {
        tiles = new int[t.Length][];
        paths = new int[t.Length][];
        for (int x = 0; x < tiles.Length; x++) {
            tiles[x] = new int[t[x].Length];
            paths[x] = new int[t[x].Length];
            for (int y = 0; y < tiles[x].Length; y++) {
                tiles[x][y] = t[x][y];
            }
        }
    }

    // to visualize path distance
    void OnDrawGizmos() {
        if (paths == null) {
            return;
        }
        for (int x = 0; x < paths.Length; x++) {
            for (int y = 0; y < paths[x].Length; y++) {
                float c = paths[x][y];
                if (c >= 0) {
                    Gizmos.color = new Color(1f - c / greatestCost, 0f, c / greatestCost);
                    if (c == 0) {
                        Gizmos.color = Color.yellow;
                    }
                    float maxH = 5f;
                    float height = maxH - c / greatestCost * maxH;
                    Gizmos.DrawCube(new Vector3((x + .5f) * tS, height / 2f, (y + .5f) * tS), new Vector3(.5f, height, .5f));
                }
            }
        }
    }
}
