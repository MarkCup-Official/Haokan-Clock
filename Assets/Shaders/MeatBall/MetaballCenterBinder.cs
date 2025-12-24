using UnityEngine;

[DisallowMultipleComponent]
public class MetaballCenterBinder : MonoBehaviour
{
    public Transform centerSource;

    [Header("Choose one (auto if empty)")]
    public SpriteRenderer spriteRenderer;
    public Renderer meshRenderer;

    [Header("Shader Properties")]
    public string centerWsProperty = "_CenterWS";
    public string solidRadiusProperty = "_SolidRadius";
    public string gradientWidthProperty = "_GradientWidth";
    public string sizeProperty = "_SolidRadius"; // 你原来的 size

    public bool driveRadii = false;
    public float solidRadius = 0.5f;
    public float gradientWidth = 0.25f;

    Renderer _r;
    MaterialPropertyBlock _mpb;
    int _centerId, _solidId, _widthId, _sizeId;

    void Reset()
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!meshRenderer) meshRenderer = GetComponent<Renderer>();
    }

    void Awake()
    {
        _r = spriteRenderer ? (Renderer)spriteRenderer : meshRenderer;
        _mpb = new MaterialPropertyBlock();

        _centerId = Shader.PropertyToID(centerWsProperty);
        _solidId  = Shader.PropertyToID(solidRadiusProperty);
        _widthId  = Shader.PropertyToID(gradientWidthProperty);
        _sizeId   = Shader.PropertyToID(sizeProperty);
    }

    void LateUpdate()
    {
        if (_r == null || centerSource == null) return;

        _r.GetPropertyBlock(_mpb);

        Vector3 c = centerSource.position;
        _mpb.SetVector(_centerId, new Vector4(c.x, c.y, 0f, 0f));

        if (driveRadii)
        {
            _mpb.SetFloat(_solidId, solidRadius);
            _mpb.SetFloat(_widthId, gradientWidth);
        }

        _mpb.SetFloat(_sizeId, transform.localScale.x * 0.5f - 0.25f);

        _r.SetPropertyBlock(_mpb);
    }
}
