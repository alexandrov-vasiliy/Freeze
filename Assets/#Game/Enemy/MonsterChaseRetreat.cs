using System;
using UnityEngine;

/// <summary>
/// Враг-монстр: преследует игрока при входе в зону и движении игрока;
/// при полной неподвижности игрока уходит по цепочке точек отхода; при смене точки меняется тип движения.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MonsterChaseRetreat : MonoBehaviour
{
    public enum MovementStyle
    {
        Straight,
        Zigzag,
        Arc
    }

    public enum MonsterState
    {
        Idle,
        Chase,
        Retreat
    }

    [Serializable]
    public class RetreatPointEntry
    {
        [Tooltip("Точка отхода на сцене")]
        public Transform point;
        [Tooltip("Стиль движения к этой точке")]
        public MovementStyle movementStyle = MovementStyle.Straight;
    }

    [Header("Точки отхода (порядок прохождения: 0, 1, 2, …)")]
    [SerializeField]
    private RetreatPointEntry[] retreatPoints = Array.Empty<RetreatPointEntry>();

    [Header("Преследование")]
    [SerializeField]
    [Tooltip("Стиль движения при преследовании игрока")]
    private MovementStyle chaseMovementStyle = MovementStyle.Straight;

    [SerializeField]
    private float chaseSpeed = 4f;

    [Header("Отход")]
    [SerializeField]
    private float retreatSpeed = 3f;

    [SerializeField]
    [Tooltip("Дистанция, при которой монстр считается достигшим точки отхода")]
    private float retreatPointReachDistance = 0.15f;

    [SerializeField]
    [Tooltip("Минимальная дистанция до игрока, при которой монстр может перейти в Idle после отхода")]
    private float minDistanceFromPlayerToResumeIdle = 8f;

    [Header("Замирание игрока")]
    [SerializeField]
    [Tooltip("Сколько секунд игрок должен быть неподвижен, чтобы монстр перешёл в отход")]
    private float stillDetectionInterval = 0.8f;

    [Header("Зигзаг (для стиля Zigzag)")]
    [SerializeField]
    private float zigzagAmplitude = 0.4f;

    [SerializeField]
    private float zigzagFrequency = 4f;

    [Header("Дуга (для стиля Arc)")]
    [SerializeField]
    private float arcAmplitude = 0.5f;

    [SerializeField]
    private float arcFrequency = 2f;

    private MonsterState _state = MonsterState.Idle;
    private Transform _playerTransform;
    private PlayerActivityDetector _playerActivityDetector;
    private int _currentRetreatPointIndex;
    private float _stillTimer;

    private void OnEnable()
    {
        G.Monster = this;
    }

    private void OnDisable()
    {
        if (G.Monster == this)
            G.Monster = null;
    }

    /// <summary>
    /// Вызывается дополнительными зонами (MonsterChaseZone): игрок вошёл в зону — начать преследование.
    /// Зона после вызова должна сама деактивироваться.
    /// </summary>
    public void NotifyPlayerEnteredZone(Collider2D playerCollider)
    {
        if (playerCollider == null)
            return;
        if (!playerCollider.TryGetComponent<Player>(out Player player))
            return;
        SetPlayerAndStartChase(player);
    }

    private void SetPlayerAndStartChase(Player player)
    {
        _playerTransform = player.transform;
        _playerActivityDetector = player.GetComponent<PlayerActivityDetector>();
        _state = MonsterState.Chase;
        _stillTimer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out Player player))
            SetPlayerAndStartChase(player);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out _))
        {
            _playerTransform = null;
            _playerActivityDetector = null;
            _state = MonsterState.Idle;
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        switch (_state)
        {
            case MonsterState.Idle:
                break;

            case MonsterState.Chase:
                UpdateChase(deltaTime);
                break;

            case MonsterState.Retreat:
                UpdateRetreat(deltaTime);
                break;
        }
    }

    private void UpdateChase(float deltaTime)
    {
        if (_playerTransform == null)
        {
            _state = MonsterState.Idle;
            return;
        }

        if (_playerActivityDetector != null && _playerActivityDetector.IsCompletelyStill)
        {
            _stillTimer += deltaTime;
            if (_stillTimer >= stillDetectionInterval)
            {
                TryStartRetreat();
                return;
            }
        }
        else
        {
            _stillTimer = 0f;
        }

        Vector2 from = transform.position;
        Vector2 to = _playerTransform.position;
        Vector2 direction = GetMovementDirection(from, to, Time.time, chaseMovementStyle);
        transform.position = from + direction * (chaseSpeed * deltaTime);
    }

    private void TryStartRetreat()
    {
        if (retreatPoints == null || retreatPoints.Length == 0)
        {
            _state = MonsterState.Idle;
            return;
        }
        _currentRetreatPointIndex = 0;
        _state = MonsterState.Retreat;
    }

    private void UpdateRetreat(float deltaTime)
    {
        if (retreatPoints == null || retreatPoints.Length == 0)
        {
            _state = MonsterState.Idle;
            _currentRetreatPointIndex = 0;
            return;
        }

        // Бесконечный обход по кругу: после последней точки снова идём к первой
        if (_currentRetreatPointIndex >= retreatPoints.Length)
            _currentRetreatPointIndex = 0;

        RetreatPointEntry entry = retreatPoints[_currentRetreatPointIndex];
        if (entry.point == null)
        {
            _currentRetreatPointIndex++;
            return;
        }

        Vector2 from = transform.position;
        Vector2 to = entry.point.position;
        float distanceToPoint = Vector2.Distance(from, to);

        if (distanceToPoint < retreatPointReachDistance)
        {
            _currentRetreatPointIndex++;
            // По кругу: после последней точки переходим к первой, не в Idle
            if (_currentRetreatPointIndex >= retreatPoints.Length)
                _currentRetreatPointIndex = 0;
            return;
        }

        if (_playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(from, _playerTransform.position);
            if (distanceToPlayer > minDistanceFromPlayerToResumeIdle)
            {
                _state = MonsterState.Idle;
                _currentRetreatPointIndex = 0;
                return;
            }
        }

        Vector2 direction = GetMovementDirection(from, to, Time.time, entry.movementStyle);
        transform.position = from + direction * (retreatSpeed * deltaTime);
    }

    private Vector2 GetMovementDirection(Vector2 from, Vector2 to, float time, MovementStyle style)
    {
        Vector2 toTarget = to - from;
        float distance = toTarget.magnitude;
        if (distance < 0.001f)
            return Vector2.zero;

        Vector2 baseDirection = toTarget / distance;

        switch (style)
        {
            case MovementStyle.Straight:
                return baseDirection;

            case MovementStyle.Zigzag:
            {
                Vector2 perpendicular = new Vector2(-baseDirection.y, baseDirection.x);
                float offset = zigzagAmplitude * Mathf.Sin(time * zigzagFrequency);
                Vector2 result = baseDirection + perpendicular * offset;
                return result.sqrMagnitude > 0.001f ? result.normalized : baseDirection;
            }

            case MovementStyle.Arc:
            {
                Vector2 perpendicular = new Vector2(-baseDirection.y, baseDirection.x);
                float offset = arcAmplitude * Mathf.Sin(time * arcFrequency);
                Vector2 result = baseDirection + perpendicular * offset;
                return result.sqrMagnitude > 0.001f ? result.normalized : baseDirection;
            }

            default:
                return baseDirection;
        }
    }
}
