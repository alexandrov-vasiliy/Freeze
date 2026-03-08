using UnityEngine;
using MoreMountains.Feedbacks;

/// <summary>
/// При нахождении игрока (компонент Player) в триггере и нажатии E добавляет ресурс и проигрывает фидбэк через MMF_Player.
/// На объекте должен быть Collider2D с isTrigger = true.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ResourcePick : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Какой ресурс выдаётся при подборе")]
    private ResourceType resourceType = ResourceType.Sticks;

    [SerializeField]
    [Tooltip("Сколько единиц добавлять за одно нажатие E")]
    private int amountPerPick = 1;

    [SerializeField]
    [Tooltip("MMF_Player для проигрывания фидбэка при подборе")]
    private MMF_Player pickFeedbackPlayer;

    [SerializeField] private SpriteRenderer _spriteRenderer;

    private Collider2D _playerColliderInRange;

    private void Start()
    {
        if (pickFeedbackPlayer != null)
            pickFeedbackPlayer.Initialization();

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out _))
        {
            _playerColliderInRange = other;
            G.hintText.SetActive(true);
        }

        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _playerColliderInRange)
        {
            G.hintText.SetActive(false);
            _playerColliderInRange = null;
        }
    }

    private void Update()
    {
        if (_playerColliderInRange == null || !Input.GetKeyDown(KeyCode.E))
            return;

        if (G.Resources == null)
            return;

        G.Resources.Add(resourceType, amountPerPick);
        pickFeedbackPlayer?.PlayFeedbacks();
        enabled = false;
        if(_spriteRenderer is null) return;
        
        _spriteRenderer.enabled = false;
    }
}
