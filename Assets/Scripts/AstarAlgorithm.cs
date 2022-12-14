using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AStarAlgorithm
{
    GridSystem grid;
    public AStarAlgorithm(GridSystem grid)
    {
        this.grid = grid;
    }
    public void FindTheWay(int startX, int startY, int destinationX, int destinationY)
    {
        Node targetNode = new Node(startX, startY, destinationX, destinationY, destinationX, destinationY);
        Node startNode = new Node(startX, startY, startX, startY, destinationX, destinationY);
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            
            var current = openList[0];
            foreach(Node currentNode in openList)
            {
                if (currentNode.fCost <= current.fCost && currentNode.hCost < current.hCost)
                {
                    current = currentNode;
                }
            }
            openList.Remove(current);
            closedList.Add(current);
            if (current.currentX == destinationX && current.currentY == destinationY)
            {
                targetNode = current;
                RetracePath(startNode, targetNode);
                if (grid.isDebugOn && grid.isImageOn)
                {
                    grid.RenderingDebugImagesPath(startNode);
                }
                if (grid.isDebugOn && grid.isLineOn)
                {
                    grid.RenderingDebugTMPsPath(startNode);
                }
                return;
            }
            foreach (Node neighbour in GetNeighbours(current))
            {
                if (closedList.Contains(neighbour) == true)
                {
                    continue;
                }
                int newMovementCostToNeighbour = current.gCost + GetDistance(current, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || openList.Contains(neighbour) == false)
                {
                    
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = current;
                    if (openList.Contains(neighbour) == false)
                    {
                        openList.Add(neighbour);
                    }
                }
            }
        }
    }
    public void FindTheWayUsingQueue(int startX, int startY, int destinationX, int destinationY)
    {
        Node targetNode = new Node(startX, startY, destinationX, destinationY, destinationX, destinationY);
        Node startNode = new Node(startX, startY, startX, startY, destinationX, destinationY);
        Heap<Node> priorityQueue = new Heap<Node>(grid.width * grid.Height);
        HashSet<Node> closedList = new HashSet<Node>();
        priorityQueue.Add(startNode);

        while (priorityQueue.Count > 0)
        {

            var current = priorityQueue.RemoveFirst();
            closedList.Add(current);
            if (current.currentX == targetNode.currentX && current.currentY == targetNode.currentY)
            {
                targetNode = current;
                RetracePath(startNode, targetNode);
                if (grid.isDebugOn && grid.isImageOn)
                {
                    grid.RenderingDebugImagesPath(startNode);
                }
                if (grid.isDebugOn && grid.isLineOn)
                {
                    grid.RenderingDebugTMPsPath(startNode);
                }
                return;
            }
            foreach (Node neighbour in GetNeighbours(current))
            {
                if (closedList.Contains(neighbour) == true)
                {
                    continue;
                }
                int newMovementCostToNeighbour = current.gCost + GetDistance(current, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || priorityQueue.Contains(neighbour) == false)
                {

                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = current;
                    if (priorityQueue.Contains(neighbour) == false)
                    {
                        priorityQueue.Add(neighbour);
                    }
                }
            }
        }
    }
    private List<Node> GetNeighbours(Node currentNode)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                int checkX = currentNode.currentX + x;
                int checkY = currentNode.currentY + y;
                if (checkX >= 0 && checkX < grid.width && checkY >= 0 && checkY < grid.Height && grid.gridArray[checkX,checkY].traversable)
                {
                    grid.debugingPathList.Add(grid.gridArray[checkX, checkY]);
                    neighbours.Add(grid.gridArray[checkX, checkY]);

                }
            }
        }
        return neighbours;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.currentX - nodeB.currentX);
        int distanceY = Mathf.Abs(nodeA.currentY - nodeB.currentY);
        if (distanceX < distanceY)
        {
            return distanceX * 14 + (distanceY - distanceX) * 10;
        }
        return distanceY * 14 + (distanceX - distanceY) * 10;
    }

    public void GetStartPosition(out int startX, out int startY)
    {
        if (grid.playerTransform == null)
        {
            Debug.LogError("player object is not setted yet");
        }
        int x, y;
        grid.GetXY(Camera.main.WorldToScreenPoint(grid.playerTransform.position), out x, out y);
        startX = x;
        startY = y;
    }
    public void GetDestination(Vector3 mousePosition, out int destinationX, out int destinationY)
    {
        int x, y;
        grid.GetXY(mousePosition, out x, out y);
        destinationX = x;
        destinationY = y;
    }

    private void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        grid.path = path;
    }
}
