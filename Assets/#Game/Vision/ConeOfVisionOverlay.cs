using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// Рисует полноэкранное затемнение вне конуса обзора с плавным фейдом (стиль Darkwood).
/// Параметры конуса берутся из MMConeOfVision2D на игроке.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ConeOfVisionOverlay : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField]
    [Tooltip("Трансформ игрока (источник позиции конуса)")]
    private Transform playerTransform;

    [SerializeField]
    [Tooltip("Конус обзора на игроке (откуда брать радиус, угол и направление)")]
    private MMConeOfVision2D coneOfVision;

    [SerializeField]
    [Tooltip("Материал с шейдером Game/ConeOfVisionOverlay")]
    private Material overlayMaterial;

    [Header("Настройки затемнения")]
    [SerializeField]
    [Tooltip("Включить оверлей затемнения")]
    private bool overlayEnabled = true;

    [SerializeField]
    [Tooltip("Цвет области вне конуса (обычно чёрный)")]
    private Color overlayColor = Color.black;

    [SerializeField]
    [Tooltip("Ширина плавного перехода от видимой зоны к затемнению (в единицах мира)")]
    [Min(0.01f)]
    private float fadeWidth = 1f;

    [SerializeField]
    [Tooltip("Расстояние quad от камеры по оси взгляда")]
    private float quadDistanceFromCamera = 10f;

    private Camera _camera;
    private Transform _quadTransform;
    private MeshRenderer _quadRenderer;
    private MaterialPropertyBlock _propertyBlock;
    private MMConeOfVision2D _cachedConeOfVision;

    private static readonly int PlayerWorldPosId = Shader.PropertyToID("_PlayerWorldPos");
    private static readonly int ViewAngleId = Shader.PropertyToID("_ViewAngle");
    private static readonly int ConeHalfAngleId = Shader.PropertyToID("_ConeHalfAngle");
    private static readonly int ConeRadiusId = Shader.PropertyToID("_ConeRadius");
    private static readonly int FadeWidthId = Shader.PropertyToID("_FadeWidth");
    private static readonly int OverlayColorId = Shader.PropertyToID("_OverlayColor");
    private static readonly int CameraWorldPosId = Shader.PropertyToID("_CameraWorldPos");
    private static readonly int OrthoSizeId = Shader.PropertyToID("_OrthoSize");
    private static readonly int AspectRatioId = Shader.PropertyToID("_AspectRatio");

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _propertyBlock = new MaterialPropertyBlock();

        if (overlayMaterial == null)
        {
            Debug.LogWarning("ConeOfVisionOverlay: материал не назначен, оверлей отключён.");
            enabled = false;
            return;
        }

        CreateOverlayQuad();
    }

    private void CreateOverlayQuad()
    {
        GameObject quadObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadObject.name = "ConeOfVisionOverlayQuad";
        quadObject.transform.SetParent(transform, false);
        quadObject.transform.localPosition = new Vector3(0f, 0f, quadDistanceFromCamera);
        quadObject.transform.localRotation = Quaternion.identity;
        quadObject.transform.localScale = Vector3.one;

        var meshRenderer = quadObject.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = overlayMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        var collider = quadObject.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        _quadTransform = quadObject.transform;
        _quadRenderer = meshRenderer;
    }

    private void LateUpdate()
    {
        if (!overlayEnabled || _quadRenderer == null || overlayMaterial == null)
        {
            if (_quadRenderer != null)
                _quadRenderer.enabled = false;
            return;
        }

        if (playerTransform == null)
        {
            _quadRenderer.enabled = false;
            return;
        }

        if (coneOfVision == null && _cachedConeOfVision == null && playerTransform != null)
            _cachedConeOfVision = playerTransform.GetComponent<MMConeOfVision2D>();

        MMConeOfVision2D cone = coneOfVision != null ? coneOfVision : _cachedConeOfVision;
        if (cone == null)
        {
            _quadRenderer.enabled = false;
            return;
        }

        _quadRenderer.enabled = true;

        UpdateQuadPositionAndScale();
        UpdateMaterialProperties(cone);
    }

    private void UpdateQuadPositionAndScale()
    {
        if (_camera == null || _quadTransform == null)
            return;

        float orthoSize = _camera.orthographicSize;
        float aspect = _camera.aspect;
        _quadTransform.position = _camera.transform.position + _camera.transform.forward * quadDistanceFromCamera;
        _quadTransform.rotation = _camera.transform.rotation;
        _quadTransform.localScale = new Vector3(orthoSize * 2f * aspect, orthoSize * 2f, 1f);
    }

    private void UpdateMaterialProperties(MMConeOfVision2D cone)
    {
        Vector3 playerPosition = playerTransform.position;
        float viewAngleDegrees = cone.EulerAngles.y;
        float coneHalfAngle = cone.VisionAngle * 0.5f;
        float coneRadius = cone.VisionRadius;

        _quadRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetVector(PlayerWorldPosId, playerPosition);
        _propertyBlock.SetFloat(ViewAngleId, viewAngleDegrees);
        _propertyBlock.SetFloat(ConeHalfAngleId, coneHalfAngle);
        _propertyBlock.SetFloat(ConeRadiusId, coneRadius);
        _propertyBlock.SetFloat(FadeWidthId, fadeWidth);
        _propertyBlock.SetColor(OverlayColorId, overlayColor);
        _propertyBlock.SetVector(CameraWorldPosId, _camera.transform.position);
        _propertyBlock.SetFloat(OrthoSizeId, _camera.orthographicSize);
        _propertyBlock.SetFloat(AspectRatioId, _camera.aspect);
        _quadRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void OnDestroy()
    {
        if (_quadTransform != null && _quadTransform.gameObject != null)
            Destroy(_quadTransform.gameObject);
    }
}
