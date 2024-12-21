using System.Collections.Generic;
using UnityEngine;

public class BSPTree {
    public BSPNode root;
    public List<BSPNode> leafs {get; private set;} = new List<BSPNode>(); //only end nodes
    public List<BSPNode> nodes {get; private set;} = new List<BSPNode>(); //all nodes

    static public BSPTree NewTree(int iterations, Vector2Int size, float maxWidthHeightFactor, float partitionVariation) {
        BSPTree tree = new BSPTree();
        tree.root = new BSPNode(null, size);
        tree.leafs.Add(tree.root);
        tree.nodes.Add(tree.root);

        int currentIterations = 0;

        while (currentIterations < iterations) {
            List<BSPNode> leafsToSplit = new List<BSPNode>(tree.leafs);
            tree.leafs.Clear();

            foreach(BSPNode leaf in leafsToSplit) {
                int orientation;  //0 is vertical split (along x axis), 1 is horizontal split (along y axis)

                //if area is 2x as wide as it is high partition along x axis and vice versa to prevent undesiribly narrow partitions
                if (leaf.size.x >= leaf.size.y*maxWidthHeightFactor) {
                    orientation = 0;
                } else if (leaf.size.y >= leaf.size.x*maxWidthHeightFactor) {
                    orientation = 1;
                }
                else {
                    orientation = Mathf.RoundToInt(Random.value);
                }

                int splitPosition;
                if (orientation == 0) {
                    splitPosition = Mathf.RoundToInt(leaf.size.x*(0.5f+Random.Range(-partitionVariation, partitionVariation))); 
                }
                else {
                    splitPosition = Mathf.RoundToInt(leaf.size.y*(0.5f+Random.Range(-partitionVariation, partitionVariation)));
                }
                

                BSPNode[] newLeafs = leaf.Split(orientation, splitPosition);
                foreach (BSPNode newLeaf in newLeafs) {
                    tree.leafs.Add(newLeaf);
                    tree.nodes.Add(newLeaf);
                }
            }

            currentIterations++;
        }

        return tree;
    }
}