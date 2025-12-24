using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class JFADistanceField : MonoBehaviour
{
    [Header("Inputs")]
    public RenderTexture maskRT;          // 你已有的Mask RT（alpha为字）
    public ComputeShader jfaCS;

    [Header("Settings")]
    [Range(0, 1)] public float threshold = 0.5f;
    public bool generateSignedDistance = false;

    [Header("Smooth Settings")]
    public bool useSmoothing = true;
    public float smoothSpeed = 0.1f;
    public float maxAlpha = 1.0f;
    public float easeRange = 32.0f;

    [Header("Outputs")]
    public RenderTexture distOutRT;       // 外部距离：到字体最近距离
    public RenderTexture distOutSmoothRT; // 平滑后的外部距离
    public RenderTexture distInRT;        // 内部距离：到背景最近距离（可选）
    public RenderTexture signedRT;        // sdf = distOut - distIn（可选）

    RenderTexture seedA, seedB;
    bool _needsSmoothInit = false;

    int kInit, kJump, kDist, kSmooth;

    RenderTexture smoothA, smoothB; // 内部 ping-pong


    void OnEnable()
    {
        kInit = jfaCS.FindKernel("KInit");
        kJump = jfaCS.FindKernel("KJump");
        kDist = jfaCS.FindKernel("KDistance");
        kSmooth = jfaCS.FindKernel("KSmooth");

        AllocateIfNeeded();
    }

    void OnDisable()
    {
        ReleaseRT(seedA); ReleaseRT(seedB);
        // 你也可以不释放distOutRT等，取决于你如何管理
    }

    void AllocateIfNeeded()
    {
        if(maskRT == null) return;
        int w = maskRT.width;
        int h = maskRT.height;

        // seed 用 float2：推荐 RGHalf 或 RGFloat
        if (seedA == null || seedA.width != w || seedA.height != h) seedA = CreateRT(w, h, GraphicsFormat.R16G16_SFloat);
        if (seedB == null || seedB.width != w || seedB.height != h) seedB = CreateRT(w, h, GraphicsFormat.R16G16_SFloat);

        if (distOutRT == null || distOutRT.width != w || distOutRT.height != h) distOutRT = CreateRT(w, h, GraphicsFormat.R16_SFloat);

        if (useSmoothing)
        {
            if (smoothA == null || smoothA.width != w || smoothA.height != h) smoothA = CreateRT(w, h, GraphicsFormat.R16_SFloat);
            if (smoothB == null || smoothB.width != w || smoothB.height != h) smoothB = CreateRT(w, h, GraphicsFormat.R16_SFloat);

            if (distOutSmoothRT == null || distOutSmoothRT.width != w || distOutSmoothRT.height != h)
            {
                distOutSmoothRT = smoothA;
                // 标记需要初始化
                _needsSmoothInit = true;
            }
        }

        if (generateSignedDistance)
        {
            if (distInRT == null || distInRT.width != w || distInRT.height != h) distInRT = CreateRT(w, h, GraphicsFormat.R16_SFloat);
            if (signedRT == null || signedRT.width != w || signedRT.height != h) signedRT = CreateRT(w, h, GraphicsFormat.R16_SFloat);
        }
    }

    RenderTexture CreateRT(int w, int h, GraphicsFormat format)
    {
        var rt = new RenderTexture(w, h, 0)
        {
            enableRandomWrite = true,
            graphicsFormat = format,
            filterMode = FilterMode.Point,   // seed传播建议 Point
            wrapMode = TextureWrapMode.Clamp
        };
        rt.Create();
        return rt;
    }

    void ReleaseRT(RenderTexture rt)
    {
        if (rt != null) rt.Release();
    }

    void FixedUpdate()
    {
        Run();
    }

    public void Run()
    {
        if (maskRT == null || jfaCS == null) return;

        AllocateIfNeeded();

        // 1) 先生成“到字体”的距离 distOutRT
        RunOnePass(maskRT, invert: false, out distOutRT);

        // 1.5) 平滑处理
        if (useSmoothing && smoothA != null)
        {
            if (_needsSmoothInit)
            {
                Graphics.Blit(distOutRT, smoothA); // prev = target
                _needsSmoothInit = false;
            }
            else
            {
                jfaCS.SetInt("_Width", distOutRT.width);
                jfaCS.SetInt("_Height", distOutRT.height);
                jfaCS.SetFloat("_SmoothSpeed", smoothSpeed);
                jfaCS.SetFloat("_MaxAlpha", maxAlpha);
                jfaCS.SetFloat("_EaseRange", easeRange);

                jfaCS.SetTexture(kSmooth, "_DistancePrev", smoothA);
                jfaCS.SetTexture(kSmooth, "_DistanceTarget", distOutRT);
                jfaCS.SetTexture(kSmooth, "_DistanceNext", smoothB);

                Dispatch2D(jfaCS, kSmooth, distOutRT.width, distOutRT.height);

                // swap
                var tmp = smoothA; smoothA = smoothB; smoothB = tmp;
                distOutSmoothRT = smoothA; // 对外输出当前这一张
            }
        }


        if (!generateSignedDistance) return;

        // 2) 再生成“到背景”的距离 distInRT
        RunOnePass(maskRT, invert: true, out distInRT);
    }

    void RunOnePass(RenderTexture mask, bool invert, out RenderTexture distRT)
    {
        distRT = invert ? distInRT : distOutRT;

        int w = mask.width;
        int h = mask.height;

        jfaCS.SetInt("_Width", w);
        jfaCS.SetInt("_Height", h);
        jfaCS.SetFloat("_Threshold", threshold);
        jfaCS.SetInt("_Invert", invert ? 1 : 0);
        jfaCS.SetTexture(kInit, "_Mask", mask);

        // --- Init: 写 seedA
        jfaCS.SetTexture(kInit, "_SeedWrite", seedA);
        Dispatch2D(jfaCS, kInit, w, h);

        // --- JFA: 从最大步长开始，每次 /2
        int step = HighestPowerOfTwoCeil(Mathf.Max(w, h)) / 2;
        RenderTexture read = seedA;
        RenderTexture write = seedB;

        while (step >= 1)
        {
            jfaCS.SetInt("_Step", step);
            jfaCS.SetTexture(kJump, "_SeedRead", read);
            jfaCS.SetTexture(kJump, "_SeedWrite", write);
            Dispatch2D(jfaCS, kJump, w, h);

            // ping-pong
            var tmp = read; read = write; write = tmp;
            step /= 2;
        }

        // --- Distance: seedRead -> distRT
        jfaCS.SetTexture(kDist, "_SeedRead", read);
        jfaCS.SetTexture(kDist, "_DistanceWrite", distRT);
        Dispatch2D(jfaCS, kDist, w, h);
    }

    void Dispatch2D(ComputeShader cs, int kernel, int w, int h)
    {
        uint tx, ty, tz;
        cs.GetKernelThreadGroupSizes(kernel, out tx, out ty, out tz);
        int gx = Mathf.CeilToInt(w / (float)tx);
        int gy = Mathf.CeilToInt(h / (float)ty);
        cs.Dispatch(kernel, gx, gy, 1);
    }

    int HighestPowerOfTwoCeil(int x)
    {
        int p = 1;
        while (p < x) p <<= 1;
        return p;
    }
}
