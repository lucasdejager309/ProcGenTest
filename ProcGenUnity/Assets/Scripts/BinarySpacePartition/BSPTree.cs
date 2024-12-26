using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BSPTree {
    public BSPNode root;
    public List<BSPNode> leafs {get; private set;} = new List<BSPNode>(); //only end nodes
    public List<BSPNode> nodes {get; private set;} = new List<BSPNode>(); //all nodes
    public Dictionary<KeyValuePair<BSPNode, BSPNode>, int> pairs = new Dictionary<KeyValuePair<BSPNode, BSPNode>, int>(); //siblings

    public BSPTree(int iterations, Vector2Int size, float maxWidthHeightFactor, float partitionVariation) {
        root = new BSPNode(null, size);
        root.SetDepth(0);
        leafs.Add(root);
        nodes.Add(root);

        int currentIterations = 0;

        while (currentIterations < iterations) {
            List<BSPNode> leafsToSplit = new List<BSPNode>(leafs);
            leafs.Clear();

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
                    newLeaf.SetDepth(currentIterations+1);
                    leafs.Add(newLeaf);
                    nodes.Add(newLeaf);
                }
            }

            currentIterations++;
        }

        pairs = GetSiblingPairs(nodes);
        foreach (BSPNode n in nodes) {
            n.GetLeafs();
        }
    }

    static public Dictionary<KeyValuePair<BSPNode, BSPNode>, int> GetSiblingPairs(List<BSPNode> nodes) {
        Dictionary<KeyValuePair<BSPNode, BSPNode>, int> connections = new Dictionary<KeyValuePair<BSPNode, BSPNode>, int>();

        foreach(BSPNode node in nodes) {
            if (node.children[0] != null) {
                connections.Add(new KeyValuePair<BSPNode, BSPNode>(node.children[0], node.children[1]), node.depth);
            }
        }

        return connections;
    }
}