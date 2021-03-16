using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceSlider : ResourceDisplay
{

    public Slider slider;

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
        } else if(slider == null) {
            Debug.Log("No slider display");
        } else {
            slider.maxValue = resource.maxValue;
            slider.value = resource.currentValue;
            AlignWithParent();
        }
    }

    public override void SetParentUnit(Unit parent) {
        parentUnit = parent;
    }

    private void OnEnable() {
        if(parentUnit != null && resource != null) {
            AlignWithParent();
        }
    }

    private void AlignWithParent() {
        Vector3 pos = parentUnit.transform.position;
        Vector2 canvasPos;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(pos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(FindObjectOfType<Canvas>().GetComponent<RectTransform>(), screenPoint, null, out canvasPos);
        transform.localPosition = canvasPos + new Vector2(0,-20);
    }
}
