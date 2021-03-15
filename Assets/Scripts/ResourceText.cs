using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceText : ResourceDisplay
{

    public TextMeshProUGUI textBox;

    Resource resource;
    Unit parentUnit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SetResource(Resource r) {
        resource = r;
    }

    public override void UpdateDisplay() {
        if(resource == null) {
            Debug.Log("No resource");
        } else if(textBox == null) {
            Debug.Log("No text display");
        } else {
            textBox.text = resource.currentValue + "/" + resource.maxValue;
        }
    }

    public override void SetParentUnit(Unit parent) {
        parentUnit = parent;
    }
}
