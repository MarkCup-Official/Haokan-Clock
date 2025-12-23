using UnityEngine;

[ExecuteAlways]
public class BackgroundController : MonoBehaviour
{
    public Material backgroundMaterial;
    
    [Header("Balls Control")]
    public Transform[] ballTransforms = new Transform[5];
    
    public bool overrideColors = false;
    public Color[] ballColors = new Color[5];
    
    public bool overrideSizes = false;
    public float[] ballSizes = new float[5] { 1.3f, 0.8f, 0.9f, 1.1f, 0.9f };

    [Header("Random Movement")]
    public bool enableRandomMovement = false;
    public float movementSpeed = 0.5f;
    public Vector2 movementRange = new Vector2(5f, 3f);
    
    private Vector3[] _initialPositions;
    private float[] _noiseOffsets;

    [Header("Posterization & Outline")]
    public bool overridePosterization = false;
    [Range(2, 20)] public float posterizeSteps = 5f;
    public Color outlineColor = Color.white;
    [Range(0, 0.5f)] public float outlineWidth = 0.05f;

    [Header("Global Settings")]
    [Range(0, 1)] public float hueShift = 0f;
    [Range(0, 2)] public float saturation = 1f;

    private static readonly int HueShiftId = Shader.PropertyToID("_HueShift");
    private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
    
    private static readonly int[] BallPosIds = {
        Shader.PropertyToID("_Ball1Pos"),
        Shader.PropertyToID("_Ball2Pos"),
        Shader.PropertyToID("_Ball3Pos"),
        Shader.PropertyToID("_Ball4Pos"),
        Shader.PropertyToID("_Ball5Pos")
    };
    
    private static readonly int[] BallColorIds = {
        Shader.PropertyToID("_Ball1Color"),
        Shader.PropertyToID("_Ball2Color"),
        Shader.PropertyToID("_Ball3Color"),
        Shader.PropertyToID("_Ball4Color"),
        Shader.PropertyToID("_Ball5Color")
    };

    private static readonly int[] BallSizeIds = {
        Shader.PropertyToID("_Ball1Size"),
        Shader.PropertyToID("_Ball2Size"),
        Shader.PropertyToID("_Ball3Size"),
        Shader.PropertyToID("_Ball4Size"),
        Shader.PropertyToID("_Ball5Size")
    };

    private static readonly int PosterizeStepsId = Shader.PropertyToID("_PosterizeSteps");
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

    void Start()
    {
        InitializeMovement();
    }

    void InitializeMovement()
    {
        if (ballTransforms == null) return;
        
        _initialPositions = new Vector3[ballTransforms.Length];
        _noiseOffsets = new float[ballTransforms.Length];
        
        for (int i = 0; i < ballTransforms.Length; i++)
        {
            if (ballTransforms[i] != null)
            {
                _initialPositions[i] = ballTransforms[i].position;
                _noiseOffsets[i] = Random.value * 1000f;
            }
        }
    }

    void Update()
    {
        if (backgroundMaterial == null) return;

        // 处理随机运动
        if (enableRandomMovement && Application.isPlaying)
        {
            if (_initialPositions == null || _initialPositions.Length != ballTransforms.Length)
            {
                InitializeMovement();
            }

            float time = Time.time * movementSpeed;
            for (int i = 0; i < ballTransforms.Length; i++)
            {
                if (ballTransforms[i] != null)
                {
                    float offsetX = (Mathf.PerlinNoise(time + _noiseOffsets[i], 0f) - 0.5f) * 2f;
                    float offsetY = (Mathf.PerlinNoise(0f, time + _noiseOffsets[i]) - 0.5f) * 2f;
                    
                    Vector3 targetPos = _initialPositions[i] + new Vector3(offsetX * movementRange.x, offsetY * movementRange.y, 0);
                    ballTransforms[i].position = targetPos;
                }
            }
        }

        // 设置全局属性
        backgroundMaterial.SetFloat(HueShiftId, hueShift);
        backgroundMaterial.SetFloat(SaturationId, saturation);

        // 设置每个球的位置
        if (ballTransforms != null)
        {
            for (int i = 0; i < ballTransforms.Length && i < BallPosIds.Length; i++)
            {
                if (ballTransforms[i] != null)
                {
                    // 直接使用物体的世界坐标
                    Vector3 pos = ballTransforms[i].position;
                    backgroundMaterial.SetVector(BallPosIds[i], new Vector4(pos.x, pos.y, 0, 0));
                    
                    // 确定球的大小
                    float size;
                    if (overrideSizes && i < ballSizes.Length)
                    {
                        size = ballSizes[i];
                    }
                    else
                    {
                        // 使用物体的缩放来控制球的大小 (取平均缩放或单一轴)
                        size = (ballTransforms[i].lossyScale.x + ballTransforms[i].lossyScale.y) * 0.5f;
                    }
                    backgroundMaterial.SetFloat(BallSizeIds[i], size);
                }
            }
        }

        // 如果开启了颜色覆盖，则从脚本更新颜色
        if (overrideColors && ballColors != null)
        {
            for (int i = 0; i < ballColors.Length && i < BallColorIds.Length; i++)
            {
                if (i < ballColors.Length)
                {
                    backgroundMaterial.SetColor(BallColorIds[i], ballColors[i]);
                }
            }
        }
    }
}

