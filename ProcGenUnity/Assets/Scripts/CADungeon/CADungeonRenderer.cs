using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CADungeonRenderer : MonoBehaviour
{
    public TileBase[] palette;
    public Tilemap map;

    CADungeonGen caDungeonGen;

    void Start() {
        caDungeonGen = GetComponent<CADungeonGen>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            caDungeonGen.NewDungeon();

            int[,] tiles = new int[caDungeonGen.size.x, caDungeonGen.size.y];
            foreach (KeyValuePair<BSPNode, CAGen> room in caDungeonGen.rooms) {
                for (int x = 0; x < room.Value.size.x; x++) {
                    for (int y = 0; y < room.Value.size.y; y++) {
                        tiles[x+room.Key.position.x, y+room.Key.position.y] = room.Value.cellularAutomata[x, y];
                    }
                }
            }

            DrawTiles(tiles);
        }
    }

    void DrawTiles(int[,] tiles) {
        for (int x = 0; x < tiles.GetLength(0); x++) {
            for (int y = 0; y < tiles.GetLength(1); y++) {
                DrawTile(new Vector2Int(x,y), palette[tiles[x,y]]);
            }
        }
    }

    void DrawTile(Vector2Int pos, TileBase tile) {
        map.SetTile(new Vector3Int(pos.x, pos.y, 0), tile);
    }
}
