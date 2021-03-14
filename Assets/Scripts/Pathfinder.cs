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

    public Tile movementCost1;
    public Tile movementCost2;
    public Tile movementCost9;

    public int maxLength = 5;

    List<Vector3Int> path;

    Vector3Int lastValidEndPoint;
    

    // Start is called before the first frame update
    void Start() {
        Debug.Log(movementOverlayMap.localBounds.min);
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void StartPathfind(Vector3Int mousePos) {
        path = new List<Vector3Int>();
        path.Add(GetMousePosition());
        lastValidEndPoint = path[0];
        unitController.prevMousePos = mousePos;
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

        /*
        Vector3Int nextPos = prevPos;
        while(go && nextPos.x != destPos.x) {
            nextPos += new Vector3Int((int)Mathf.Sign(destPos.x - prevPos.x), 0, 0);
            AddToPath(ref go, nextPos);
        }
        while(go && nextPos.y != destPos.y) {
            nextPos += new Vector3Int(0, (int)Mathf.Sign(destPos.y - prevPos.y), 0);
            AddToPath(ref go, nextPos);
        }
        */

        if(go) {
            AStar(path[path.Count -1], destPos, ref go);
        }

        /*
        if(!go) { //determine why the path cannot continue
            Vector3Int diff = destPos - path[0]; //distance from start of path
            if((Mathf.Abs(diff.x) + Mathf.Abs(diff.y)) > maxLength) {
                Debug.Log("Position out of range");
            } else {
                Debug.Log("Out of length, recalculating");
                Vector3Int start = path[0];
                path = new List<Vector3Int>();
                path.Add(start);
                //Pathfind(start, destPos);
                AStar(start, destPos, ref go);
            }

        }
        */
        
    }


    private void AStar(Vector3Int startPos, Vector3Int destPos, ref bool go) {
        MovementTile startTile = movementGrid.GetMovementTile(startPos);
        MovementTile endTile = movementGrid.GetMovementTile(destPos);

        if(startTile == null || endTile == null) {
            return;
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
                AddToPath(RetracePath(startTile, endTile), maxLength, ref go);
                return;
            }

            foreach(MovementTile neighbor in currentTile.neighbors) {
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

    private void AddToPath(List<Vector3Int> addedPath, int maxPathLength, ref bool go) {
        int currentLength = GetCurrentPathLength();
        Debug.Log("Current Length before loop: " + currentLength);
        
        foreach(Vector3Int tile in addedPath) {
            int newLength = currentLength + movementGrid.GetMovementTile(tile).movementCost;
            Debug.Log("New Length: " + newLength);
            Debug.Log("Path Count: " + path.Count);
            Debug.Log(tile);
            if(newLength <= maxPathLength) {
                path.Add(tile);
                currentLength = newLength;
            } else {
                Debug.Log("Path too long");
                go = false;
                ReturnToLastValidEndPoint();
                return;
            }
        }
        lastValidEndPoint = path[path.Count - 1];
    }

    private int GetCurrentPathLength() {
        int currentLength = 0;
        for(int i = 1; i < path.Count; i++) {
            currentLength += movementGrid.GetMovementTile(path[i]).movementCost;
        }
        return currentLength;
    }

    private void ReturnToLastValidEndPoint() {
        for(int i = path.Count - 1; i >= 0; i--) {
            if(path[i] != lastValidEndPoint) {
                path.RemoveAt(i);
            } else
                return;
        }
    }

    private void CheckForOverlap(ref bool go, Vector3Int newPos) {
        for(int i = path.Count - 1; i > 0; i--) {
            if(path[i] == newPos) { //remove path after this point
                for(int j = path.Count - 1; j > i; j--) {
                    path.RemoveAt(j);
                }
                Debug.Log("Retread current path, backtracking");
                lastValidEndPoint = path[path.Count - 1];
                go = false;
                return;
            }
        }
    }

    private void AddToPath(ref bool go, Vector3Int nextPos) {
        if(path.Count > maxLength) {
            go = false;
            return;
        }
        Debug.Log(movementOverlayMap.GetTile(nextPos));
        path.Add(nextPos);
    }

    

    public List<Vector3Int> GetPath() {
        return path;
    }

    public Vector3Int GetMousePosition() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return grid.WorldToCell(mouseWorldPos);
    }
}
