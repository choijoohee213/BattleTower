using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class AStar {
    private static Dictionary<Point, Node> nodes;

    static void CreateNodes() {
        nodes = new Dictionary<Point, Node>();

        foreach (TileScript tile in LevelManager.Instance.Tiles.Values) {
            nodes.Add(tile.GridPosition, new Node(tile));

        }
    }

    public static Stack<Node> GetPath(Point start, Point goal) {
        if (nodes == null) {
            CreateNodes();
        }

        HashSet<Node> openList = new HashSet<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Stack<Node> finalPath = new Stack<Node>();
        Node currentNode = nodes[start];

        //#1. Adds the start node to the OpenList
        openList.Add(currentNode);

        while (openList.Count > 0) {
            //#2. Runs through all neighbors
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    Point neighbourPos = new Point(currentNode.GridPosition.x - x, currentNode.GridPosition.y - y);
                    
                    if (LevelManager.Instance.InBounds(neighbourPos) && LevelManager.Instance.Tiles[neighbourPos].tileIndex.Equals(1) && neighbourPos != currentNode.GridPosition) {
                        int gCost = 0;
                        if (Math.Abs(x - y).Equals(1))
                            gCost = 10;
                        else {  //Scores 14 if we are diagonal
                            if (!ConnectedDiagonally(currentNode, nodes[neighbourPos]))
                                continue;
                            gCost = 14;
                        }


                        //#3. Adds the neighbor to the open list
                        Node neighbour = nodes[neighbourPos];

                        if (openList.Contains(neighbour)) {
                            if (currentNode.G + gCost < neighbour.G) {
                                neighbour.CalcValues(currentNode, nodes[goal], gCost);
                            }
                        }

                        else if (!closedList.Contains(neighbour)) {
                            openList.Add(neighbour);
                            neighbour.CalcValues(currentNode, nodes[goal], gCost);
                        }
                    }
                }
            }

            //#5. Moves the current node from the open list to closed list
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (openList.Count > 0) {
                //sorts the list by F value, and selects the first on the list
                currentNode = openList.OrderBy(n => n.F).First();
            }

            //#5. Reach the goal node, Push nodes in the stack from goal to start node.
            if (currentNode.Equals(nodes[goal])) {
                while (currentNode.GridPosition != start) {
                    finalPath.Push(currentNode);
                    currentNode = currentNode.Parent;
                }
                break;
            }

        }
        return finalPath;
    }


    static bool ConnectedDiagonally(Node currentNode, Node neighbour) {
        return nodes[new Point(currentNode.GridPosition.x, neighbour.GridPosition.y)].TileRef.tileIndex.Equals(1) 
            && nodes[new Point(neighbour.GridPosition.x, currentNode.GridPosition.y)].TileRef.tileIndex.Equals(1);
    }

}
