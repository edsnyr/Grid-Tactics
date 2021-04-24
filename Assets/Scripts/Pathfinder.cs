using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour {

    public UnitController unitController;

    public bool pathChanged = false;

    public Grid grid;
    public Tilemap movementOverlayMap;
    public MovementGrid movementGrid;

    public int maxLength = 5;

    List<Vector3Int> path;

    Vector3Int lastValidEndPoint;
    

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void StartPathfind(Vector3Int mousePos, int maxMovement) {
        path = new List<Vector3Int>();
        path.Add(GetMousePosition());
        lastValidEndPoint = path[0];
        unitController.prevMousePos = mousePos;
        maxLength = maxMovement;
        pathChanged = true;
    }

    public void Pathfind(Vector3Int prevPos, Vector3Int destPos) {
        if(destPos == path[0]) {
            path = new List<Vector3Int> {
                destPos
            };
            lastValidEndPoint = path[0];
            pathChanged = true;
            return;
        }

        bool go = true; //determines if the path can continue

        CheckForOverlap(ref go, destPos);

        if(go) {
            List<Vector3Int> newPath = AStar(path[path.Count -1], destPos, path);
            if(newPath == null) {
                Debug.Log("Out of range, escaping");
                return;
            }
            if(newPath.Count > 0 && newPath[newPath.Count - 1] != destPos) {
                Vector3Int startPos = path[0];
                path = new List<Vector3Int> {
                    startPos
                };
                List<Vector3Int> tryPath = AStar(path[0], destPos, path);
                if(tryPath.Count != 0 && tryPath[tryPath.Count - 1] == destPos) {
                    path = tryPath;
                } else {
                    path = newPath;
                }
            } else {
                path = newPath;
            }
            lastValidEndPoint = path[path.Count - 1];
        }        
    }


    public List<Vector3Int> AStar(Vector3Int startPos, Vector3Int destPos, List<Vector3Int> currentPath) {
        MovementTile startTile = movementGrid.GetMovementTile(startPos);
        MovementTile endTile = movementGrid.GetMovementTile(destPos);

        if(startTile == null || endTile == null) {
            Debug.Log("Start or end point invalid");
            Debug.Log("startPos: " + startPos);
            Debug.Log("destPos: " + destPos);
            return null;
        }

        List<MovementTile> openTiles = new List<MovementTile>();
        List<MovementTile> closedTiles = new List<MovementTile>();

        openTiles.Add(startTile);

        while(openTiles.Count > 0) {

            MovementTile currentTile = openTiles[0];
            for(int i = 1; i < openTiles.Count; i++) {
                if(openTiles[i].fCost < currentTile.fCost || (openTiles[i].fCost == currentTile.fCost && openTiles[i].hCost < currentTile.hCost)) {
                    currentTile = openTiles[i];
                }
            }

            openTiles.Remove(currentTile);
            closedTiles.Add(currentTile);

            if(currentTile == endTile) {
                return AddToPath(RetracePath(startTile, endTile), maxLength, currentPath);
            }

            foreach(MovementTile neighbor in currentTile.neighbors) {
                Unit unitAtNeighbor = unitController.CheckForUnit(neighbor.coordinates);
                if((unitAtNeighbor != null && unitAtNeighbor.teamNumber != unitController.GetSelectedUnit().teamNumber)) {
                    closedTiles.Add(neighbor);
                    continue;
                }
                if(closedTiles.Contains(neighbor)) {
                    continue;
                }

                int newCostToNeighbor = currentTile.gCost + neighbor.movementCost;
                if(newCostToNeighbor < neighbor.gCost || !openTiles.Contains(neighbor)) {
                    neighbor.gCost = newCostToNeighbor;
                    neighbor.hCost = GetHeuristicDistance(neighbor, endTile);
                    neighbor.parent = currentTile;

                    if(!openTiles.Contains(neighbor)) {
                        openTiles.Add(neighbor);
                    }
                }
            }

        }
        Debug.Log("Escaped");
        return null;
    }

    private int GetHeuristicDistance(MovementTile neighbor, MovementTile endTile) {
        int distX = Mathf.Abs(neighbor.coordinates.x - endTile.coordinates.x);
        int distY = Mathf.Abs(neighbor.coordinates.y - endTile.coordinates.y);

        if(distX > distY) {
            return Mathf.RoundToInt(1.4f * distY + (distX - distY));
        } else {
            return Mathf.RoundToInt(1.4f * distX + (distY - distX));
        }
    }

    private List<Vector3Int> RetracePath(MovementTile startTile, MovementTile endTile) {
        List<Vector3Int> path = new List<Vector3Int>();
        MovementTile currentTile = endTile;

        while(currentTile != startTile) {
            path.Add(currentTile.coordinates);
            currentTile = currentTile.parent;
        }
        path.Reverse();
        return path;
    }

    private List<Vector3Int> AddToPath(List<Vector3Int> addedPath, int maxPathLength, List<Vector3Int> currentPath) {
        int currentLength = GetCurrentPathLength(currentPath);
        
        if(unitController.CheckForUnit(addedPath[addedPath.Count - 1]) != null) {
            Debug.Log("Cannot stop on another unit");
            currentPath = ReturnToLastValidEndPoint(currentPath);
            return currentPath;
        }

        foreach(Vector3Int tile in addedPath) {
            int newLength = currentLength + movementGrid.GetMovementTile(tile).movementCost;
            if(newLength <= maxPathLength) {
                currentPath.Add(tile);
                currentLength = newLength;
            } else {
                currentPath = ReturnToLastValidEndPoint(currentPath);
                return currentPath;
            }
        }
        return currentPath;
    }

    private int GetCurrentPathLength(List<Vector3Int> currentPath) {
        int currentLength = 0;
        for(int i = 1; i < currentPath.Count; i++) {
            currentLength += movementGrid.GetMovementTile(currentPath[i]).movementCost;
        }
        return currentLength;
    }

    private List<Vector3Int> ReturnToLastValidEndPoint(List<Vector3Int> currentPath) {
        for(int i = currentPath.Count - 1; i >= 0; i--) {
            if(currentPath[i] != lastValidEndPoint) {
                currentPath.RemoveAt(i);
            } else
                return currentPath;
        }
        return currentPath;
    }

    private void CheckForOverlap(ref bool go, Vector3Int newPos) {
        for(int i = path.Count - 1; i > 0; i--) {
            if(path[i] == newPos) { //remove path after this point
                for(int j = path.Count - 1; j > i; j--) {
                    path.RemoveAt(j);
                }
                if(unitController.CheckForUnit(path[path.Count - 1]) != null) {
                    Debug.Log("Unit on retreaded path, back up one more");
                    path.RemoveAt(path.Count - 1);
                }
                lastValidEndPoint = path[path.Count - 1];
                go = false;
                return;
            }
        }
    }    

    public List<Vector3Int> GetPath() {
        return path;
    }

    public Vector3Int GetMousePosition() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return grid.WorldToCell(mouseWorldPos);
    }
}
