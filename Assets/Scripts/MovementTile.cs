using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTile
{

    public int gCost;
    public int hCost;

    public int fCost {
        get {
            return gCost + hCost;
        }
    }

    public Vector3Int coordinates;
    public int movementCost;

    public List<MovementTile> neighbors;
    public MovementTile parent;

    public MovementTile(Vector3Int cd, int cost) {
        coordinates = cd;
        movementCost = cost;
    }

}
