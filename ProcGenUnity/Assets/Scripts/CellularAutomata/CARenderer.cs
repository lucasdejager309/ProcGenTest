using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CARenderer : MonoBehaviour
{
    [SerializeField] Vector2Int size;
    [Range(1,9)] [SerializeField] int neighboursRequired;
    [Range(0,1)] [SerializeField] float fillPercent = 0.5f;
    [Range(0, 100)][SerializeField] int border = 0;
    [Range(0,10)] [SerializeField] int steps = 3;
    [SerializeField] bool largestSizeRequired = false;
    [Range(0,1)] [SerializeField] float minlargestSizeRequired = 0.4f;
    [SerializeField] bool removeSmallRooms = false;

    public TileBase[] palette;
    public Tilemap map;
    List<GameObject> tempMaps = new List<GameObject>();
    
    CAGen ca;

    void Start() {
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
            
            Area largest = ca.largest[1];
            
            foreach (Area area in ca.areas[1]) {
                if (area == largest) continue;

                GameObject newMap = new GameObject();
                tempMaps.Add(newMap);
                newMap.transform.parent = map.transform.parent;
                newMap.AddComponent<Tilemap>();
                newMap.AddComponent<TilemapRenderer>();
                

                foreach (Vector2Int v in area.positions) {
                    newMap.GetComponent<Tilemap>().SetTile(new Vector3Int(v.x, v.y, 0), palette[2]);
                }
            }
        }
    }

    void Reset() {

        foreach (GameObject obj in tempMaps) {
            Destroy(obj);
        }
        tempMaps.Clear();
        

        ca = new CAGen(size, neighboursRequired, fillPercent, steps, border, removeSmallRooms, largestSizeRequired, minlargestSizeRequired);
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

