using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

public class TrapActivation : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField]
    [Tooltip("MMF_Player для проигрывания фидбэка при попадании в ловушку")]
    private MMF_Player trapFeedbackPlayer;

    [SerializeField]
    [Tooltip("Задержка после активации ловушки перед появлением панели смерти")]
    private float delayBeforeDeathPanel = 1f;

    [SerializeField]
    [Tooltip("Скорость появления панели смерти")]
    private float deathPanelFadeSpeed = 6f;

    [SerializeField]
    [Tooltip("До какого значения alpha проявлять панель")]
    private float targetAlpha = 1f;

    private bool _isActivated;
    //private TopDownMovement2D _movement;
    private CanvasGroup deathPanelCanvasGroup;

    private void Awake()
    {
        deathPanelCanvasGroup = G.DeathPanel.GetComponent<CanvasGroup>();
        
        //_movement = GetComponent<TopDownMovement2D>();
        if (deathPanelCanvasGroup != null)
        {
            deathPanelCanvasGroup.alpha = 0f;
            deathPanelCanvasGroup.interactable = false;
            deathPanelCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isActivated)
            return;

        if (other.TryGetComponent<Player>(out _))
        {
            _isActivated = true;
            other.gameObject.GetComponent<TopDownMovement2D>().enabled = false;
            trapFeedbackPlayer?.PlayFeedbacks();
            StartCoroutine(ShowDeathPanelWithDelay());
        }
    }

    private IEnumerator ShowDeathPanelWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeDeathPanel);

        if (deathPanelCanvasGroup == null)
            yield break;

        deathPanelCanvasGroup.interactable = true;
        deathPanelCanvasGroup.blocksRaycasts = true;

        while (deathPanelCanvasGroup.alpha < targetAlpha)
        {
            deathPanelCanvasGroup.alpha = Mathf.MoveTowards(
                deathPanelCanvasGroup.alpha,
                targetAlpha,
                deathPanelFadeSpeed * Time.deltaTime
            );

            yield return null;
        }
    }
}
