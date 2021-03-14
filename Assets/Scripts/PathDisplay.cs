using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class PathDisplay : MonoBehaviour
{

    enum RangeStatus { Movement, Attack, BlockedByAlly, BlockedByEnemy };

    public Pathfinder pathfinder;
    public MovementGrid movementGrid;
    public UnitController unitController;

    public Grid grid;
    public Tilemap arrowOverlayMap;
    public Tilemap rangeOverlayMap;
    public Tilemap pointerOverlayMap;
    public Tile hoverTile;

    public Tile arrowOne;
    public Tile arrowStart;
    public Tile arrowLine;
    public Tile arrowTurn;
    public Tile arrowEnd;

    public Tile movementTileHighlight;
    public Tile attackTileHighlight;

    bool showingRange = false;

    Vector3Int prevMousePos = new Vector3Int();

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(rangeOverlayMap.localBounds.min);
        Debug.Log(rangeOverlayMap.localBounds.max);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int mousePos = pathfinder.GetMousePosition();
        if(mousePos != prevMousePos) {
            pointerOverlayMap.SetTile(prevMousePos, null);
            pointerOverlayMap.SetTile(mousePos, hoverTile);
            prevMousePos = mousePos;
        }
        if(unitController.mode == UnitController.Mode.OpenSelect) {
            if(pathfinder.pathChanged == true) {
                arrowOverlayMap.ClearAllTiles();
                rangeOverlayMap.ClearAllTiles();
                pathfinder.pathChanged = false;
                showingRange = false;
            }
            
        }
        else if(unitController.mode == UnitController.Mode.Pathfind) {
            if(pathfinder.pathChanged) {
                DrawPath();
                pathfinder.pathChanged = false;
            }
            if(showingRange == false) {
                DrawRange();
            }
        }
    }

    private void DrawPath() {
        arrowOverlayMap.ClearAllTiles();
        List<Vector3Int> path = pathfinder.GetPath();

        if(path.Count == 1) {
            arrowOverlayMap.SetTile(path[0], arrowOne);
            return;
        }

        arrowOverlayMap.SetTile(path[path.Count - 1], arrowEnd);
        arrowOverlayMap.SetTransformMatrix(path[path.Count - 1], SetTileRotation(path[path.Count - 1], path[path.Count - 2]));

        for(int i = path.Count - 2; i > 0; i--) {
            arrowOverlayMap.SetTile(path[i], DecideTile(path[i+1], path[i-1]));
            arrowOverlayMap.SetTransformMatrix(path[i], SetTileRotation(path[i+1], path[i], path[i-1]));
        }

        arrowOverlayMap.SetTile(path[0], arrowStart);
        arrowOverlayMap.SetTransformMatrix(path[0], SetTileRotation(path[1], path[0]));
    }

    private Tile DecideTile(Vector3Int next, Vector3Int prev) {
        Vector3Int diff = next - prev;
        if(diff.x == 0 || diff.y == 0)
            return arrowLine;
        else
            return arrowTurn;
    }

    private Matrix4x4 SetTileRotation(Vector3Int current, Vector3Int prev) {
        float rotation = 0f;
        Vector3Int diff = current - prev;
        if(diff.y == 1)
            rotation = 0f;
        if(diff.x == -1)
            rotation = 90f;
        if(diff.y == -1)
            rotation = 180f;
        if(diff.x == 1)
            rotation = 270f;

        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, rotation), Vector3.one);
        return matrix;
    }

    private Matrix4x4 SetTileRotation(Vector3Int next, Vector3Int current, Vector3Int prev) {
        float rotation = 0f;
        Vector3Int diff = next - prev;

        //these two for straight line segments
        if(diff.x == 0) { //in a vertical line
            rotation = 0f;
        }
        else if(diff.y == 0) {//in a horizontal line
            rotation = 90f;
        }

        //these for corner pieces
        else if(diff.x == 1) {                          //next is right of prev
            if(diff.y == 1) {                           //next is above prev
                if(next.x - current.x == 0) {           //current on same x as next
                    rotation = 0f;                      //left-up pointing
                } else {                                //current on same x as prev
                    rotation = 180f;                    //down-right pointing
                }
            } else {                                    //next is below prev
                if(next.x - current.x == 0) {           //current on same x as next
                    rotation = 90f;                     //down-left pointing
                } else {                                //current on same x as prev
                    rotation = 270f;                    //up-right pointing
                }
            }
        } else {                                        //next is left of prev
            if(diff.y == 1) {                           //next is above prev
                if(next.x - current.x == 0) {           //current on same x as next
                    rotation = 270f;                    //up-right pointing
                } else {                                //current on same x as prev
                    rotation = 90f;                     //down-left pointing
                }
            } else {                                    //next is below prev
                if(next.x - current.x == 0) {           //current on same x as next
                    rotation = 180f;                    //down-right pointing
                } else {                                //current on same x as prev
                    rotation = 0f;                      //up-left pointing
                }
            }
        }

        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, rotation), Vector3.one);
        return matrix;
    }

    private void DrawRange() {
        rangeOverlayMap.ClearAllTiles();
        Vector3Int start = pathfinder.GetPath()[0];
        //Debug.Log("Draw Range Start: " + start);
        int length = pathfinder.maxLength;
        Dictionary<Vector3Int, RangeStatus> highlightStatuses = new Dictionary<Vector3Int, RangeStatus>();
        for(int i = length * -1; i <= length; i++) {
            for(int j = length * -1; j <= length; j++) {
                Vector3Int dest = new Vector3Int(start.x + i, start.y + j, 0);
                if((Mathf.Abs(i) + Mathf.Abs(j)) > length || unitController.CheckForUnit(dest) != null) { //if true, always out of range even if there would be a direct path
                    continue;
                }
                if(movementGrid.GetMovementTile(dest) != null) {
                    List<Vector3Int> testPath = pathfinder.AStar(start, dest, new List<Vector3Int> { start });
                    if(testPath == null) { //pathfinding escaped, no path
                        List<Vector3Int> attackingTiles = GetAttackableTiles(unitController.GetSelectedUnit().position);
                        foreach(Vector3Int location in attackingTiles) {
                            if(!highlightStatuses.ContainsKey(location)) {
                                Unit unitAtLocation = unitController.CheckForUnit(location);
                                if(unitAtLocation != null) {
                                    if(unitAtLocation.teamNumber == unitController.GetSelectedUnit().teamNumber) {
                                        //Debug.Log("Marked blocked by ally");
                                        highlightStatuses.Add(location, RangeStatus.BlockedByAlly);
                                    } else {
                                        //Debug.Log("Marked blocked by enemy");
                                        highlightStatuses.Add(location, RangeStatus.BlockedByEnemy);
                                        rangeOverlayMap.SetTile(location, attackTileHighlight);
                                    }
                                } else {
                                    //Debug.Log("Marked attacking");
                                    highlightStatuses.Add(location, RangeStatus.Attack);
                                    rangeOverlayMap.SetTile(location, attackTileHighlight);
                                }
                            }
                        }
                    } else if(testPath.Count > 0 && testPath[testPath.Count - 1] == dest) {
                        //Debug.Log("Getting attack highlights for: " + testPath[testPath.Count - 1]);
                        List<Vector3Int> attackingTiles = GetAttackableTiles(testPath[testPath.Count - 1]);
                        foreach(Vector3Int location in attackingTiles) {
                            //Debug.Log("Enter Attack Highlight for location: " + location);
                            if(!highlightStatuses.ContainsKey(location)) {
                                Unit unitAtLocation = unitController.CheckForUnit(location);
                                if(unitAtLocation != null) {
                                    if(unitAtLocation.teamNumber == unitController.GetSelectedUnit().teamNumber) {
                                        //Debug.Log("Marked blocked by ally");
                                        highlightStatuses.Add(location, RangeStatus.BlockedByAlly);
                                    } else {
                                        //Debug.Log("Marked blocked by enemy");
                                        highlightStatuses.Add(location, RangeStatus.BlockedByEnemy);
                                        rangeOverlayMap.SetTile(location, attackTileHighlight);
                                    }
                                } else {
                                    //Debug.Log("Marked attacking");
                                    highlightStatuses.Add(location, RangeStatus.Attack);
                                    rangeOverlayMap.SetTile(location, attackTileHighlight);
                                }
                            }
                        }

                        foreach(Vector3Int location in testPath) {
                            if(highlightStatuses.ContainsKey(location)) {
                                if(highlightStatuses[location] == RangeStatus.Movement || highlightStatuses[location] == RangeStatus.BlockedByAlly || highlightStatuses[location] == RangeStatus.BlockedByEnemy) {
                                    continue;
                                } else {
                                    highlightStatuses[location] = RangeStatus.Movement;
                                    rangeOverlayMap.SetTile(location, movementTileHighlight);
                                }
                            } else if(unitController.CheckForUnit(location) != null) {
                                if(unitController.CheckForUnit(location).teamNumber == unitController.GetSelectedUnit().teamNumber) {
                                    highlightStatuses.Add(location, RangeStatus.BlockedByAlly);
                                }
                            } else {
                                highlightStatuses.Add(location, RangeStatus.Movement);
                                rangeOverlayMap.SetTile(location, movementTileHighlight);
                            }
                        }
                    }
                }
            }
        }
        showingRange = true;
    }

    private List<Vector3Int> GetAttackableTiles(Vector3Int location) {
        List<Vector3Int> attackableTiles = new List<Vector3Int>();
        int maxRange = unitController.GetSelectedUnit().maxAttackRange;
        for(int i = maxRange * -1; i <= maxRange; i++) {
            for(int j = maxRange * -1; j <= maxRange; j++) {
                int dist = Mathf.Abs(i) + Mathf.Abs(j);
                Debug.Log(i + " " + j + " " + dist);
                if(dist == 0 || dist > maxRange || dist < unitController.GetSelectedUnit().minAttackRange) {
                    //Debug.Log("Not in range");
                    continue;
                } else {
                    Vector3Int attackingLocation = new Vector3Int(location.x + i, location.y + j, 0);
                    Debug.Log(location);
                    Debug.Log(attackingLocation);
                    if(movementGrid.GetMovementTile(attackingLocation) != null) {
                        attackableTiles.Add(attackingLocation);
                    }
                }
            }
        }
        //Debug.Log("Number of attacking tiles: " + attackableTiles.Count);
        return attackableTiles;
        /*
        for(int i = unitController.GetSelectedUnit().minAttackRange; i <= unitController.GetSelectedUnit().maxAttackRange; i++) {
            foreach(Vector3Int adjacent in adjacents) {
                Vector3Int checkLocation = location + (adjacent * i);

            }
        }
        */
        
    }
}