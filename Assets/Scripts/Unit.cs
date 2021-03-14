using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public string unitName;
    public Vector3Int position;
    public bool canMove;

    // Start is called before the first frame update
    void Start()
    {
        
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

        }
    }
}
