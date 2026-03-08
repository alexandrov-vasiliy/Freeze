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

    [Header("Замирание игрока")]
    [SerializeField]
    [Tooltip("Дистанция до игрока, внутри которой монстр останавливается, если игрок стоит. Если монстр дальше — подходит, не останавливаясь")]
    private float stopNearPlayerDistance = 2f;

    [SerializeField]
    [Tooltip("Сколько секунд игрок должен стоять на месте (монстр уже остановился рядом), чтобы монстр ушёл к точке отхода")]
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
    private MonsterView _monsterView;
    private bool _retreatRequested;

    /// <summary> Текущее состояние монстра (для MonsterView и прочих). </summary>
    public MonsterState CurrentState => _state;

    /// <summary> Монстр стоит рядом с игроком (Idle или Chase в зоне остановки). Для отключения анимации движения. </summary>
    public bool IsStandingNearPlayer
    {
        get
        {
            if (_playerTransform == null) return false;
            if (_state == MonsterState.Idle) return true;
            if (_state == MonsterState.Chase && Vector2.Distance(transform.position, _playerTransform.position) <= stopNearPlayerDistance)
                return true;
            return false;
        }
    }

    /// <summary> Вызывается при старте преследования (Idle → Chase). </summary>
    public event Action OnChaseStarted;

    /// <summary> Для отладки в редакторе: индекс текущей точки отхода. </summary>
    public int DebugRetreatPointIndex => _currentRetreatPointIndex;

    /// <summary> Для отладки в редакторе: таймер неподвижности игрока (сек). </summary>
    public float DebugStillTimer => _stillTimer;

    /// <summary> Для отладки в редакторе: запрошен отход (ждём анимацию). </summary>
    public bool DebugRetreatRequested => _retreatRequested;

    /// <summary> Для отладки в редакторе: куда идёт монстр (игрок при Chase, точка при Retreat). </summary>
    public Vector3? GetDebugTargetPosition()
    {
        if (_state == MonsterState.Chase && _playerTransform != null)
            return _playerTransform.position;
        if (_state == MonsterState.Retreat && retreatPoints != null && retreatPoints.Length > 0)
        {
            int index = _currentRetreatPointIndex >= retreatPoints.Length ? 0 : _currentRetreatPointIndex;
            if (retreatPoints[index].point != null)
                return retreatPoints[index].point.position;
        }
        return null;
    }

    private void Awake()
    {
        _monsterView = GetComponent<MonsterView>();
    }

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
        OnChaseStarted?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out Player player))
            SetPlayerAndStartChase(player);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent<Player>(out _))
            return;
        // В режиме отхода игрок сам уходит из триггера — не сбрасывать состояние, идём к точкам
        if (_state == MonsterState.Retreat)
        {
            _playerTransform = null;
            _playerActivityDetector = null;
            return;
        }
        _playerTransform = null;
        _playerActivityDetector = null;
        _state = MonsterState.Idle;
        _retreatRequested = false;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        switch (_state)
        {
            case MonsterState.Idle:
                UpdateIdle(deltaTime);
                break;

            case MonsterState.Chase:
                UpdateChase(deltaTime);
                break;

            case MonsterState.Retreat:
                UpdateRetreat(deltaTime);
                break;
        }
    }

    private void UpdateIdle(float deltaTime)
    {
        // Idle рядом с игроком: ждём — либо игрок снова двинется (вернёмся в Chase), либо истечёт таймер (уход к точке)
        if (_playerTransform == null || _playerActivityDetector == null)
            return;

        if (!_playerActivityDetector.IsCompletelyStill)
        {
            _state = MonsterState.Chase;
            return;
        }

        _stillTimer += deltaTime;
        if (_stillTimer >= stillDetectionInterval && !_retreatRequested)
        {
            _retreatRequested = true;
            TryStartRetreat();
        }
    }

    private void UpdateChase(float deltaTime)
    {
        if (_playerTransform == null)
        {
            _state = MonsterState.Idle;
            _retreatRequested = false;
            return;
        }

        Vector2 from = transform.position;
        float distanceToPlayer = Vector2.Distance(from, _playerTransform.position);

        if (_playerActivityDetector != null && _playerActivityDetector.IsCompletelyStill
            && distanceToPlayer <= stopNearPlayerDistance)
        {
            // Игрок стоит и монстр уже рядом — останавливаемся; таймер не сбрасываем, чтобы накапливалось время в Idle
            _state = MonsterState.Idle;
            _retreatRequested = false;
            return;
        }

        // Игрок далеко или двигается — продолжаем преследование; сбрасываем таймер только когда реально движемся
        _stillTimer = 0f;
        Vector2 to = _playerTransform.position;
        Vector2 direction = GetMovementDirection(from, to, Time.time, chaseMovementStyle);
        transform.position = from + direction * (chaseSpeed * deltaTime);
    }

    private void TryStartRetreat()
    {
        if (retreatPoints == null || retreatPoints.Length == 0)
        {
            _state = MonsterState.Idle;
            _retreatRequested = false;
            return;
        }
        if (_monsterView != null)
            _monsterView.RequestRetreat(OnRetreatReady);
        else
            OnRetreatReady();
    }

    private void OnRetreatReady()
    {
        _currentRetreatPointIndex = 0;
        _state = MonsterState.Retreat;
    }

    private void UpdateRetreat(float deltaTime)
    {
        if (retreatPoints == null || retreatPoints.Length == 0)
        {
            _state = MonsterState.Idle;
            _currentRetreatPointIndex = 0;
            _retreatRequested = false;
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
            // Дошли до точки — переходим к следующей (по кругу)
            _currentRetreatPointIndex++;
            if (_currentRetreatPointIndex >= retreatPoints.Length)
                _currentRetreatPointIndex = 0;
            return;
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
