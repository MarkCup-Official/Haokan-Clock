using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public GameObject fireBall;

    public int max;

    float time;

    public float maxSize;
    public float minSize;

    public float cold;
    void Update()
    {
        time += Time.deltaTime;
        if(time > cold)
        {
            time = 0;
            max--;
            GameObject go = Instantiate(fireBall, transform.position, Quaternion.identity);
            float size=Random.Range(minSize, maxSize);
            go.transform.localScale = new Vector3(size, size, size);
            go.transform.position += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
        }
        if(max <= 0)
        {
            enabled =false;
        }
    }
}
