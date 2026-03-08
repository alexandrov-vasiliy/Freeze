using UnityEngine;
using MoreMountains.Feedbacks;

/// <summary>
/// При нахождении игрока (компонент Player) в триггере и нажатии E добавляет ресурс.
/// Если ресурс типа Cassette, дополнительно проигрывает звук и субтитры из компонента CassetteData.
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

    [Header("Cassette")]
    private SubtitlePlayer subtitlePlayer;

    private Collider2D _playerColliderInRange;

    private void Start()
    {
        subtitlePlayer = G.subtitleText;
        
        if (pickFeedbackPlayer != null)
            pickFeedbackPlayer.Initialization();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out _))
        {
            _playerColliderInRange = other;
            other.GetComponent<InteractorDisplayer>().Show(gameObject.GetComponent<HintData>().hintText);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _playerColliderInRange)
        {
            other.GetComponent<InteractorDisplayer>().Hide();
            _playerColliderInRange = null;
        }
    }

    private void Update()
    {
        if (_playerColliderInRange == null || !Input.GetKeyDown(KeyCode.E))
            return;

        if (resourceType == ResourceType.Cassette)
        {
            PlayCassetteContent();
        }
        else
        {
            if (G.Resources == null)
                return;

            G.Resources.Add(resourceType, amountPerPick);
        }

        pickFeedbackPlayer?.PlayFeedbacks();

        enabled = false;

        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;
    }

    private void PlayCassetteContent()
    {
        CassetteData cassetteData = GetComponent<CassetteData>();
        if (cassetteData == null)
        {
            Debug.LogWarning($"На объекте {name} resourceType = Cassette, но нет компонента CassetteData");
            return;
        }

        if (cassetteData.CassetteAudio != null)
        {
            AudioSource.PlayClipAtPoint(
                cassetteData.CassetteAudio,
                transform.position,
                cassetteData.Volume
            );
        }

        if (subtitlePlayer != null && cassetteData.Subtitles != null && cassetteData.Subtitles.Length > 0)
        {
            subtitlePlayer.Play(cassetteData.Subtitles);
        }
    }
}