using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "New Weapon", order = 0)]
public class Weapon : ScriptableObject
{

    public int minRange;
    public int maxRange;
    public int damage;

}
