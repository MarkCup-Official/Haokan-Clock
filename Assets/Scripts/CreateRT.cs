using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRT : MonoBehaviour
{
    public JFADistanceField jfaDistanceField;

    public Camera cam;
    public RenderTexture rt;

    void ResetRT()
    {
        w = Screen.width;
        h = Screen.height;
        rt = new RenderTexture(w, h, 0);
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
