using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BSPRenderer : MonoBehaviour
{
    public TileBase[] palette;
    [SerializeField] int iterations = 4;
    [SerializeField] Vector2Int size = new Vector2Int(100, 100);
    [SerializeField] [Range(1, 3)]float maxWidthHeightFactor = 1.5f;
    [SerializeField] [Range(0, 0.5f)] float partitionVariation = 0.125f;

    Grid grid;
    List<GameObject> partitionmaps = new List<GameObject>();

    void Start() {
        grid = GetComponentInChildren<Grid>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Reset();
            BSPTree tree = BSPTree.NewTree(iterations, size, maxWidthHeightFactor, partitionVariation);
            DrawPartitions(tree.leafs);

            Debug.Log(tree.nodes[1].position + " " + tree.nodes[1].GetSibling().position + " " + tree.nodes[1].GetSibling().size);
        }
    }

    void Reset() {
        foreach (GameObject obj in partitionmaps) {
            Destroy(obj);
        }
        partitionmaps.Clear();
    }

    void DrawPartitions(List<BSPNode> partitons) {
        foreach (BSPNode node in partitons) {
            GameObject newMap = new GameObject();
            partitionmaps.Add(newMap);
            newMap.transform.parent = grid.gameObject.transform;
            newMap.AddComponent<Tilemap>();
            newMap.AddComponent<TilemapRenderer>();
            newMap.name = node.position.ToString();

            for (int x = node.position.x+1; x < node.position.x+node.size.x-1; x++) {
                for (int y = node.position.y+1; y < node.position.y+node.size.y-1; y++) {
                    newMap.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), palette[0]);
                }
            }
        }
    }

}
