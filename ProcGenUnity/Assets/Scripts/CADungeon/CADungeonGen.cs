using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class CADungeonGen : MonoBehaviour
{
    [Header("BSP")]
    [SerializeField] int iterations = 4;
    public Vector2Int size = new Vector2Int(100, 100);
    [SerializeField][Range(1, 3)] float maxWidthHeightFactor = 1.5f;
    [SerializeField][Range(0, 0.5f)] float partitionVariation = 0.125f;

    [Header("Cellular Automata")]
    [Range(1,9)] [SerializeField] int neighboursRequired;
    [Range(0,1)] [SerializeField] float fillPercent = 0.5f;
    [Range(0, 100)][SerializeField] int border = 0;
    [Range(0,10)] [SerializeField] int steps = 3;
    [SerializeField] bool largestSizeRequired = false;
    [Range(0,1)] [SerializeField] float minlargestSizeRequired = 0.4f;
    [SerializeField] bool removeSmallRooms = false;

    public Dictionary<BSPNode, Area> leafAreas {get; private set;} = new Dictionary<BSPNode, Area>();
    Dictionary<BSPNode, List<Area>> areas = new Dictionary<BSPNode, List<Area>>();
    public BSPTree tree {get; private set;} = null;

    public void NewDungeon() {
        leafAreas.Clear();
        areas.Clear();

        tree = new BSPTree(iterations, size, maxWidthHeightFactor, partitionVariation);

        //create rooms in leafs
        foreach (BSPNode leaf in tree.leafs) {
            CAGen area = new CAGen(leaf.size, neighboursRequired, fillPercent, steps, border, removeSmallRooms, largestSizeRequired, minlargestSizeRequired);
            leafAreas.Add(leaf, area.largest[1]);
        }

        foreach (BSPNode node in tree.nodes) {
            List<Area> areasInNode = new List<Area>();
            foreach (BSPNode leaf in node.leafs) {
                areasInNode.Add(leafAreas[leaf]);
            }

            areas.Add(node, areasInNode);
        }
        
        CreateConnections();
    }

    void CreateConnections() {
        foreach (KeyValuePair<KeyValuePair<BSPNode, BSPNode>, int> kv in tree.pairs)
        {
            ConnectAreas(kv.Key.Key, kv.Key.Value);
        }
    }

    void ConnectAreas(BSPNode n1,  BSPNode n2) {
        Vector2Int dir = BSPNode.GetDirection(n1, n2);

        List<BSPNode> leafs1 = BSPNode.GetLeafsFacing(n1, dir);
        List<BSPNode> leafs2 = BSPNode.GetLeafsFacing(n2, -dir);

        //get first Node
        BSPNode leaf1 = leafs1[Random.Range(0, leafs1.Count - 1)];
        BSPNode leaf2 = null;

        //get second Node that faces first Node
        foreach (BSPNode l2 in leafs2) {
            if (dir.x != 0) {
                if (leaf1.position.y+leafAreas[leaf1].boundsMin.y <= l2.position.y+leafAreas[l2].boundsMax.y &&
                    leaf1.position.y+leafAreas[leaf1].boundsMax.y >= l2.position.y+leafAreas[l2].boundsMin.y) 
                {
                    leaf2 = l2;
                }
            }

            if (dir.y != 0) {
                if (leaf1.position.x+leafAreas[leaf1].boundsMin.x <= l2.position.x+leafAreas[l2].boundsMax.x &&
                    leaf1.position.x+leafAreas[leaf1].boundsMax.x >= l2.position.x+leafAreas[l2].boundsMin.x) 
                {
                    leaf2 = l2;
                }
            }
        }

        Area area1 = leafAreas[leaf1];
        Area area2 = leafAreas[leaf2];

        List<Vector2Int> f1 = area1.GetEdgeFacing(dir);
        List<Vector2Int> f2 = area2.GetEdgeFacing(-dir);

        Vector2Int v1 = new Vector2Int();
        Vector2Int v2 = new Vector2Int();

        if (dir.x != 0) {
            int y = Random.Range(Mathf.Max(area1.boundsMin.y, area2.boundsMin.y), Mathf.Min(area1.boundsMax.y, area2.boundsMax.y));
            
            v1.y = y;
            foreach (Vector2Int vf1 in f1) {
                if (vf1.y == y) {
                    v1.x = vf1.x;
                }
            }

            v2.y = y;
            foreach (Vector2Int vf2 in f2) {
                if (vf2.y == y) {
                    v2.x = vf2.x;
                }
            }

        }

        if (dir.y != 0) {
            int x = Random.Range(Mathf.Max(area1.boundsMin.x, area2.boundsMin.x), Mathf.Min(area1.boundsMax.x, area2.boundsMax.x));
            
            v1.x = x;
            foreach (Vector2Int vf1 in f1) {
                if (vf1.x == x) {
                    v1.y = vf1.y;
                }
            }

            v2.x = x;
            foreach (Vector2Int vf2 in f2) {
                if (vf2.x == x) {
                    v2.y = vf2.y;
                }
            }
        }

        
    }
}

public class DungeonConnection {
    public Vector2Int start {get; private set;}
    public Vector2Int end {get; private set;}
    public Vector2Int dir {get; private set;}

    public DungeonConnection(Vector2Int start, Vector2Int end, Vector2Int dir) {
        this.start = start;
        this.end = end;
        this.dir = dir;
    }
}
