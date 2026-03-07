using System;
using UnityEngine;

public class TemperatureStat : MonoBehaviour
{
    [Header("Temperature")]
    [SerializeField] private float maxTemperature = 100f;
    [SerializeField] private float currentTemperature = 100f;

    [Header("Rates per second")]
    [SerializeField] private float freezeRateIdle = 2f;
    [SerializeField] private float freezeRateMoving = 0.02f;
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

        float rate;

        if (_player != null && _player.campFireColliderInRange)
            rate = heatRate;
        else if (_movement != null && _movement.isMoving)
            rate = -freezeRateMoving;
        else
            rate = -freezeRateIdle;
        
        ChangeTemperature(rate * deltaTimeToUse);
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

}
