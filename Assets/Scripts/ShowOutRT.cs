using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowOutRT : MonoBehaviour
{
    public JFADistanceField jfaDistanceField;
    public Material material;

    public RenderTexture rt;

    void Update()
    {
        RenderTexture targetRT = jfaDistanceField.useSmoothing ? jfaDistanceField.distOutSmoothRT : jfaDistanceField.distOutRT;
        
        if (targetRT == null) return;

        if(rt == targetRT) return;
        
        rt=targetRT;
        material.SetTexture("_DistTex", rt);
    }
}
