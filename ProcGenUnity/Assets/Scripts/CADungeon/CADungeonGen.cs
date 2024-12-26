using System.Collections;
using System.Collections.Generic;
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

    public Dictionary<BSPNode, Area> areas {get; private set;} = new Dictionary<BSPNode, Area>();
    public BSPTree tree {get; private set;} = null;

    public void NewDungeon() {
        areas.Clear();
        tree = new BSPTree(iterations, size, maxWidthHeightFactor, partitionVariation);

        foreach (BSPNode leaf in tree.leafs) {
            CAGen area = new CAGen(leaf.size, neighboursRequired, fillPercent, steps, border, removeSmallRooms, largestSizeRequired, minlargestSizeRequired);
            areas.Add(leaf, area.largest[1]);
        }
    }
}
