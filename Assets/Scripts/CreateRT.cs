using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRT : MonoBehaviour
{
    public JFADistanceField jfaDistanceField;

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
        jfaDistanceField.maskRT = rt;
    }

    int w=0;
    int h=0;

    void Update(){
        if(w != Screen.width || h != Screen.height){
            ResetRT();
        }
    }

}
