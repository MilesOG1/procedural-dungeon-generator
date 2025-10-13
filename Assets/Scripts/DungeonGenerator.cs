using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Basic procedural dungeon generator:
/// - stores a 2D int grid (0=empty,1=floor,2=wall)
/// - randomly places non-overlapping rectangular rooms
/// - connects rooms by carving straight tunnels between centers
/// - instantiates floor/wall prefabs to visualize the map
/// 
/// This is intentionally simple and readable so you can learn and extend it.
/// </summary>
public class DungeonGenerator : MonoBehaviour
{
    [Header("Map Size")]
    public int mapWidth = 80;     // number of tiles horizontally
    public int mapHeight = 50;    // number of tiles vertically

    [Header("Room Settings")]
    public int maxRooms = 12;     // try to place up to this many rooms
    public int minRoomSize = 4;   // minimum width/height of a room
    public int maxRoomSize = 10;  // maximum width/height of a room

    [Header("Prefabs & Parents")]
    public GameObject floorPrefab;    // assign in inspector
    public GameObject wallPrefab;     // assign in inspector
    public Transform tilesParent;     // optional: parent transform to hold created tiles

    [Header("Generation Options")]
    public bool autoGenerateOnStart = true;   // generate on Start
    public int seed = 0;        // 0 = random seed; otherwise deterministic

    // internal map grid: 0 empty, 1 floor, 2 wall
    private int[,] mapGrid;

    // list of placed rooms (for corridors and later logic)
    private List<RectInt> rooms = new List<RectInt>();

    // store references to created tile GameObjects so we can destroy them on regenerate
    private List<GameObject> createdTiles = new List<GameObject>();

    void Start()
    {
        if (autoGenerateOnStart)
        {
            GenerateDungeon();
        }
    }

    /// <summary>
    /// Public method to trigger generation (can be attached to UI button)
    /// </summary>
    public void GenerateDungeon()
    {
        // clear previous tiles and data
        ClearDungeon();

        // optionally use deterministic seed for reproducibility
        if (seed != 0)
            Random.InitState(seed);
        else
            Random.InitState(System.Environment.TickCount);

        // initialize grid
        mapGrid = new int[mapWidth, mapHeight];
        rooms.Clear();

        // 1) Place rooms
        for (int i = 0; i < maxRooms; i++)
        {
            // choose random size
            int w = Random.Range(minRoomSize, maxRoomSize + 1);
            int h = Random.Range(minRoomSize, maxRoomSize + 1);

            // choose random position (room Rect uses x,y as bottom-left)
            int x = Random.Range(1, mapWidth - w - 1);
            int y = Random.Range(1, mapHeight - h - 1);

            RectInt newRoom = new RectInt(x, y, w, h);

            // check overlap with existing rooms (with 1-tile padding)
            bool overlaps = false;
            foreach (RectInt room in rooms)
            {
                RectInt padded = new RectInt(room.xMin - 1, room.yMin - 1, room.width + 2, room.height + 2);
                if (padded.Overlaps(newRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                // accept room: carve floor tiles
                rooms.Add(newRoom);
                for (int rx = newRoom.xMin; rx < newRoom.xMax; rx++)
                {
                    for (int ry = newRoom.yMin; ry < newRoom.yMax; ry++)
                    {
                        mapGrid[rx, ry] = 1; // floor
                    }
                }
            }
        }

        // 2) Connect rooms: simple method — connect room centers in order with L-shaped corridors
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int prevCenter = Vector2Int.RoundToInt(rooms[i - 1].center);
            Vector2Int curCenter = Vector2Int.RoundToInt(rooms[i].center);

            // randomly decide corridor order for variety
            if (Random.value < 0.5f)
            {
                CarveHorizontalTunnel(prevCenter.x, curCenter.x, prevCenter.y);
                CarveVerticalTunnel(prevCenter.y, curCenter.y, curCenter.x);
            }
            else
            {
                CarveVerticalTunnel(prevCenter.y, curCenter.y, prevCenter.x);
                CarveHorizontalTunnel(prevCenter.x, curCenter.x, curCenter.y);
            }
        }

        // 3) surround floor with walls (optional, simple method)
        AddWallsAroundFloors();

        // 4) Spawn prefabs to visualize
        DrawMap();
    }

    // carve horizontal corridor on given y across x1..x2
    private void CarveHorizontalTunnel(int x1, int x2, int y)
    {
        int start = Mathf.Min(x1, x2);
        int end = Mathf.Max(x1, x2);
        for (int x = start; x <= end; x++)
        {
            if (IsInBounds(x, y))
                mapGrid[x, y] = 1; // floor
        }
    }

    // carve vertical corridor on given x across y1..y2
    private void CarveVerticalTunnel(int y1, int y2, int x)
    {
        int start = Mathf.Min(y1, y2);
        int end = Mathf.Max(y1, y2);
        for (int y = start; y <= end; y++)
        {
            if (IsInBounds(x, y))
                mapGrid[x, y] = 1; // floor
        }
    }

    // mark surrounding wall tiles: any empty tile adjacent to a floor becomes a wall
    private void AddWallsAroundFloors()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapGrid[x, y] == 0)
                {
                    // check 4-neighbour adjacency for any floor
                    bool adjacentToFloor = false;
                    Vector2Int[] neighbours = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
                    foreach (var n in neighbours)
                    {
                        int nx = x + n.x;
                        int ny = y + n.y;
                        if (IsInBounds(nx, ny) && mapGrid[nx, ny] == 1)
                        {
                            adjacentToFloor = true;
                            break;
                        }
                    }

                    if (adjacentToFloor)
                        mapGrid[x, y] = 2; // wall
                }
            }
        }
    }

    // instantiate prefabs based on mapGrid
    private void DrawMap()
    {
        if (floorPrefab == null || wallPrefab == null)
        {
            Debug.LogWarning("Prefabs not assigned in DungeonGenerator.");
            return;
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3 pos = new Vector3(x, y, 0f); // z=0 for 2D
                GameObject toCreate = null;

                if (mapGrid[x, y] == 1)
                    toCreate = Instantiate(floorPrefab, pos, Quaternion.identity, tilesParent);
                else if (mapGrid[x, y] == 2)
                    toCreate = Instantiate(wallPrefab, pos, Quaternion.identity, tilesParent);

                if (toCreate != null)
                    createdTiles.Add(toCreate);
            }
        }
    }

    // helper: bounds check
    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
    }

    // remove previously created tile GameObjects
    private void ClearDungeon()
    {
        foreach (GameObject g in createdTiles)
        {
            if (g != null)
                DestroyImmediate(g);
        }
        createdTiles.Clear();
    }

    // for editor convenience: regenerate from inspector
#if UNITY_EDITOR
    // This function will be called by a UI button you can add in the Editor as needed.
    [ContextMenu("Regenerate Dungeon")]
    private void EditorRegenerate()
    {
        GenerateDungeon();
    }
#endif
}
