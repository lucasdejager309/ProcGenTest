using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CARenderer : MonoBehaviour
{
    public TileBase[] palette;
    public Tilemap map;
    List<GameObject> tempMaps = new List<GameObject>();
    
    CAgen ca;

    void Start() {
        ca = GetComponent<CAgen>();
        Reset();
        DrawTiles(ca.cellularAutomata);
    }

    void Update() {
        
        //RESET
        if (Input.GetKeyDown(KeyCode.R)) {
            Reset();
            DrawTiles(ca.cellularAutomata);
        }

        //SHOWAREAS
        if (Input.GetKeyDown(KeyCode.A)) {
            List<List<Vector2Int>> connected = ca.GetConnectedAreas(1);

            List<Vector2Int> largest = new List<Vector2Int>();

            foreach (List<Vector2Int> list in connected) {
                if (list.Count > largest.Count) {
                    largest = list;
                }
            }
            
            foreach (List<Vector2Int> area in connected) {
                if (area == largest) continue;

                GameObject newMap = new GameObject();
                tempMaps.Add(newMap);
                newMap.transform.parent = map.transform.parent;
                newMap.AddComponent<Tilemap>();
                newMap.AddComponent<TilemapRenderer>();
                

                foreach (Vector2Int v in area) {
                    newMap.GetComponent<Tilemap>().SetTile(new Vector3Int(v.x, v.y, 0), palette[2]);
                }
            }

            // foreach (Vector2Int tile in largest) {
            //     DrawTile(tile, palette[2]);
            // }
        }
    }

    void Reset() {

        foreach (GameObject obj in tempMaps) {
            Destroy(obj);
        }
        tempMaps.Clear();
        

        ca.GetNew();
        map.ClearAllTiles();
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

