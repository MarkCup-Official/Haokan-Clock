using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowRT_MetaBall : MonoBehaviour
{
    public MetaBallHandler metaBallHandler;

    public Material material;

    public RenderTexture rt;

    static readonly int RenderTexId = Shader.PropertyToID("_RenderTex");
    static readonly int UseRenderTexId = Shader.PropertyToID("_UseRenderTex");

    void Update()
    {
        if (metaBallHandler == null || material == null) return;

        if(rt == metaBallHandler.outputRT) return;

        rt = metaBallHandler.outputRT;

        material.SetTexture(RenderTexId, rt);
        material.SetFloat(UseRenderTexId, rt != null ? 1f : 0f);
    }
}
