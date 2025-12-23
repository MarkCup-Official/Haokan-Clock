using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowOutRT : MonoBehaviour
{
    public JFADistanceField jfaDistanceField;
    public Material material;

    void Update()
    {
        RenderTexture targetRT = jfaDistanceField.useSmoothing ? jfaDistanceField.distOutSmoothRT : jfaDistanceField.distOutRT;
        
        if (targetRT == null) return;

        material.SetTexture("_DistTex", targetRT);
        this.enabled = false;
    }
}
