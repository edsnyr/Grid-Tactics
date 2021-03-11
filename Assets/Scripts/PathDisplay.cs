using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class PathDisplay : MonoBehaviour
{

    public Pathfinder pathfinder;

    public Grid grid;
    public Tilemap arrowOverlayMap;
    public Tilemap rangeOverlayMap;
    public Tile hoverTile;

    public Tile arrowOne;
    public Tile arrowStart;
    public Tile arrowLine;
    public Tile arrowTurn;
    public Tile arrowEnd;

    public Tile tileHighlight;

    bool showingRange = false;

    Vector3Int prevMousePos = new Vector3Int();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(pathfinder.mode == Pathfinder.Mode.OpenSelect) {
            if(pathfinder.pathChanged == true) {
                arrowOverlayMap.ClearAllTiles();
                rangeOverlayMap.ClearAllTiles();
                pathfinder.pathChanged = false;
                showingRange = false;
            }
            Vector3Int mousePos = pathfinder.GetMousePosition();
            if(mousePos != prevMousePos) {
                Debug.Log(mousePos);
                arrowOverlayMap.SetTile(prevMousePos, null);
                arrowOverlayMap.SetTile(mousePos, hoverTile);
                prevMousePos = mousePos;
            }
        }
        else if(pathfinder.mode == Pathfinder.Mode.Pathfind) {
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
        int length = pathfinder.maxLength;
        for(int i = length * -1; i <= length; i++) {
            for(int j = length * -1; j <= length; j++) {
                if((Mathf.Abs(i) + Mathf.Abs(j)) <= length) {
                    rangeOverlayMap.SetTile(new Vector3Int(start.x + i, start.y + j, 0), tileHighlight);
                }
            }
        }
        showingRange = true;
    }
}