using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{

    // distance to follow from
    public Transform target;
    public float distance;

    private Vector3 positionDelta;

    // Start is called before the first frame update
    void Start()
    {
        positionDelta = new Vector3(0, 0, -distance);
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null) {
            this.transform.position = target.transform.position
                + positionDelta;
        }
    }
}
