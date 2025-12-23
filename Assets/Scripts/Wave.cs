using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave : MonoBehaviour
{
    public float offset = 0;
    public float speed = 1;
    public float amplitude = 0.3f;

    void Update()
    {
        transform.position = new Vector3(transform.position.x,  -0.2f+Mathf.Sin(Time.time * speed + offset) * amplitude);
    }
}
