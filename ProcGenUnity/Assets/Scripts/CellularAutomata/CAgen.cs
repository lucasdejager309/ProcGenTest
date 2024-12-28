using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class CAGen
{
    public int[,] cellularAutomata {get; private set;}
    public Vector2Int size;

    public Dictionary<int, List<Area>> areas {get; private set;} = new Dictionary<int, List<Area>>();
    public Dictionary<int, Area> largest {get; private set;} = new Dictionary<int, Area>();

    public CAGen(Vector2Int size, int neighboursRequired, float fillPercent, int steps, int border = 0, bool removeSmallRooms = false, bool largestSizeRequired = false, float minlargestSizeRequired = 0f) {
        this.size = size;

        bool foundGood = false;
        
        int tries = 0;
        while (!foundGood) {
            areas.Clear();
            largest.Clear();

            ///////////////////////

            cellularAutomata = new int[size.x, size.y];
            for (int x = border; x < size.x-border; x++) {
                for (int y = border; y < size.y-border; y++) {
                    cellularAutomata[x, y] = Random.value > fillPercent ? 0 : 1;
                }
            }

            for (int i = 0; i < steps; i++) {
                Step(neighboursRequired, size);
            }

            /////////////////////////////
            
            List<Area> cAreas = GetConnectedAreas(1);

            if (cAreas.Count > 0) {
                largest.Add(1, Area.GetLargest(cAreas));

                if (!largestSizeRequired || (float)largest[1].size/(float)(size.x*size.y) >= minlargestSizeRequired) {
                    areas.Add(1, cAreas);
                    
                    foundGood = true;
                }
            }
            tries++;
        }

        // Debug.Log("CA TRIES: " + tries);

        if (removeSmallRooms) {
            foreach (Area a in areas[1]) {
                    if (a != largest[1]) {
                        foreach (Vector2Int v in a.positions) {
                            cellularAutomata[v.x, v.y] = 0;
                        }
                    }
            }

            areas.Remove(1);
            areas.Add(1, new List<Area>());
            areas[1].Add(largest[1]);
        }
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

    List<Area> GetConnectedAreas(int value) {
        List<Area> areasToReturn = new List<Area>();
        List<Vector2Int> tilesVisited = new List<Vector2Int>();

        Queue<Vector2Int> tileQueue = new Queue<Vector2Int>();
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                if (cellularAutomata[x,y] == value) {
                    tileQueue.Enqueue(new Vector2Int(x,y));
                }
            }
        }

        while (tileQueue.Count > 0) {
            Vector2Int pos = tileQueue.Dequeue();

            if (!tilesVisited.Contains(pos)) {
                List<Vector2Int> fill = FloodFill(pos.x, pos.y, cellularAutomata, value);
                foreach (Vector2Int v in fill) {
                    tilesVisited.Add(v);
                }

                areasToReturn.Add(new Area(value, fill));
            }
    
        }
        
        return areasToReturn;
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

public class Area
{
    public int value { get; private set; }
    public List<Vector2Int> positions { get; private set; } = new List<Vector2Int>();
    public int size { get { return positions.Count; } }
    
    public Dictionary<Vector2Int, List<Vector2Int>> edge { get; private set; } = new Dictionary<Vector2Int, List<Vector2Int>>(); //facing, positions list
    
    public Vector2Int boundsMin { get; private set; } = new Vector2Int(int.MaxValue, int.MaxValue);
    public Vector2Int boundsMax { get; private set; } = new Vector2Int(0,0);
    public Vector2Int dimensions { get 
        {
        return new Vector2Int(boundsMax.x-boundsMin.x, boundsMax.y-boundsMin.y);
        }
    }

    public Area(int value, List<Vector2Int> positions) {
        this.value = value;
        this.positions = positions;
        GetEdge();
    }

    void GetEdge() {
        foreach (Vector2Int pos in positions) {
            List<Vector2Int> neighbours = GetNeighbours(pos);
            if (neighbours.Count < 8) {


                for (int x = -1; x <= 1; x++) {
                    for (int y = -1; y <= 1; y++) {
    
                        if (!(x == 0 && y == 0)) {
                            if (!neighbours.Contains(new Vector2Int(pos.x + x, pos.y + y))) {

                                if (edge.ContainsKey(new Vector2Int(x, y))) {
                                    edge[new Vector2Int(x, y)].Add(pos);
                                } else {
                                    edge.Add(new Vector2Int(x, y), new List<Vector2Int>(){pos});
                                }
                            }
                        }
                    }
                }

                if (boundsMin.x > pos.x) boundsMin = new Vector2Int(pos.x, boundsMin.y);
                if (boundsMin.y > pos.y) boundsMin = new Vector2Int(boundsMin.x, pos.y);

                if (boundsMax.x < pos.x) boundsMax = new Vector2Int(pos.x, boundsMax.y);
                if (boundsMax.y < pos.y) boundsMax = new Vector2Int(boundsMax.x, pos.y);
            }
        }
    }

    List<Vector2Int> GetNeighbours(Vector2Int position) {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (!(x == 0 && y == 0) && positions.Contains(new Vector2Int(position.x + x, position.y + y))) {
                    neighbours.Add(new Vector2Int(position.x + x, position.y + y));
                }
            }
        }

        return neighbours;
    }

    public List<Vector2Int> GetEdgeFacing(Vector2Int dir) {
        List<Vector2Int> edgeToReturn = new List<Vector2Int>(edge[dir]);

        foreach(Vector2Int v in edge[dir]) {
            if (dir.x != 0) {
                for (int x = v.x+dir.x; boundsMin.x < x && x < boundsMax.x; x += dir.x) {
                    if (positions.Contains(new Vector2Int(x, v.y))) {
                        edgeToReturn.Remove(v);
                    }
                }
            }

            if (dir.y != 0) {
                for (int y = v.y+dir.y; boundsMin.y < y && y < boundsMax.y; y += dir.y) {
                    if (positions.Contains(new Vector2Int(v.x, y))) {
                        edgeToReturn.Remove(v);
                    }
                }
            }
        }

        List<Vector2Int> tempList = new List<Vector2Int>();
        foreach (Vector2Int e in edgeToReturn) {
            foreach (Vector2Int v in GetNeighbours(e)) {
                if (edgeToReturn.Contains(v)) {
                    tempList.Add(e);
                    break;
                }
            }
        }

        return tempList;
    }

    static public Area GetLargest(List<Area> areasToCheck) {
        Area largest;
        largest = areasToCheck[0];

        foreach (Area area in areasToCheck){
            if (area.size > largest.size) {
                largest = area;
            }
        }

        return largest;

    }
}




