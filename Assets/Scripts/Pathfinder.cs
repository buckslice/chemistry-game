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
    private float ts;

    public bool drawPathData = false;

    void Awake() {
        instance = this;
    }

    // Use this for initialization
    void Start() {
        ts = Level.tileSize;
        player = GameObject.Find("Player").transform;
        frontier = new Queue<Node>();
    }

    // Update is called once per frame
    void Update() {

        if (!player) {
            return;
        }

        pX = (int)(player.position.x / ts);
        pY = (int)(player.position.z / ts);
        // only generate path if player has changed tile position
        if (pX != lastX || pY != lastY) {
            generatePath(pX, pY);
            pathsGenerated++;
            //Debug.Log(tiles[x][y] + " " + x + " " + y + " " + pathsGenerated);
        }
        lastX = pX;
        lastY = pY;
    }

    public Vector3 getPath(float xPos, float yPos) {
        int x = (int)(xPos / ts);
        int y = (int)(yPos / ts);

        Vector3 dir = Vector3.zero;
        if (!isWalkable(x, y) || paths[x][y] < 0 || !player) {
            return dir;
        }
        if (x == pX && y == pY) {
            return Vector3.down;
        }

        int shortest = paths[x][y];

        if (Random.value > .5f) {   // random chance to prefer x over y axis and vice versa
            if (isWalkable(x + 1, y) && paths[x + 1][y] < shortest) {
                shortest = paths[x + 1][y];
                dir = getRandomPointInTile(x + 1, y);
            }
            if (isWalkable(x - 1, y) && paths[x - 1][y] < shortest) {
                shortest = paths[x - 1][y];
                dir = getRandomPointInTile(x - 1, y);
            }
            if (isWalkable(x, y + 1) && paths[x][y + 1] < shortest) {
                shortest = paths[x][y + 1];
                dir = getRandomPointInTile(x, y + 1);
            }
            if (isWalkable(x, y - 1) && paths[x][y - 1] < shortest) {
                shortest = paths[x][y - 1];
                dir = getRandomPointInTile(x, y - 1);
            }
        } else {
            if (isWalkable(x, y + 1) && paths[x][y + 1] < shortest) {
                shortest = paths[x][y + 1];
                dir = getRandomPointInTile(x, y + 1);
            }
            if (isWalkable(x, y - 1) && paths[x][y - 1] < shortest) {
                shortest = paths[x][y - 1];
                dir = getRandomPointInTile(x, y - 1);
            }
            if (isWalkable(x + 1, y) && paths[x + 1][y] < shortest) {
                shortest = paths[x + 1][y];
                dir = getRandomPointInTile(x + 1, y);
            }
            if (isWalkable(x - 1, y) && paths[x - 1][y] < shortest) {
                shortest = paths[x - 1][y];
                dir = getRandomPointInTile(x - 1, y);
            }
        }
        dir -= new Vector3(xPos, 0f, yPos);
        return dir.normalized;
    }

    private void generatePath(int x, int y) {
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
            if (isWalkable(n.x + 1, n.y) && paths[n.x + 1][n.y] < 0) {
                frontier.Enqueue(new Node(n.x + 1, n.y));
                paths[n.x + 1][n.y] = paths[n.x][n.y] + 1;
            }
            // left neighbor
            if (isWalkable(n.x - 1, n.y) && paths[n.x - 1][n.y] < 0) {
                frontier.Enqueue(new Node(n.x - 1, n.y));
                paths[n.x - 1][n.y] = paths[n.x][n.y] + 1;
            }
            // front neighbor
            if (isWalkable(n.x, n.y + 1) && paths[n.x][n.y + 1] < 0) {
                frontier.Enqueue(new Node(n.x, n.y + 1));
                paths[n.x][n.y + 1] = paths[n.x][n.y] + 1;
            }
            // back neighbor
            if (isWalkable(n.x, n.y - 1) && paths[n.x][n.y - 1] < 0) {
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

    // if inside level and on a walkable tile
    private bool isWalkable(int x, int y) {
        return insideLevel(x, y) && tiles[x][y] == Level.GROUND;
    }

    public bool insideLevel(int x, int y) {
        return x >= 0 && x < tiles.Length && y >= 0 && y < tiles[x].Length;
    }

    public bool insideLevel(float x, float y) {
        return x >= 0f && x <= tiles.Length * ts && y >= 0f && y <= tiles[0].Length * ts;
    }

    public Vector3 clampInside(Vector3 v) {
        return new Vector3(Mathf.Clamp(v.x, 0f, tiles.Length * ts), v.y, Mathf.Clamp(v.z, 0f, tiles[0].Length * ts));
    }

    private Vector3 getRandomPointInTile(int x, int y) {
        if (!insideLevel(x, y)) {
            return new Vector3(x * ts, 0f, y * ts);
        }
        return new Vector3(x * ts + Random.value * ts, 0f, y * ts + Random.value * ts);
    }

    // returns center of a random walkable tile
    public Vector3 getRandomWalkable() {
        int xe, ye;
        int tries = 0;
        do {
            xe = Random.Range(0, tiles.Length);
            ye = Random.Range(0, tiles[0].Length);
        }
        while (!isWalkable(xe, ye) && tries++ < 1000);
        return new Vector3((xe + .5f) * ts, .5f, (ye + .5f) * ts);
    }

    // set and build arrays
    public void setTiles(int[][] t) {
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
        if (paths == null || !drawPathData) {
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
                    Gizmos.DrawCube(new Vector3((x + .5f) * ts, height / 2f, (y + .5f) * ts), new Vector3(.5f, height, .5f));
                }
            }
        }
    }
}
