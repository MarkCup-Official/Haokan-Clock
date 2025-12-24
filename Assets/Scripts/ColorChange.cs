using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    public Color beforeColor1;
    public Color beforeColor2;
    public Color startColor1;
    public Color startColor2;
    public Color endColor1;
    public Color endColor2;

    public Material material;

    public Clock StartTime;

    public Clock CurrentTime;

    [Range(0, 1)]
    public float manualTime = 0;

    public float transitionDuration = 0.5f;
    private Coroutine _activeTransition;
    private Color _lastTarget1;
    private Color _lastTarget2;
    private int _lastSecond = -1;

    void SetColor(Color a, Color b)
    {
        if (ColorDiff(a, _lastTarget1) < 0.001f && ColorDiff(b, _lastTarget2) < 0.001f)
            return;

        _lastTarget1 = a;
        _lastTarget2 = b;

        if (_activeTransition != null) StopCoroutine(_activeTransition);
        _activeTransition = StartCoroutine(SetColorCoroutine(a, b));
    }

    float ColorDiff(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) + Mathf.Abs(a.a - b.a);
    }

    IEnumerator SetColorCoroutine(Color targetA, Color targetB)
    {
        Color startA = material.GetColor("_FillColorA");
        Color startB = material.GetColor("_FillColorB");
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float easedT = 1f - (1f - t) * (1f - t);

            material.SetColor("_FillColorA", Color.Lerp(startA, targetA, easedT));
            material.SetColor("_FillColorB", Color.Lerp(startB, targetB, easedT));
            yield return null;
        }

        material.SetColor("_FillColorA", targetA);
        material.SetColor("_FillColorB", targetB);
        _activeTransition = null;
    }

    Color ColorLerpHSL(Color a, Color b, float t)
    {
        float ah, asat, alight;
        float bh, bsat, blight;
        RGBToHSL(a, out ah, out asat, out alight);
        RGBToHSL(b, out bh, out bsat, out blight);

        float h = Mathf.LerpAngle(ah * 360f, bh * 360f, t) / 360f;
        float s = Mathf.Lerp(asat, bsat, t);
        float l = Mathf.Lerp(alight, blight, t);

        return HSLToRGB(h, s, l, Mathf.Lerp(a.a, b.a, t));
    }

    void RGBToHSL(Color color, out float h, out float s, out float l)
    {
        float r = color.r;
        float g = color.g;
        float b = color.b;
        float max = Mathf.Max(r, Mathf.Max(g, b));
        float min = Mathf.Min(r, Mathf.Min(g, b));
        float d = max - min;

        l = (max + min) / 2f;

        if (d == 0)
        {
            h = s = 0;
        }
        else
        {
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
            if (max == r) h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g) h = (b - r) / d + 2;
            else h = (r - g) / d + 4;
            h /= 6f;
        }
    }

    Color HSLToRGB(float h, float s, float l, float a)
    {
        float r, g, b;
        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
            float p = 2f * l - q;
            r = HueToRGB(p, q, h + 1f / 3f);
            g = HueToRGB(p, q, h);
            b = HueToRGB(p, q, h - 1f / 3f);
        }
        return new Color(r, g, b, a);
    }

    float HueToRGB(float p, float q, float t)
    {
        if (t < 0) t += 1f;
        if (t > 1) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }

    void Update()
    {
        if (CurrentTime.Seconds == _lastSecond)
        {
            return;
        }
        _lastSecond = CurrentTime.Seconds;

        if (CurrentTime.Seconds == 0 && CurrentTime.Minutes == 0 && CurrentTime.Hours == 0)
        {
            SetColor(endColor1, endColor2);
        }
        else if (CurrentTime.currentTimestamp < StartTime.displayTimestamp)
        {
            SetColor(beforeColor1, beforeColor2);
        }
        else
        {
            float t = (CurrentTime.currentTimestamp - StartTime.displayTimestamp) / (CurrentTime.displayTimestamp - StartTime.displayTimestamp);
            SetColor(ColorLerpHSL(startColor1, endColor1, t), ColorLerpHSL(startColor2, endColor2, t));
        }


        if (manualTime > 0)
        {
            float t = manualTime;
            SetColor(ColorLerpHSL(startColor1, endColor1, t), ColorLerpHSL(startColor2, endColor2, t));
        }
    }
}
