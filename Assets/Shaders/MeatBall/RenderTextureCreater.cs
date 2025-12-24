using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureCreater : MonoBehaviour
{
    public Camera cam;
    public RenderTexture rt;

    public int fixedHeight = 1080;

    void ResetRT()
    {
        w = Screen.width;
        h = Screen.height;
        rt = new RenderTexture((int)((float)fixedHeight/h*w), fixedHeight, 0);
        rt.Create();

        cam.targetTexture = rt;
    }

    int w=0;
    int h=0;

    void Update(){
        if(w != Screen.width || h != Screen.height){
            ResetRT();
        }
    }

}
