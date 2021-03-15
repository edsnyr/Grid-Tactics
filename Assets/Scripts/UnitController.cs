using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{

    public float moveTime = 0.3f;

    public enum Mode { OpenSelect, Pathfind };
    public Mode mode = Mode.OpenSelect;

    public Pathfinder pathfinder;

    public List<Unit> units;
    Unit selectedUnit;

    public Vector3Int prevMousePos = new Vector3Int();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int mousePos = pathfinder.GetMousePosition();

        if(Input.GetMouseButtonDown(0)) {
            if(mode == Mode.OpenSelect) {
                foreach(Unit unit in units) {
                    if(unit.position == mousePos) {
                        mode = Mode.Pathfind;
                        selectedUnit = unit;
                        pathfinder.StartPathfind(mousePos, unit.maxMovement);
                    }
                }
            } else if(mode == Mode.Pathfind) {
                
                StartCoroutine(MoveUnit());
                prevMousePos = mousePos;
                
            }
        }

        //Find path while hovering
        if(mode == Mode.Pathfind) {
            if(mousePos != prevMousePos) {
                pathfinder.Pathfind(prevMousePos, mousePos);
                prevMousePos = mousePos;
                pathfinder.pathChanged = true;
            }
        }
    }

    private IEnumerator MoveUnit() {
        mode = Mode.OpenSelect;
        pathfinder.pathChanged = true;
        yield return StartCoroutine(selectedUnit.MoveOnPath(pathfinder.GetPath(), moveTime));
    }

    public Unit GetSelectedUnit() {
        return selectedUnit;
    }

    public Unit CheckForUnit(Vector3Int pos) {
        foreach(Unit unit in units) {
            if(unit.position == pos)
                return unit;
        }
        return null;
    }

    public List<Vector3Int> GetAttackableTiles(Vector3Int location) {
        List<Vector3Int> attackableTiles = new List<Vector3Int>();
        int maxRange = selectedUnit.maxAttackRange;
        for(int i = maxRange * -1; i <= maxRange; i++) {
            for(int j = maxRange * -1; j <= maxRange; j++) {
                int dist = Mathf.Abs(i) + Mathf.Abs(j);
                Debug.Log(i + " " + j + " " + dist);
                if(dist == 0 || dist > maxRange || dist < selectedUnit.minAttackRange) {
                    //Debug.Log("Not in range");
                    continue;
                } else {
                    Vector3Int attackingLocation = new Vector3Int(location.x + i, location.y + j, 0);
                    Debug.Log(location);
                    Debug.Log(attackingLocation);
                    
                    attackableTiles.Add(attackingLocation);
                }
            }
        }
        return attackableTiles;

    }

}
