using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {
    public Point GridPosition { get; private set; }

    public Vector2 WorldPosition { get; set; }

    public TileScript TileRef { get; private set; }

    public Node Parent { get; private set; }
    
    public int G { get; set; }
    public int H { get; set; }
    public int F { get; set; }


    public Node(TileScript tileRef) {
        TileRef = tileRef;
        GridPosition = tileRef.GridPosition;
        WorldPosition = tileRef.WorldPostion;
    }

    public void CalcValues(Node parent, Node goal, int gCost) {
        Parent = parent;
        G = parent.G + gCost;
        H = (Math.Abs(GridPosition.x - goal.GridPosition.x)+ Math.Abs(goal.GridPosition.y - GridPosition.y)) * 10;
        F = G + H;
    }

}
