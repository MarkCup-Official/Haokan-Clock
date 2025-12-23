using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowH : MonoBehaviour
{
    public Clock clock;
    public TextMeshPro text;

    public int index;

    public bool showMin = false;

    public Material material;

    string lastText = "";

    void Start()
    {
        material.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
        material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1);
    }

    void Update()
    {
        string currentText = clock.Hours.ToString("D2")[index].ToString();
        if (showMin)
        {
            currentText = clock.Minutes.ToString("D2")[index].ToString();
        }
        if (currentText != lastText)
        {
            lastText = currentText;
            StartCoroutine(Fade(currentText));
        }
    }

    IEnumerator Fade(string currentText)
    {
        float duration = 1f / 2.1f; // 计算总时长
        float elapsed = 0f;
        
        // 第一阶段：从 0 到 -1，缓入快出
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime*1.5f;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease-in 曲线：t^2 (二次缓入)
            float easedT = t * t;
            float dilate = Mathf.Lerp(0f, -1f, easedT);
            material.SetFloat(ShaderUtilities.ID_FaceDilate, dilate);
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, dilate+1);
            yield return null;
        }

        
        text.text = currentText;

        
        // 第二阶段：从 -1 到 0，快进慢出
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime*1.5f;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease-out 曲线：1 - (1-t)^2 (二次缓出)
            float easedT = 1f - Mathf.Pow(1f - t, 2f);
            float dilate = Mathf.Lerp(-1f, 0f, easedT);
            material.SetFloat(ShaderUtilities.ID_FaceDilate, dilate);
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, dilate+1);
            yield return null;
        }

        material.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
        material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 1);
    }
}
