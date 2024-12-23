using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class BSPNode {
    public BSPNode parent {get; private set;} = null;
    public void SetParent(BSPNode node) {parent = node;}

    public BSPNode[] children {get; private set;} = new BSPNode[2];
    public List<BSPNode> leafs {get; private set;} = new List<BSPNode>();

    public Vector2Int position {get; private set;}
    public void SetPosition(Vector2Int position) {this.position = position;}

    public Vector2Int size {get; private set;}

    public int depth {get; private set;}
    public void SetDepth(int d) {depth = d;}
    
    public BSPNode(BSPNode parent, Vector2Int size) {
        this.parent = parent;
        this.size = size;
    }

    public BSPNode GetSibling() {
        if (parent == null) return null;
        else {
            if (parent.children[0] != this) return parent.children[0];
            else return parent.children[1];
        }
    }

    public List<BSPNode> GetLineage(int depth = int.MaxValue) {
        List<BSPNode> lineage = new List<BSPNode>();

        BSPNode currentNode = this;
        for (int i = 0; i < depth; i++) {
            if (currentNode.parent == null) break;

            lineage.Add(currentNode.parent);
            currentNode = currentNode.parent;
        }

        return lineage;
    }

    public void GetLeafs() {

        Queue<BSPNode> nodesToCheck = new Queue<BSPNode>();
        nodesToCheck.Enqueue(this);

        while (nodesToCheck.Count > 0) {
            BSPNode current = nodesToCheck.Dequeue();
            if (current.children[0] != null) {
                foreach (BSPNode child in current.children) {
                    nodesToCheck.Enqueue(child);
                }
            } else {
                leafs.Add(current);
            }
        }
    }


    public BSPNode[] Split(int orientation, int splitPos) {
        
        if (orientation == 0) {
            //split along x axis
            children[0] = new BSPNode(this, new Vector2Int(splitPos, size.y));
            children[0].position = new Vector2Int(position.x, position.y);

            children[1] = new BSPNode(this, new Vector2Int(size.x-splitPos, size.y));
            children[1].position = new Vector2Int(position.x+splitPos, position.y);

        } else {
            //split along y axis
            children[0] = new BSPNode(this, new Vector2Int(size.x, splitPos));
            children[0].position = new Vector2Int(position.x, position.y);
            
            children[1] = new BSPNode(this, new Vector2Int(size.x, size.y-splitPos));
            children[1].position = new Vector2Int(position.x, position.y+splitPos);
        }

        return children;
    }

    public static Vector2Int GetDirection(BSPNode node1, BSPNode node2) {
        Vector2Int dir = new Vector2Int();
        
        if ( node2.position.y == node1.position.y) {
            if (node2.position.x > node1.position.x) dir = new Vector2Int(1,0);
            else if (node2.position.x < node1.position.x) dir = new Vector2Int(-1,0);
        } else {
            if (node2.position.y > node1.position.y) {
                dir = new Vector2Int(0,1);
            }
            else dir = new Vector2Int(0, -1);
        }

        return dir;
    }

    public static List<BSPNode> GetLeafsFacing(BSPNode node, Vector2Int dir) {
        List<BSPNode> leafsToReturn = new List<BSPNode>();

        foreach (BSPNode leaf in node.leafs) {
            bool facing = false;
            
            switch(dir.x) {
                case 1:
                    
                    if (leaf.position.x+leaf.size.x == node.position.x+node.size.x) facing = true;
                        
                    break;
                case -1:
                    
                    if (leaf.position.x == node.position.x) facing = true;
                    
                    break;
            }
            
            switch(dir.y) {
                case 1:
                    
                    if (leaf.position.y+leaf.size.y == node.position.y+node.size.y) facing = true;
                    
                    break;
                case -1:

                    if (leaf.position.y == node.position.y) facing = true;

                    break;
            }

            if (facing == true) leafsToReturn.Add(leaf);
        }

        return leafsToReturn;
    }
}
