using UnityEngine;

/// <summary>
/// Дополнительная зона для монстра: при входе игрока сообщает монстру начать преследование.
/// Срабатывает один раз, после срабатывания зона деактивируется.
/// Монстр берётся из G.Monster (не нужно назначать в каждой зоне).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MonsterChaseZone : MonoBehaviour
{
    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        if (_collider != null)
            _collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (G.Monster == null)
            return;
        if (_collider != null && !_collider.enabled)
            return;
        if (!other.TryGetComponent<Player>(out _))
            return;

        G.Monster.NotifyPlayerEnteredZone(other);
        if (_collider != null)
            _collider.enabled = false;
    }
}
