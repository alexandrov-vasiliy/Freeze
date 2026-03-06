using UnityEngine;
using FunkyCode;

/// <summary>
/// Анимация «ряби» света костра: модулирует size и lightStrength у Light2D для реалистичного мерцания.
/// Включается отдельно (например, из CampfireSpot при поджигании).
/// </summary>
[RequireComponent(typeof(Light2D))]
public class FireLightFlicker : MonoBehaviour
{
    [Header("Ссылка на свет")]
    [SerializeField]
    [Tooltip("Если не задан — берётся с этого объекта")]
    private Light2D targetLight;

    [Header("Базовые значения (если 0 — берутся из Light2D при старте)")]
    [SerializeField]
    private float baseSize = 0f;

    [SerializeField]
    private float baseLightStrength = 0f;

    [Header("Рябь размера")]
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Амплитуда колебания размера (доля от базового)")]
    private float sizeFlickerAmount = 0.15f;

    [SerializeField]
    [Tooltip("Частота ряби размера")]
    private float sizeFlickerSpeed = 4f;

    [Header("Рябь интенсивности")]
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Амплитуда колебания интенсивности (доля от базовой)")]
    private float strengthFlickerAmount = 0.2f;

    [SerializeField]
    [Tooltip("Частота ряби интенсивности")]
    private float strengthFlickerSpeed = 5f;

    [Header("Опционально: лёгкое изменение оттенка")]
    [SerializeField]
    private bool enableColorFlicker;

    [SerializeField]
    private Color baseColor = new Color(1f, 0.6f, 0.2f, 1f);

    [SerializeField]
    [Range(0f, 0.2f)]
    private float colorFlickerAmount = 0.05f;

    private float _sizeUsed;
    private float _strengthUsed;
    private float _perlinOffset;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light2D>();
        _perlinOffset = Random.Range(0f, 1000f);
    }

    private void OnEnable()
    {
        if (targetLight == null)
            return;
        _sizeUsed = baseSize > 0f ? baseSize : targetLight.size;
        _strengthUsed = baseLightStrength > 0f ? baseLightStrength : targetLight.lightStrength;
    }

    private void LateUpdate()
    {
        if (targetLight == null)
            return;

        float time = Time.time + _perlinOffset;

        float sizeNoise = Mathf.PerlinNoise(time * sizeFlickerSpeed, 0f);
        float sizeMultiplier = 1f + (sizeNoise - 0.5f) * 2f * sizeFlickerAmount;
        targetLight.size = _sizeUsed * sizeMultiplier;

        float strengthNoise = Mathf.PerlinNoise(time * strengthFlickerSpeed, 100f);
        float strengthMultiplier = 1f + (strengthNoise - 0.5f) * 2f * strengthFlickerAmount;
        targetLight.lightStrength = _strengthUsed * strengthMultiplier;

        if (enableColorFlicker)
        {
            float colorNoise = Mathf.PerlinNoise(time * 3f, 200f);
            float t = (colorNoise - 0.5f) * 2f * colorFlickerAmount;
            targetLight.color = Color.Lerp(baseColor, baseColor * 1.1f, 0.5f + t);
        }
    }
}
