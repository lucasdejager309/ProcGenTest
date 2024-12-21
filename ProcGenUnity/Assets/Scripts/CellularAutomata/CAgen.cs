using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAgen : MonoBehaviour
{
    public int[,] cellularAutomata;
    public Vector2Int size;
    [Range(1,9)] [SerializeField] int neighboursRequired;
    [Range(0,1)] [SerializeField] float fillPercent = 0.5f;
    [Range(0, 100)][SerializeField] int border = 0;
    [Range(0,10)] [SerializeField] int steps = 3;

    public void GetNew() {
        cellularAutomata = new int[size.x, size.y];
        for (int x = 0+border; x < size.x-border; x++) {
            for (int y = 0+border; y < size.y-border; y++) {
                cellularAutomata[x, y] = Random.value > fillPercent ? 0 : 1;
            }
        }

        for (int i = 0; i < steps; i++) {
            Step();
        }
    }

    void Step() {
        int[,] caBuffer = new int[size.x, size.y];

        for (int x = 0; x < size.x; ++x) {
            for (int y = 0; y < size.y; ++y) {
                int liveCellCount = cellularAutomata[x,y] + GetNeigbourCount(x,y);
                caBuffer[x,y] = liveCellCount > neighboursRequired ? 1 : 0;
            }
        }

        for (int x = 0; x < size.x; ++x) {
            for (int y = 0; y < size.y; ++y) {
                cellularAutomata[x,y] = caBuffer[x,y];
            }
        }
    }

    int GetNeigbourCount(int x, int y) {
        int count = 0;

        Dictionary<Vector2Int, int> neighbours = GetNeighbours(x, y, cellularAutomata);
        foreach (KeyValuePair<Vector2Int, int> n in neighbours) {
            if (n.Value == 1) count +=1;
        }

        return count;
    }

    Dictionary<Vector2Int, int> GetNeighbours (int x, int y, int[,] data) {
        Dictionary<Vector2Int, int> neighbours = new Dictionary<Vector2Int, int>();
        for (int rx = -1; rx <= 1; rx++) {
            for (int ry = -1; ry <= 1; ry++) {
                if (rx == 0 && ry == 0) continue; //you are not your own neighbour
                if (x+rx < 0 || x+rx > size.x-1 || y+ry < 0 || y+ry > size.y-1) continue; //your neighbour needs to be within bounds (or he doesnt exist)

                neighbours.Add(new Vector2Int(x+rx, y+ry), data[x+rx, y+ry]);
            }
        }

        return neighbours;
    }

    public List<List<Vector2Int>> GetConnectedAreas(int value) {
        List<List<Vector2Int>> connectedareas = new List<List<Vector2Int>>();
        List<Vector2Int> tilesVisited = new List<Vector2Int>();
        
        List<Vector2Int> tilesToCheck = new List<Vector2Int>();
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                if (cellularAutomata[x,y] == value) {
                    tilesToCheck.Add(new Vector2Int(x,y));
                }
            }
        }
        
        
        for (int x = 0; x < cellularAutomata.GetLength(0); x++) {
            for (int y = 0; y < cellularAutomata.GetLength(1); y++) {
                if (tilesVisited.Contains(new Vector2Int(x,y))) continue; //if visited
                if (!tilesToCheck.Contains(new Vector2Int(x,y))) continue; //if not to check

                List<Vector2Int> fill = FloodFill(x, y, cellularAutomata, value);
                connectedareas.Add(fill);

                foreach(Vector2Int v in fill) {
                    tilesVisited.Add(v);
                }
            }
        }

        return connectedareas;
    }

    List<Vector2Int> FloodFill(int x, int y, int[,] data, int value) {
        List<Vector2Int> fill = new List<Vector2Int>();
        fill.Add(new Vector2Int(x,y)); //add starting point

        List<Vector2Int> visited = new List<Vector2Int>();
        
        Queue<KeyValuePair<Vector2Int, int>> frontier = new Queue<KeyValuePair<Vector2Int, int>>();
        foreach (KeyValuePair<Vector2Int, int> n in GetNeighbours(x, y, data)) {
            frontier.Enqueue(n);
        }

        while(frontier.Count > 0) {
            KeyValuePair<Vector2Int, int> current = frontier.Dequeue();

            if (current.Value == value && !visited.Contains(current.Key) && !fill.Contains(current.Key)) {
                fill.Add(current.Key);
                
                foreach (KeyValuePair<Vector2Int, int> n in GetNeighbours(current.Key.x, current.Key.y, data)) {
                    frontier.Enqueue(n);
                }
            }

            visited.Add(current.Key);
        }
        
        return fill;
    }

    public bool IsInBounds(int x, int y) {
        return (x >= 0 && y >= 0 && x < size.x && y < size.y);
    }
}
