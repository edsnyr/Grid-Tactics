using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour {

    public enum Mode { OpenSelect, Pathfind };
    public Mode mode = Mode.OpenSelect;

    public bool pathChanged = false;

    public Grid grid;

    public int maxLength = 5;

    List<Vector3Int> path;
    Vector3Int prevMousePos = new Vector3Int();

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        Vector3Int mousePos = GetMousePosition();

        if(Input.GetMouseButtonDown(0)) {
            if(mode == Mode.OpenSelect) {
                if(Input.GetMouseButtonDown(0)) {
                    mode = Mode.Pathfind;
                    path = new List<Vector3Int>();
                    path.Add(GetMousePosition());
                    prevMousePos = mousePos;
                    pathChanged = true;
                }
            } else if(mode == Mode.Pathfind) {
                mode = Mode.OpenSelect;
                prevMousePos = mousePos;
                pathChanged = true;
            }
        }


        if(mode == Mode.Pathfind) {
            if(mousePos != prevMousePos) {
                Pathfind(prevMousePos, mousePos);
                prevMousePos = mousePos;
            }
        }
    }

    public void Pathfind(Vector3Int prevPos, Vector3Int newPos) {

        if(newPos == path[0]) {
            path = new List<Vector3Int>();
            path.Add(newPos);
            pathChanged = true;
            return;
        }

        bool go = true; //determines if the path can continue

        CheckForOverlap(ref go, newPos);

        Vector3Int nextPos = prevPos;
        while(go && nextPos.x != newPos.x) {
            nextPos += new Vector3Int((int)Mathf.Sign(newPos.x - prevPos.x), 0, 0);
            AddToPath(ref go, nextPos);
        }
        while(go && nextPos.y != newPos.y) {
            nextPos += new Vector3Int(0, (int)Mathf.Sign(newPos.y - prevPos.y), 0);
            AddToPath(ref go, nextPos);
        }

        if(!go) { //determine why the path cannot continue
            Vector3Int diff = newPos - path[0]; //distance from start of path
            if((Mathf.Abs(diff.x) + Mathf.Abs(diff.y)) > maxLength) {
                Debug.Log("Position out of range");
            } else {
                Debug.Log("Out of length, recalculating");
                Vector3Int start = path[0];
                path = new List<Vector3Int>();
                path.Add(start);
                Pathfind(start, newPos);
            }

        }

        pathChanged = true;
    }

    private void CheckForOverlap(ref bool go, Vector3Int newPos) {
        for(int i = path.Count - 1; i > 0; i--) {
            if(path[i] == newPos) { //remove path after this point
                for(int j = path.Count - 1; j > i; j--) {
                    path.RemoveAt(j);
                }
                Debug.Log("Retread current path, backtracking");
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
        path.Add(nextPos);
    }

    public Vector3Int GetMousePosition() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return grid.WorldToCell(mouseWorldPos);
    }

    public List<Vector3Int> GetPath() {
        return path;
    }
}
