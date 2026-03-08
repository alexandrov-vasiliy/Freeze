using UnityEngine;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownMovement2D : MonoBehaviour
{
    /// <summary>
    /// Текущее направление движения (нормализованное). (0,0) когда персонаж стоит.
    /// Используется PlayerView для Blend Tree анимаций.
    /// </summary>
    public Vector2 MovementDirection { get; private set; }

    public bool isMoving;
    
    [SerializeField]
    [Tooltip("Скорость движения в единицах в секунду")]
    private float movementSpeed = 5f;

    [Header("Ввод (Legacy Input Manager)")]
    [SerializeField]
    [Tooltip("Имя оси для горизонтального движения (Edit → Project Settings → Input Manager)")]
    private string horizontalAxisName = "Horizontal";

    [SerializeField]
    [Tooltip("Имя оси для вертикального движения")]
    private string verticalAxisName = "Vertical";

    [Header("Фидбэк шагов (Feel)")]
    [SerializeField]
    [Tooltip("MMF Player для проигрывания фидбэка шага при движении. Настройте внутри него звук/вибрацию/эффекты.")]
    private MMF_Player stepFeedbackPlayer;

    [SerializeField]
    [Tooltip("Интервал в секундах между проигрываниями шага")]
    private float stepInterval = 0.35f;

    private Rigidbody2D _rigidbody;
    private float _stepCooldown;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (stepFeedbackPlayer != null)
        {
            stepFeedbackPlayer.Initialization();
        }
    }

    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis(horizontalAxisName);
        float vertical = Input.GetAxis(verticalAxisName);
        Vector2 direction = new Vector2(horizontal, vertical).normalized;
        MovementDirection = direction;
        Vector2 movement = direction * movementSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + movement);

        if (stepFeedbackPlayer != null && direction.sqrMagnitude > 0.01f)
        {
            isMoving = true;
            _stepCooldown -= Time.fixedDeltaTime;
            if (_stepCooldown <= 0f)
            {
                _stepCooldown = stepInterval;
                stepFeedbackPlayer.PlayFeedbacks(transform.position);
            }
        }
        else
        {
            isMoving = false;
            _stepCooldown = 0f;
        }
    }
}
