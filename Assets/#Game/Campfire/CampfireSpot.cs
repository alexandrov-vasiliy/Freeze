using UnityEngine;
using MoreMountains.Feedbacks;
using FunkyCode;

/// <summary>
/// Место для костра: в триггере при нажатии E и достаточных ресурсах поджигает костёр —
/// смена спрайта, звук поджигания, зацикленный звук костра, включение Light2D и анимации ряби света.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CampfireSpot : MonoBehaviour
{
    [System.Serializable]
    public struct ResourceCost
    {
        public ResourceType type;
        public int amount;
    }

    [Header("Ресурсы для поджигания")]
    [SerializeField]
    [Tooltip("Какие ресурсы и в каком количестве нужны для разжигания")]
    private ResourceCost[] costToLight = new ResourceCost[0];

    [Header("Визуал")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    [Tooltip("Спрайт костра до поджигания")]
    private Sprite spriteUnlit;

    [SerializeField]
    [Tooltip("Спрайт горящего костра")]
    private Sprite spriteLit;

    [Header("Звук и фидбэк")]
    [SerializeField]
    [Tooltip("Один раз проигрывается при поджигании")]
    private MMF_Player igniteFeedbackPlayer;

    [SerializeField]
    [Tooltip("Зацикленный звук горящего костра")]
    private AudioSource campfireLoopAudioSource;

    [Header("Свет")]
    [SerializeField]
    [Tooltip("2D-свет костра (обычно на дочернем объекте), включается при поджигании")]
    private Light2D light2D;

    [SerializeField]
    [Tooltip("Компонент ряби света, включается при поджигании")]
    private FireLightFlicker fireLightFlicker;

    private Collider2D _playerColliderInRange;
    private bool _isLit;

    private void Start()
    {
        if (igniteFeedbackPlayer != null)
            igniteFeedbackPlayer.Initialization();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteUnlit != null)
            spriteRenderer.sprite = spriteUnlit;

        if (light2D == null)
            light2D = GetComponentInChildren<Light2D>(true);
        if (light2D != null)
            light2D.enabled = false;

        if (fireLightFlicker == null)
            fireLightFlicker = GetComponentInChildren<FireLightFlicker>(true);
        if (fireLightFlicker != null)
            fireLightFlicker.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out _))
        {
            _playerColliderInRange = other;
        }

        
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out _))
        {
            if (_isLit)
            {
                other.GetComponent<Player>().campFireColliderInRange = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _playerColliderInRange)
        {
            other.GetComponent<Player>().campFireColliderInRange = false;
            _playerColliderInRange = null;
        }

    }

    private void Update()
    {
        if (_isLit || _playerColliderInRange == null || !Input.GetKeyDown(KeyCode.E))
            return;

        if (G.Resources == null || !HasEnoughResources())
            return;

        if (!TrySpendAllCost())
            return;

        LightFire();
    }

    /// <summary>
    /// Проверяет, хватает ли ресурсов для поджигания (без списания).
    /// </summary>
    public bool HasEnoughResources()
    {
        if (G.Resources == null || costToLight == null)
            return false;

        for (int i = 0; i < costToLight.Length; i++)
        {
            if (!G.Resources.CanSpend(costToLight[i].type, costToLight[i].amount))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Пытается списать все ресурсы из costToLight. При неудаче возвращает ресурсы обратно.
    /// </summary>
    private bool TrySpendAllCost()
    {
        int spentCount = 0;
        for (int i = 0; i < costToLight.Length; i++)
        {
            if (!G.Resources.TrySpend(costToLight[i].type, costToLight[i].amount))
            {
                for (int j = 0; j < spentCount; j++)
                    G.Resources.Add(costToLight[j].type, costToLight[j].amount);
                return false;
            }
            spentCount++;
        }

        return true;
    }

    private void LightFire()
    {
        _isLit = true;

        if (spriteRenderer != null && spriteLit != null)
            spriteRenderer.sprite = spriteLit;

        igniteFeedbackPlayer?.PlayFeedbacks();

        if (campfireLoopAudioSource != null)
            campfireLoopAudioSource.Play();

        if (light2D != null)
            light2D.enabled = true;

        if (fireLightFlicker != null)
            fireLightFlicker.enabled = true;
    }
}
