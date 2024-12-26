using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonGen : MonoBehaviour
{
    [Header("BSP")]
    [SerializeField] int iterations = 4;
    [SerializeField] Vector2Int size = new Vector2Int(100, 100);
    [SerializeField][Range(1, 3)] float maxWidthHeightFactor = 1.5f;
    [SerializeField][Range(0, 0.5f)] float partitionVariation = 0.125f;
    [Header("Rooms")]
    [SerializeField] bool randomRoomSize = true;
    [SerializeField][Range(0, 1)] float minRoomSize = 0.4f;
    [SerializeField][Range(0, 1)] float maxRoomSize = 0.8f;
    [SerializeField] bool randomizeRoomPosition = true;

    public Dungeon NewDungeon()
    {
        BSPTree tree = new BSPTree(iterations, size, maxWidthHeightFactor, partitionVariation);
        Dungeon dungeon = new Dungeon(tree);

        dungeon.PopulateWithRooms(randomRoomSize, minRoomSize, maxRoomSize, randomizeRoomPosition);

        dungeon.CreateConnections();

        return dungeon;
    }
}

public class Dungeon
{
    public BSPTree tree;
    public Dictionary<BSPNode, Room> rooms { get; private set; } = new Dictionary<BSPNode, Room>();
    public List<Connection> connections = new List<Connection>();

    public Dungeon(BSPTree tree)
    {
        this.tree = tree;
    }

    public static List<Room> GetRoomsInNodeLeafs(BSPNode node, Dictionary<BSPNode, Room> rooms)
    {
        List<BSPNode> leafs = node.leafs;
        List<Room> roomsInNode = new List<Room>();
        foreach (BSPNode leaf in leafs)
        {
            roomsInNode.Add(rooms[leaf]);
        }

        return roomsInNode;

    }


    public void PopulateWithRooms(bool randomRoomSize, float minRoomSize, float maxRoomSize, bool randomizeRoomPosition)
    {
        foreach (BSPNode leaf in tree.leafs)
        {

            Vector2Int area = new Vector2Int();

            if (randomRoomSize)
            {
                area = new Vector2Int(
                    Mathf.RoundToInt(Random.Range(minRoomSize, maxRoomSize) * leaf.size.x),
                    Mathf.RoundToInt(Random.Range(minRoomSize, maxRoomSize) * leaf.size.y)
                );
            }
            else
            {
                area = new Vector2Int(Mathf.RoundToInt(leaf.size.x * 0.8f), Mathf.RoundToInt(leaf.size.y * 0.8f));
            }


            Vector2Int position = new Vector2Int();

            if (randomizeRoomPosition)
            {
                position.x = Mathf.RoundToInt(Random.Range(leaf.position.x, leaf.position.x + (leaf.size.x - area.x)));
                position.y = Mathf.RoundToInt(Random.Range(leaf.position.y, leaf.position.y + (leaf.size.y - area.y)));
            }
            else
            {
                position.x = leaf.position.x + (leaf.size.x - area.x) / 2;
                position.y = leaf.position.y + (leaf.size.y - area.y) / 2;
            }

            rooms.Add(leaf, new Room(position, area));
        }

    }

    public void CreateConnections() {
        foreach (KeyValuePair<KeyValuePair<BSPNode, BSPNode>, int> kv in tree.pairs)
        {
            ConnectAreas(kv.Key.Key, kv.Key.Value);
        }
    }

    void ConnectAreas(BSPNode n1, BSPNode n2)
    {
        Vector2Int dir = BSPNode.GetDirection(n1, n2);

        List<BSPNode> leafs1 = BSPNode.GetLeafsFacing(n1, dir);
        List<BSPNode> leafs2 = BSPNode.GetLeafsFacing(n2, -dir);

        //pick random edge room from first leaf
        Room room1 = rooms[leafs1[Random.Range(0, leafs1.Count - 1)]];

        //find edge room from second leaf that has a side facing first edge room
        Room room2 = null;
        foreach (BSPNode leaf in leafs2)
        {
            if (rooms[leaf] != null)
            {
                Room room = rooms[leaf];

                if ((dir.x == 1 || dir.x == -1) && room1.position.y <= room.position.y + room.area.y && room1.position.y + room1.area.y >= room.position.y)
                {
                    room2 = room;
                }

                if ((dir.y == 1 || dir.y == -1) && room1.position.x <= room.position.x + room.area.x && room1.position.x + room1.area.x >= room.position.x)
                {
                    room2 = room;
                }

            }

        }
        if (room2 == null) return;


        Vector2Int start = new Vector2Int();
        Vector2Int end = new Vector2Int();

        if (dir.y == 1 || dir.y == -1) {
            int minX = Mathf.Max(room1.position.x, room2.position.x);
            int maxX = Mathf.Min(room1.position.x+room1.area.x, room2.position.x+room2.area.x)-1;

            start.x = Random.Range(minX, maxX);
            end.x = start.x;

            if (dir.y == 1) {
                start.y = room1.position.y+room1.area.y;
                end.y = room2.position.y-1;
            } else {
                start.y = room1.position.y;
                end.y = room2.position.y+room2.area.y;
            }
        }

        else {
            int minY = Mathf.Max(room1.position.y, room2.position.y);
            int maxY = Mathf.Min(room1.position.y+room1.area.y, room2.position.y+room2.area.y)-1;

            start.y = Random.Range(minY, maxY);
            end.y = start.y;

            if (dir.x == 1) {
                start.x = room1.position.x+room1.area.x;
                end.x = room2.position.x-1;
            } else {
                start.x = room1.position.x;
                end.x = room2.position.x+room2.area.x;
            }
        }

        connections.Add(new Connection(start, end, dir, room1, room2));

    }

        public void ConnectRooms() {
        //     foreach(BSPNode n in tree.leafs) {
        //         Vector2Int dir = BSPNode.GetDirection(n, n.GetSibling());

        //         Room room1 = rooms[n];
        //         Room room2 = rooms[n.GetSibling()];

        //         if (room1.connectedAreas.Contains(room2) || room2.connectedAreas.Contains(room1)) continue;

        //         //this code is shit
        //         if (dir.y == 0) {
        //             //horizontal connection
        //             int minY = Mathf.Max(room1.position.y, room2.position.y);
        //             int maxY = Mathf.Min(room1.position.y+room1.area.y, room2.position.y+room2.area.y);

        //             int startY = Random.Range(minY, maxY);

        //             int startX;
        //             int endX;
        //             if (dir.x == 1) {
        //                 startX = room1.position.x+room1.area.x;
        //                 endX = room2.position.x;
        //             } else {
        //                 startX = room2.position.x+room2.area.x;
        //                 endX= room1.position.x;
        //             }

        //             Connection c = new Connection(new Vector2Int(startX, startY), new Vector2Int(endX-1, startY), room1, room2);
        //             connections.Add(c);
        //             room1.connectedAreas.Add(room2);
        //             room2.connectedAreas.Add(room1);
        //         } else {
        //             //vertical connection
        //             int minX = Mathf.Max(room1.position.x, room2.position.x);
        //             int maxX = Mathf.Min(room1.position.x+room1.area.x, room2.position.x+room2.area.x);

        //             int startX = Random.Range(minX, maxX);

        //             int startY;
        //             int endY;
        //             if (dir.y == 1) {
        //                 startY = room1.position.y+room1.area.y;
        //                 endY = room2.position.y;
        //             } else {
        //                 startY = room2.position.y+room2.area.y;
        //                 endY= room1.position.y;
        //             }

        //             Connection c = new Connection(new Vector2Int(startX, startY), new Vector2Int(startX, endY-1), room1, room2);
        //             connections.Add(c);
        //             room1.connectedAreas.Add(room2);
        //             room2.connectedAreas.Add(room1);
        //         }
        //     }
    }
}

public class Room
{
    public Vector2Int position;
    public Vector2Int area;
    public bool isConnected { get; private set; } = false;

    public Room(Vector2Int pos, Vector2Int area)
    {
        this.position = pos;
        this.area = area;
    }

    public List<Vector2Int> GetEdgeFacing(Vector2Int dir) {
        List<Vector2Int> edge = new List<Vector2Int>();

        if (dir.x == -1 || dir.x == 1) {
            for(int y = position.y; y < position.y+area.y; y++) { 
                if (dir.x == -1) {
                    edge.Add(new Vector2Int(position.x, y));
                } else {
                    edge.Add(new Vector2Int(position.x+area.x-1, y));
                }
            }
        }

        if (dir.y == -1 || dir.y == 1) {
            for(int x = position.x; x < position.x+area.x; x++) { 
                if (dir.y == -1) {
                    edge.Add(new Vector2Int(x, position.y));
                } else {
                    edge.Add(new Vector2Int(x, position.y+area.y));
                }
            }
        }

        return edge;
    }
}

public class Connection
{
    List<Room> connections = new List<Room>();

    public Vector2Int start { get; private set; }
    public Vector2Int end { get; private set; }
    public Vector2Int dir {get; private set;}

    public Connection(Vector2Int start, Vector2Int end, Vector2Int dir, Room room1, Room room2)
    {
        this.start = start;
        this.end = end;
        this.dir = dir;

        connections.Add(room1);
        connections.Add(room2);
    }

    public List<Vector2Int> GetPositions() {
        List<Vector2Int> toReturn = new List<Vector2Int>();

        if (dir.x == 1) {
            for (int x = start.x; x < end.x; x++) {
                toReturn.Add(new Vector2Int(x, start.y));
            }
        }
        if (dir.y == 1) {
            for (int y = start.y; y < end.y; y++) {
                toReturn.Add(new Vector2Int(start.x, y));
            }
        }

        return toReturn;
    }
}