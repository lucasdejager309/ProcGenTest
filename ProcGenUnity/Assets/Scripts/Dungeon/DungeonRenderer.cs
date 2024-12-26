using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonRenderer : MonoBehaviour
{
    [SerializeField] List<TileBase> palette = new List<TileBase>();
    DungeonGen dGen;
    Grid grid;
    List<GameObject> tilemaps = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        dGen = GetComponent<DungeonGen>();
        grid = GetComponentInChildren<Grid>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            
            foreach (GameObject obj in tilemaps) {Destroy(obj);}
            tilemaps.Clear();
            
            Dungeon dungeon = dGen.NewDungeon();
            DrawRooms(dungeon);
            DrawConnections(dungeon);

        }
    }

    void DrawRooms(Dungeon dungeon) {
        foreach (Room room in dungeon.rooms.Values) {
            GameObject newMap = new GameObject();
            tilemaps.Add(newMap);
            newMap.transform.parent = grid.gameObject.transform;
            newMap.AddComponent<Tilemap>();
            newMap.AddComponent<TilemapRenderer>();
            newMap.name = room.position.ToString();
            newMap.layer = 1;

            for (int x = room.position.x; x < room.position.x+room.area.x; x++) {
                for (int y = room.position.y; y < room.position.y+room.area.y; y++) {
                    newMap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), palette[0]);
                }
            }
        }
    }

    void DrawConnections(Dungeon dungeon) {
        foreach (Connection c in dungeon.connections) {
             GameObject newMap = new GameObject();
            tilemaps.Add(newMap);
            newMap.transform.parent = grid.gameObject.transform;
            newMap.AddComponent<Tilemap>();
            newMap.AddComponent<TilemapRenderer>();
            newMap.name = c.start.ToString() + " " + c.end.ToString();
            newMap.layer = 2;
            
            for (int x = c.start.x; x <= c.end.x; x++) {
                for (int y = c.start.y; y <= c.end.y; y++) {
                    newMap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), palette[1]);
                }
            }
        }
    }
}
