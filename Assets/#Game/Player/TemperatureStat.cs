using System;
using UnityEngine;

public class TemperatureStat : MonoBehaviour
{
    [Header("Temperature")]
    [SerializeField] private float maxTemperature = 100f;
    [SerializeField] private float currentTemperature = 100f;

    [Header("Rates per second")]
    [SerializeField] private float freezeRateIdle = 2f;
    [SerializeField] private float freezeRateMoving = 2f;
    [SerializeField] private float heatRate = 3f;

    [Header("Update")]
    [SerializeField] private float temperatureTickInterval = 0f;

    public float CurrentTemperature => currentTemperature;
    public float MaxTemperature => maxTemperature;

    public event Action<float> OnTemperatureChanged;

    private TopDownMovement2D _movement;
    private Player _player;
    private float _temperatureTickTimer;

    private void Awake()
    {
        _movement = GetComponent<TopDownMovement2D>();
        _player = GetComponent<Player>();

        currentTemperature = Mathf.Clamp(currentTemperature, 0f, maxTemperature);
    }

    private void Update()
    {
        float deltaTimeToUse = Time.deltaTime;

        if (temperatureTickInterval > 0f)
        {
            _temperatureTickTimer += Time.deltaTime;

            if (_temperatureTickTimer < temperatureTickInterval)
                return;

            deltaTimeToUse = _temperatureTickTimer;
            _temperatureTickTimer = 0f;
        }

        float ratePerSecond;

        if (_player != null && _player.campFireColliderInRange)
            ratePerSecond = heatRate;
        else if (_movement != null && _movement.isMoving)
            ratePerSecond = -freezeRateMoving;
        else
            ratePerSecond = -freezeRateIdle;

        float delta = ratePerSecond * deltaTimeToUse;
        ChangeTemperature(delta);
    }

    public void ChangeTemperature(float delta)
    {
        float oldTemperature = currentTemperature;

        currentTemperature += delta;
        currentTemperature = Mathf.Clamp(currentTemperature, 0f, maxTemperature);

        if (!Mathf.Approximately(oldTemperature, currentTemperature))
        {
            OnTemperatureChanged?.Invoke(currentTemperature);
        }
    }

    public float GetColdPercent()
    {
        if (maxTemperature <= 0f)
            return 1f;

        return 1f - (currentTemperature / maxTemperature);
    }
}
