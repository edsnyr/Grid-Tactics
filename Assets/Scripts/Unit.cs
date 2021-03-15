using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public enum GroundStatus { Grounded, Flying };

    public string unitName;
    public int teamNumber;
    public Vector3Int position;

    public int startingHealth;
    public Resource health;
    public List<ResourceDisplay> healthDisplayPrefabs;

    public int maxMovement;
    public int minAttackRange;
    public int maxAttackRange;

    public GroundStatus groundStatus;
    public bool canMove;

    private void Awake() {
        transform.position = position;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        health = new Resource(startingHealth);
        health.AddResourceDisplays(InstantiateHealthDisplays());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator MoveOnPath(List<Vector3Int> path, float time) {
        if(position != path[0]) {
            Debug.Log("Wrong start location");
            yield return null;
        } else {
            SetHealthDisplays(false);
            for(int i = 1; i < path.Count; i++) {
                Vector3 startPos = transform.position;

                float elapsedTime = 0;

                while(elapsedTime < time) {
                    transform.position = Vector3.Lerp(startPos, path[i], (elapsedTime / time));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
            transform.position = path[path.Count - 1];
            position = path[path.Count - 1];
            SetHealthDisplays(true);
        }
    }

    private List<ResourceDisplay> InstantiateHealthDisplays() {
        Canvas canvas = FindObjectOfType<Canvas>();
        if(canvas == null) {
            Debug.Log("Cannot find canvas");
            return null;
        }
        List<ResourceDisplay> displays = new List<ResourceDisplay>();
        foreach(ResourceDisplay prefab in healthDisplayPrefabs) {
            Debug.Log("Instantiate prefab");
            ResourceDisplay rd = Instantiate(prefab, canvas.transform);
            rd.SetParentUnit(this);
            displays.Add(rd);
        }
        Debug.Log(displays.Count);
        return displays;
    }

    public void SetHealthDisplays(bool enabled) {
        foreach(ResourceDisplay rd in health.GetDisplays()) {
            rd.gameObject.SetActive(enabled);
        }
    }
}
