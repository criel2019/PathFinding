using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public int currentX;
    public int currentY;
    public int gCost;
    public int hCost;
    public int fCost {
        get { return gCost + hCost; }
    }
    int heapIndex;
    public int HeapIndex{ get{ return heapIndex; } set { heapIndex = value; } }

    public Node parent;
    public bool traversable;
    public Node(int startX, int startY, int currentX, int currentY, int destinationX, int destinationY)
    {
        this.hCost = GetDistance(currentX, currentY, destinationX, destinationY);
        this.traversable = true;
        this.currentX = currentX;
        this.currentY = currentY;

    }
    public int GetDistance(int startX, int startY, int destinationX, int destinationY)
    {
        int distanceX = Mathf.Abs(destinationX - startX);
        int distanceY = Mathf.Abs(destinationY - startY);
        if (distanceX < distanceY)
        {
            return distanceX * 14 + (distanceY - distanceX) * 10;
        }
        return distanceY * 14 + (distanceX - distanceY) * 10;
    }
    public void MarkTraveled()
    {
        traversable = false;
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }

}
