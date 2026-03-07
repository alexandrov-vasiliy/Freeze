using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TemperaturePanel : MonoBehaviour
{
    [SerializeField] private TemperatureStat temperatureStat;

    [Header("Alpha")]
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float maxAlpha = 0.6f;

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 0.01f;

    private CanvasGroup _canvasGroup;
    private float _targetAlpha;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (temperatureStat != null)
            temperatureStat.OnTemperatureChanged += HandleTemperatureChanged;
    }

    private void OnDisable()
    {
        if (temperatureStat != null)
            temperatureStat.OnTemperatureChanged -= HandleTemperatureChanged;
    }

    private void Start()
    {
        if (temperatureStat != null)
            HandleTemperatureChanged(temperatureStat.CurrentTemperature);
    }

    private void Update()
    {
        _canvasGroup.alpha = Mathf.MoveTowards(
            _canvasGroup.alpha,
            _targetAlpha,
            fadeSpeed * Time.deltaTime
        );
    }

    private void HandleTemperatureChanged(float temperature)
    {
        if (temperatureStat == null)
            return;

        float normalizedTemperature = temperature / temperatureStat.MaxTemperature;
        float coldPercent = 1f - normalizedTemperature;

        coldPercent = Mathf.Clamp01(coldPercent);

        _targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, coldPercent);
    }
}
