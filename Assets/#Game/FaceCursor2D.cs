using UnityEngine;

/// <summary>
/// Поворачивает объект по оси Z в сторону курсора мыши (для 2D вида сверху).
/// </summary>
public class FaceCursor2D : MonoBehaviour
{
    [Header("Поворот за курсором")]
    [SerializeField]
    [Tooltip("Включить поворот за курсором")]
    private bool enableRotation = true;

    [SerializeField]
    [Tooltip("Камера для перевода позиции мыши в мировые координаты. Если не задана — используется Camera.main")]
    private Camera targetCamera;

    [SerializeField]
    [Tooltip("Z-координата плоскости мира для ScreenToWorldPoint (обычно 0 для 2D)")]
    private float worldPlaneZ = 0f;

    [SerializeField]
    [Tooltip("Смещение угла в градусах (0 = вправо по оси X, 90 = вверх)")]
    private float angleOffsetDegrees = 0f;

    [Header("Сглаживание поворота")]
    [SerializeField]
    [Tooltip("Сглаживание угла (0 = мгновенно, большие значения — плавнее)")]
    [Range(0f, 30f)]
    private float rotationSmoothingSpeed = 10f;

    private float _currentAngleDegrees;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        _currentAngleDegrees = transform.eulerAngles.z;
    }

    private void LateUpdate()
    {
        if (!enableRotation || targetCamera == null)
            return;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(targetCamera.transform.position.z - worldPlaneZ);
        Vector3 mouseWorld = targetCamera.ScreenToWorldPoint(mouseScreen);

        Vector2 fromObjectToMouse = new Vector2(
            mouseWorld.x - transform.position.x,
            mouseWorld.y - transform.position.y);

        if (fromObjectToMouse.sqrMagnitude < 0.0001f)
            return;

        float targetAngleDegrees = Mathf.Atan2(fromObjectToMouse.y, fromObjectToMouse.x) * Mathf.Rad2Deg + angleOffsetDegrees;

        if (rotationSmoothingSpeed > 0f)
            _currentAngleDegrees = Mathf.LerpAngle(_currentAngleDegrees, targetAngleDegrees, rotationSmoothingSpeed * Time.deltaTime);
        else
            _currentAngleDegrees = targetAngleDegrees;

        transform.rotation = Quaternion.Euler(0f, 0f, _currentAngleDegrees);
    }

    /// <summary>
    /// Текущий угол поворота по Z в градусах (0 = вправо).
    /// </summary>
    public float CurrentAngleDegrees => _currentAngleDegrees;
}
