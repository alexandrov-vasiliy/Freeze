using UnityEngine;

/// <summary>
/// Определяет, полностью ли неподвижен игрок (нет ввода движения и нет движения курсора).
/// Используется монстром для перехода в отход при «замирании» игрока.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerActivityDetector : MonoBehaviour
{
    [Header("Порог движения курсора")]
    [SerializeField]
    [Tooltip("Минимальное смещение курсора в пикселях за кадр, чтобы считать, что курсор двигался")]
    private float cursorMovementThresholdPixels = 2.5f;

    private TopDownMovement2D _movement;
    private Vector3 _lastMousePosition;
    private bool _isMoving;
    private bool _cursorMoved;

    /// <summary>
    /// True, если игрок полностью неподвижен: не нажимает клавиши движения и не двигает курсор.
    /// </summary>
    public bool IsCompletelyStill => !_isMoving && !_cursorMoved;

    private void Awake()
    {
        _movement = GetComponent<TopDownMovement2D>();
        _lastMousePosition = Input.mousePosition;
    }

    private void Update()
    {
        _isMoving = _movement != null && _movement.isMoving;

        Vector3 currentMousePosition = Input.mousePosition;
        float cursorDelta = (currentMousePosition - _lastMousePosition).magnitude;
        _cursorMoved = cursorDelta >= cursorMovementThresholdPixels;
        _lastMousePosition = currentMousePosition;
    }
}
