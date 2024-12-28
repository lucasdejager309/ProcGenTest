using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            foreach (KeyValuePair<BSPNode, Area> area in caDungeonGen.leafAreas) {
                foreach (Vector2Int v in area.Value.positions) {
                    tiles[v.x+area.Key.position.x, v.y+area.Key.position.y] = area.Value.value;
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
