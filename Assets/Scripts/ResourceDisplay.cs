using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourceDisplay : MonoBehaviour
{

    public abstract void SetResource(Resource r);

    public abstract void UpdateDisplay();

    public abstract void SetParentUnit(Unit parent);

}
