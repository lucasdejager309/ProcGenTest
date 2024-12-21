using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonGen : MonoBehaviour {
    [Header("BSP")]
    [SerializeField] int iterations = 4;
    [SerializeField] Vector2Int size = new Vector2Int(100, 100);
    [SerializeField] [Range(1, 3)]float maxWidthHeightFactor = 1.5f;
    [SerializeField] [Range(0, 0.5f)] float partitionVariation = 0.125f;
    [Header("Rooms")]
    [SerializeField] bool randomRoomSize = true;
    [SerializeField] [Range(0, 1)] float minRoomSize = 0.4f;
    [SerializeField] [Range(0, 1)] float maxRoomSize = 0.8f;
    [SerializeField] bool randomizeRoomPosition = true;

    public Dungeon NewDungeon() {
        Dungeon dungeon = new Dungeon();
        
        BSPTree tree = BSPTree.NewTree(iterations, size, maxWidthHeightFactor, partitionVariation);
        dungeon.rooms = PopulateTreeWithRooms(tree);

        return dungeon;
    }

    List<Room> PopulateTreeWithRooms(BSPTree tree) {
        List<Room> rooms = new List<Room>();
        
        foreach (BSPNode leaf in tree.leafs) {
            
            Vector2Int area = new Vector2Int();

            if (randomRoomSize) {
                area = new Vector2Int(
                    Mathf.RoundToInt (Random.Range(minRoomSize, maxRoomSize)*leaf.size.x), 
                    Mathf.RoundToInt(Random.Range(minRoomSize, maxRoomSize)*leaf.size.y)
                );
            } else {
                area = new Vector2Int(Mathf.RoundToInt(leaf.size.x*0.8f), Mathf.RoundToInt(leaf.size.y*0.8f));
            }
            

            Vector2Int position = new Vector2Int();

            if (randomizeRoomPosition) {
                position.x = Mathf.RoundToInt(Random.Range(leaf.position.x, leaf.position.x+(leaf.size.x-area.x)));
                position.y = Mathf.RoundToInt(Random.Range(leaf.position.y, leaf.position.y+(leaf.size.y-area.y)));
            } 
            else {
                position.x = leaf.position.x+(leaf.size.x-area.x)/2;
                position.y = leaf.position.y+(leaf.size.y-area.y)/2;
            }

            rooms.Add(new Room(position, area));
        }

        return rooms;
    }
}

public class Dungeon {
    public List<Room> rooms = new List<Room>();
}

public class Room {
    public Vector2Int position;
    public Vector2Int area;
    public List<Room> connectedAreas = new List<Room>();
    public bool isConnected {get; private set;} = false; 

    public Room(Vector2Int pos, Vector2Int area) {
        this.position = pos;
        this.area = area;
    }
}