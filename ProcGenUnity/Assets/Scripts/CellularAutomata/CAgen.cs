using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CAGen
{
    public int[,] cellularAutomata {get; private set;}
    public Vector2Int size;

    public Dictionary<int, List<List<Vector2Int>>> connectedAreas {get; private set;}
    public Dictionary<int, List<Vector2Int>> largest {get; private set;} = null;

    public CAGen(Vector2Int size, int neighboursRequired, float fillPercent, int steps, int border = 0, bool removeSmallRooms = false, bool largestSizeRequired = false, float minlargestSizeRequired = 0f) {
        this.size = size;

        bool foundGood = false;
        
        while (!foundGood) {
            connectedAreas = null;
            largest = null;

            cellularAutomata = new int[size.x, size.y];
            for (int x = 0+border; x < size.x-border; x++) {
                for (int y = 0+border; y < size.y-border; y++) {
                    cellularAutomata[x, y] = Random.value > fillPercent ? 0 : 1;
                }
            }

            for (int i = 0; i < steps; i++) {
                Step(neighboursRequired, size);
            }

            connectedAreas = new Dictionary<int, List<List<Vector2Int>>>();
            largest = new Dictionary<int, List<Vector2Int>>();
            GetConnectedAreas(1);
            GetLargest(1);

            if (!largestSizeRequired) foundGood = true;
            
            else if ((float)largest[1].Count/(float)(size.x*size.y) >= minlargestSizeRequired) {
                foundGood = true;
            }
        }

        if (removeSmallRooms) {
            foreach (List<Vector2Int> area in connectedAreas[1]) {
                if (area != largest[1]) {
                    foreach (Vector2Int v in area) {
                        cellularAutomata[v.x, v.y] = 0;
                    }
                }
            }

            connectedAreas[1].Clear();
            connectedAreas[1].Add(largest[1]);
        }

        //Debug.Log((float)GetLargest(1).Count/(float)(size.x*size.y) + " " + minlargestSizeRequired);
    }

    void Step(int neighboursRequired, Vector2Int size) {
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

    List<List<Vector2Int>> GetConnectedAreas(int value) {
        if (!connectedAreas.ContainsKey(value)) {
            connectedAreas.Add(value, new List<List<Vector2Int>>());
            
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
                    
                    if (!connectedAreas.ContainsKey(value)) connectedAreas.Add(value, new List<List<Vector2Int>>());
                    connectedAreas[value].Add(fill);

                    foreach(Vector2Int v in fill) {
                        tilesVisited.Add(v);
                    }
                }
            }
        }
        
        return connectedAreas[value];
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

    List<Vector2Int> GetLargest(int value) {
        if (!largest.ContainsKey(value)) {
            Debug.Log(connectedAreas[value].Count);
            foreach (List<Vector2Int> list in connectedAreas[value]) {
                
                if (!largest.ContainsKey(value)) {
                        largest.Add(value, list);
                }

                else if (list.Count > largest[value].Count) {
                    largest[value] = list;
                }
            }
            
            return largest[value];

        } else {
            return largest[value];
        }

    }

    // List<Vector2Int> GetEdgeTiles(int value) {

    // }

    public class Area {
        int value;
        List<Vector2Int> positions;
        public int size {get {return positions.Count;}}
        Dictionary<Vector2Int, Vector2Int> edge; //position, facing

        public Area(int value, List<Vector2Int> positions) {
            this.value = value;
            this.positions = positions;
        }
    }
}


