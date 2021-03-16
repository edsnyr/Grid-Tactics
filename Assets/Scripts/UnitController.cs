using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{

    public float moveTime = 0.3f;

    public enum Mode { OpenSelect, Pathfind, ActionSelect, SelectTarget, WeaponSelect };
    public Mode mode = Mode.OpenSelect;

    public MovementGrid movementGrid;
    public Pathfinder pathfinder;
    public PathDisplay pathDisplay;
    public ActionMenu actionMenu;

    public List<Unit> units;
    Unit selectedUnit;
    public List<Unit> unitsInAttackRange;

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
                /*
                foreach(Unit unit in unitsInAttackRange) {
                    if(mousePos == unit.position) {
                        Debug.Log("Open attack menu");
                        mode = Mode.ActionSelect;
                        pathDisplay.ClearRange();
                        actionMenu.gameObject.SetActive(true);
                        actionMenu.SetActionMenu(selectedUnit);
                        return;
                    }
                }
                */
                StartCoroutine(MoveUnit());
                prevMousePos = mousePos;
            } else if(mode == Mode.SelectTarget) {
                foreach(Unit unit in unitsInAttackRange) {
                    if(mousePos == unit.position) {
                        mode = Mode.WeaponSelect;
                        actionMenu.SetForecastMenu(selectedUnit, unit, GetDistance(selectedUnit, unit));

                        //StartCombat(unit);
                        return;
                    }
                }
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
        mode = Mode.ActionSelect;
        actionMenu.gameObject.SetActive(true);
        actionMenu.SetActionMenu(selectedUnit);
    }

    public void StartCombat(Unit attacker, Unit defender) {
        Debug.Log(attacker + " attacks " + defender);

        if(defender.health.ModifyCurrentValue(attacker.equippedWeapon.damage * -1) != 0) {
            int dist = GetDistance(attacker, defender);
            if(dist >= defender.equippedWeapon.minRange && dist <= defender.equippedWeapon.maxRange) {
                if(attacker.health.ModifyCurrentValue(defender.equippedWeapon.damage * -1) == 0) {
                    Debug.Log(attacker + " dies");
                    KillUnit(attacker);
                }
            }
        } else {
            Debug.Log(defender + " dies");
            KillUnit(defender);
        }


        actionMenu.gameObject.SetActive(false);
        pathDisplay.ClearRange();
        mode = Mode.OpenSelect;
    }

    private void KillUnit(Unit unit) {
        units.Remove(unit);
        unit.DestroyHealthDisplays();
        Destroy(unit.gameObject);
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
                //Debug.Log(i + " " + j + " " + dist);
                if(dist == 0 || dist > maxRange || dist < selectedUnit.minAttackRange) {
                    //Debug.Log("Not in range");
                    continue;
                } else {
                    Vector3Int attackingLocation = new Vector3Int(location.x + i, location.y + j, 0);
                    //Debug.Log(location);
                    //Debug.Log(attackingLocation);
                    
                    attackableTiles.Add(attackingLocation);
                }
            }
        }

        return attackableTiles;

    }

    public List<Unit> GetUnitsInAttackRange(List<Vector3Int> attackableTiles) {
        List<Unit> unitsInRange = new List<Unit>();
        foreach(Vector3Int location in attackableTiles) {
            if(movementGrid.GetMovementTile(location) != null) {
                foreach(Unit unit in units) {
                    if(unit.position == location && unit.teamNumber != selectedUnit.teamNumber) {
                        unitsInRange.Add(unit);
                        break;
                    }
                }
            }
        }
        return unitsInRange;
    }

    public void SetUnitsInAttackRange(List<Unit> units) {
        unitsInAttackRange = units;
    }

    public int GetDistance(Unit a, Unit b) {
        Vector3Int v = a.position - b.position;
        int dist = Mathf.Abs(v.x) + Mathf.Abs(v.y);
        return dist;
    }


}
