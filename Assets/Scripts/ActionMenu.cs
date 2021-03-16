using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionMenu : MonoBehaviour
{

    public PathDisplay pathDisplay;
    public UnitController unitController;

    public Button attackButton;
    public Button waitButton;
    public Button backButton;

    public Button returnToMainButton;

    public Transform mainMenu;
    public Transform attackMenu;
    public Transform weaponMenu;

    public ResourceForcastDisplay attackerForecast;
    public ResourceForcastDisplay defenderForecast;
    public TextMeshProUGUI selectedWeaponText;
    public Button incrementWeaponList;
    public Button decrementWeaponList;

    Unit selectedUnit;
    Unit targetUnit;
    List<Vector3Int> attackableTiles;
    List<Weapon> usableWeapons;
    int usedWeaponIndex;

    // Start is called before the first frame update
    void Start()
    {
        SetMainMenu(true);
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActionMenu(Unit unit) {
        selectedUnit = unit;
        attackableTiles = unitController.GetAttackableTiles(selectedUnit.position);
        List<Unit> unitsInRange = unitController.GetUnitsInAttackRange(attackableTiles);
        unitController.SetUnitsInAttackRange(unitsInRange);
        attackButton.interactable = unitsInRange.Count > 0;
    }


    public void ShowAttackRange() {
        pathDisplay.DisplayCurrentAttackRange(attackableTiles);
        unitController.mode = UnitController.Mode.SelectTarget;
        SetAttackMenu(true);
    }

    public void Wait() {
        unitController.mode = UnitController.Mode.OpenSelect;
        gameObject.SetActive(false);
    }

    public void UndoMove() {
        if(selectedUnit != null) {
            selectedUnit.UndoMove();
            unitController.mode = UnitController.Mode.OpenSelect;
            gameObject.SetActive(false);
        }
    }

    public void SetMainMenu(bool enabled) {
        mainMenu.gameObject.SetActive(enabled);
        attackMenu.gameObject.SetActive(false);
        weaponMenu.gameObject.SetActive(false);
    }

    public void SetAttackMenu(bool enabled) {
        mainMenu.gameObject.SetActive(!enabled);
        attackMenu.gameObject.SetActive(enabled);
    }

    public void SetForecastMenu(Unit attacker, Unit defender, int dist) {
        mainMenu.gameObject.SetActive(false);
        attackMenu.gameObject.SetActive(false);
        weaponMenu.gameObject.SetActive(true);

        selectedUnit = attacker;
        targetUnit = defender;

        usableWeapons = new List<Weapon>();
        foreach(Weapon weapon in attacker.weapons) {
            if(dist >= weapon.minRange && dist <= weapon.maxRange) {
                usableWeapons.Add(weapon);
            }
        }

        if(usableWeapons.Count > 1) {
            incrementWeaponList.interactable = true;
            decrementWeaponList.interactable = true;
        } else {
            incrementWeaponList.interactable = false;
            decrementWeaponList.interactable = false;
        }

        if(dist >= attacker.equippedWeapon.minRange && dist <= attacker.equippedWeapon.maxRange) {
            usedWeaponIndex = usableWeapons.IndexOf(attacker.equippedWeapon);
        } else {
            if(usableWeapons.Count != 0) {
                usedWeaponIndex = 0;
                attacker.equippedWeapon = usableWeapons[usedWeaponIndex];
            } else {
                Debug.Log("No usable weapons");
            }
        }

        FillForcastDisplay(attacker, defender, dist);
    }

    private void FillForcastDisplay(Unit attacker, Unit defender, int dist) {
        selectedWeaponText.text = usableWeapons[usedWeaponIndex].name;
        int defenderDamage = -1;
        if(dist >= defender.equippedWeapon.minRange && dist <= defender.equippedWeapon.maxRange) {
            defenderDamage = defender.equippedWeapon.damage;
        }

        defenderForecast.Forecast(defender, defenderDamage, usableWeapons[usedWeaponIndex].damage);

        if(usableWeapons[usedWeaponIndex].damage >= defender.health.currentValue) { //expected kill, no return damage
            defenderDamage = 0;
        }

        attackerForecast.Forecast(attacker, usableWeapons[usedWeaponIndex].damage, defenderDamage);
    }

    public void SwitchWeapon(int modifier) {
        usedWeaponIndex += modifier;
        if(usedWeaponIndex == usableWeapons.Count) {
            usedWeaponIndex = 0;
        }
        if(usedWeaponIndex == -1) {
            usedWeaponIndex = usableWeapons.Count - 1;
        }
        selectedUnit.equippedWeapon = usableWeapons[usedWeaponIndex];
        FillForcastDisplay(selectedUnit, targetUnit, unitController.GetDistance(selectedUnit, targetUnit));
    }

    public void ConfirmAction() {
        unitController.StartCombat(selectedUnit, targetUnit);
        gameObject.SetActive(false);
    }

    private void OnEnable() {
        SetMainMenu(true);
    }
}
