using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementGrid : MonoBehaviour
{

    public Tilemap movementOverlayMap;

    public TileBase movementTile1;
    public TileBase movementTile2;
    public TileBase movementTile9;

    Dictionary<Vector3Int, MovementTile> movementTiles;

    List<Vector3Int> adjacents = new List<Vector3Int> {
        new Vector3Int(0,1,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(0,-1,0),
        new Vector3Int(1,0,0)
    };

    private void Awake() {
        movementTiles = new Dictionary<Vector3Int, MovementTile>();
        BuildMovementMap();
        GetNeighbors();
    }

    private void BuildMovementMap() {
        for(int i = (int)movementOverlayMap.localBounds.min.x; i < movementOverlayMap.localBounds.max.x; i++) {
            for(int j = (int)movementOverlayMap.localBounds.min.y; j < movementOverlayMap.localBounds.max.y; j++) {
                TileBase tile = movementOverlayMap.GetTile(new Vector3Int(i, j, 0));
                Vector3Int coordinates = new Vector3Int(i, j, 0);
                if(tile == null) {
                    Debug.Log("Null tile at: " + coordinates);
                } else {
                    int cost = 1;
                    if(tile == movementTile1) {
                        cost = 1;
                    }
                    if(tile == movementTile2) {
                        cost = 2;
                    }
                    if(tile == movementTile9) {
                        cost = 9;
                    }
                    movementTiles.Add(coordinates, new MovementTile(coordinates, cost));
                }
            }
        }
    }

    private void GetNeighbors() {
        for(int i = (int)movementOverlayMap.localBounds.min.x; i < movementOverlayMap.localBounds.max.x; i++) {
            for(int j = (int)movementOverlayMap.localBounds.min.y; j < movementOverlayMap.localBounds.max.y; j++) {
                Vector3Int coordinates = new Vector3Int(i, j, 0);
                if(movementTiles.ContainsKey(coordinates)) {
                    List<MovementTile> neighbors = new List<MovementTile>();
                    foreach(Vector3Int adjacent in adjacents) {
                        if(movementTiles.ContainsKey(coordinates + adjacent)) {
                            neighbors.Add(movementTiles[coordinates + adjacent]);
                        }
                    }
                    movementTiles[coordinates].neighbors = neighbors;
                }
            }
        }
    }

    public MovementTile GetMovementTile(Vector3Int cd) {
        if(movementTiles.ContainsKey(cd))
            return movementTiles[cd];
        return null;
    }
}
