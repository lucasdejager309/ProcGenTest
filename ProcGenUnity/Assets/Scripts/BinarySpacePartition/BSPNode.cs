using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class BSPNode {
    public BSPNode parent {get; private set;}
    public void SetParent(BSPNode node) {parent = node;}

    public BSPNode[] children {get; private set;} = new BSPNode[2];

    public Vector2Int position {get; private set;}
    public void SetPosition(Vector2Int position) {this.position = position;}

    public Vector2Int size {get; private set;}

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
}
