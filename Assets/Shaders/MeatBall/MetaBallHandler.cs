using UnityEngine;

public class MetaBallHandler : MonoBehaviour
{
    [Header("Compute Shader")]
    public ComputeShader colorThresholdCompute;

    [Header("输入 / 输出")]
    public RenderTextureCreater inputRTC;
    public RenderTexture inputRT{get{return inputRTC.rt;}}
    public RenderTexture outputRT; // 运行时自动创建

    [Header("阈值")]
    public float thresholdA = 0.7f;
    public float thresholdB = 0.3f;

    [Header("颜色（支持透明）")]
    public Color colorAboveA = Color.red;
    public Color colorBetween = Color.green;
    public Color colorBelowB = Color.blue;

    int _kernel = -1;
    int _width;
    int _height;

    void Awake()
    {
        InitKernel();
    }

    void OnDisable()
    {
        ReleaseOutput();
    }

    void Update()
    {
        if (colorThresholdCompute == null || inputRT == null) return;

        InitKernel();
        EnsureOutputMatchesInput();
        DispatchCompute();
    }

    void InitKernel()
    {
        if (colorThresholdCompute != null && _kernel < 0)
        {
            _kernel = colorThresholdCompute.FindKernel("CSMain");
        }
    }

    void EnsureOutputMatchesInput()
    {
        if (inputRT == null) return;

        if (outputRT != null &&
            outputRT.width == inputRT.width &&
            outputRT.height == inputRT.height)
        {
            return;
        }

        ReleaseOutput();

        _width = inputRT.width;
        _height = inputRT.height;

        var desc = new RenderTextureDescriptor(_width, _height, RenderTextureFormat.ARGB32, 0);
        desc.enableRandomWrite = true;

        outputRT = new RenderTexture(desc);
        outputRT.name = "MetaBallColorRT";
        outputRT.Create();
    }

    void DispatchCompute()
    {
        if (_kernel < 0 || outputRT == null) return;

        colorThresholdCompute.SetFloat("_ThresholdA", thresholdA);
        colorThresholdCompute.SetFloat("_ThresholdB", thresholdB);
        colorThresholdCompute.SetVector("_ColorHigh", colorAboveA);
        colorThresholdCompute.SetVector("_ColorMid", colorBetween);
        colorThresholdCompute.SetVector("_ColorLow", colorBelowB);

        colorThresholdCompute.SetTexture(_kernel, "_SourceTex", inputRT);
        colorThresholdCompute.SetTexture(_kernel, "_Result", outputRT);

        int groupX = Mathf.CeilToInt(_width / 8.0f);
        int groupY = Mathf.CeilToInt(_height / 8.0f);
        colorThresholdCompute.Dispatch(_kernel, groupX, groupY, 1);
    }

    void ReleaseOutput()
    {
        if (outputRT != null)
        {
            outputRT.Release();
            Destroy(outputRT);
            outputRT = null;
        }
    }
}