using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource
{

    public int maxValue;
    public int currentValue;

    List<ResourceDisplay> displays;

    public Resource(int value) {
        maxValue = value;
        currentValue = value;
        displays = new List<ResourceDisplay>();
    }

    public void AddResourceDisplays(ResourceDisplay rd) {
        if(!displays.Contains(rd)) {
            displays.Add(rd);
            rd.SetResource(this);
        }
        UpdateAllDisplays();
    }

    public void AddResourceDisplays(List<ResourceDisplay> rds) {
        if(displays == null)
            displays = new List<ResourceDisplay>();
        foreach(ResourceDisplay rd in rds) {
            if(!displays.Contains(rd)) {
                displays.Add(rd);
                rd.SetResource(this);
            }
        }
        UpdateAllDisplays();
    }

    public List<ResourceDisplay> GetDisplays() {
        return displays;
    }

    public int ModifyCurrentValue(int value) {
        currentValue += value;
        if(currentValue > maxValue) {
            currentValue = maxValue;
        } else if(currentValue < 0) {
            currentValue = 0;
        }
        UpdateAllDisplays();
        return currentValue;
    }

    public int ModifyMaxValue(int value) {
        maxValue += value;
        currentValue += value;
        if(maxValue < 1) {
            maxValue = 1;
        }
        if(currentValue < 1) {
            currentValue = 1;
        }
        UpdateAllDisplays();
        return maxValue;
    }

    public int SetMaxValue(int value) {
        maxValue = value;
        currentValue = value;
        if(maxValue < 1) {
            maxValue = 1;
        }
        if(currentValue < 1) {
            currentValue = 1;
        }
        UpdateAllDisplays();
        return maxValue;
    }

    private void UpdateAllDisplays() {
        foreach(ResourceDisplay rd in displays) {
            rd.UpdateDisplay();
        }
    }
}
