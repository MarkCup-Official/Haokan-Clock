using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeFitter : MonoBehaviour
{
    int w=0;
    int h=0;

    void Update(){
        if(w != Screen.width || h != Screen.height){
            ResetSize();
        }
    }

    void ResetSize(){
        w = Screen.width;
        h = Screen.height;
        transform.localScale = new Vector3((float)w/h, 1, 1);
        Debug.Log("ResetSize: " + w + " " + h);
    }
}
