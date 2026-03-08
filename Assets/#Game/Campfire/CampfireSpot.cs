using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;
using FunkyCode;

public class CampfireSpot : MonoBehaviour
{
    public enum CampfireState
    {
        Empty,
        WithWood,
        Lit
    }

    [System.Serializable]
    public struct ResourceCost
    {
        public ResourceType type;
        public int amount;
    }

    [Header("Состояние")]
    [SerializeField] private CampfireState currentState = CampfireState.Empty;

    [Header("Цена за дрова")]
    [SerializeField] private ResourceCost[] costToAddWood;

    [Header("Цена за поджиг")]
    [SerializeField] private ResourceCost[] costToIgnite;

    [Header("Задержки")]
    [SerializeField] private float addWoodDelay = 0.2f;
    [SerializeField] private float igniteDelay = 0.2f;

    [Header("Визуал")]
    [SerializeField] private SpriteRenderer baseRenderer;
    [SerializeField] private Sprite spriteEmpty;
    [SerializeField] private Sprite spriteWithWood;
    [SerializeField] private Sprite spriteLitBase;

    [SerializeField] private GameObject fireVisual;
    [SerializeField] private GameObject warmZone;

    [Header("Звук")]
    [SerializeField] private AudioSource oneShotAudioSource;
    [SerializeField] private AudioClip addWoodClip;
    [SerializeField] private AudioClip igniteClip;
    [SerializeField] private AudioSource campfireLoopAudioSource;

    [Header("Фидбэк")]
    [SerializeField] private MMF_Player addWoodFeedbackPlayer;
    [SerializeField] private MMF_Player igniteFeedbackPlayer;

    [Header("Свет")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private FireLightFlicker fireLightFlicker;

    private Collider2D _playerColliderInRange;
    private bool _isBusy;
    
    public bool IsLit => currentState == CampfireState.Lit;

    private void Start()
    {
        addWoodFeedbackPlayer?.Initialization();
        igniteFeedbackPlayer?.Initialization();

        ApplyStateVisualsImmediate();
    }

    private void Update()
    {
        if (_playerColliderInRange == null || _isBusy || !Input.GetKeyDown(KeyCode.E))
            return;

        switch (currentState)
        {
            case CampfireState.Empty:
                TryInteractFromEmpty();
                break;

            case CampfireState.WithWood:
                TryInteractFromWithWood();
                break;
        }
    }

    private void TryInteractFromEmpty()
    {
        bool canAddWood = HasEnoughResources(costToAddWood);
        bool canIgnite = HasEnoughResources(costToIgnite);

        // Если есть всё сразу, делаем оба этапа за одно нажатие
        if (canAddWood && canIgnite)
        {
            StartCoroutine(AddWoodAndIgniteRoutine());
            return;
        }

        // Если есть только палки
        if (canAddWood)
        {
            StartCoroutine(AddWoodOnlyRoutine());
            return;
        }

        // Если есть только спички, но нет палок - ничего не делаем
    }

    private void TryInteractFromWithWood()
    {
        if (!HasEnoughResources(costToIgnite))
            return;

        StartCoroutine(IgniteOnlyRoutine());
    }

    private IEnumerator AddWoodOnlyRoutine()
    {
        _isBusy = true;

        if (!TrySpendAllCost(costToAddWood))
        {
            _isBusy = false;
            yield break;
        }

        PlayOneShot(addWoodClip);
        addWoodFeedbackPlayer?.PlayFeedbacks();

        if (addWoodDelay > 0f)
            yield return new WaitForSeconds(addWoodDelay);

        currentState = CampfireState.WithWood;
        RefreshBaseVisual();

        _isBusy = false;
    }

    private IEnumerator IgniteOnlyRoutine()
    {
        _isBusy = true;

        if (!TrySpendAllCost(costToIgnite))
        {
            _isBusy = false;
            yield break;
        }

        PlayOneShot(igniteClip);
        igniteFeedbackPlayer?.PlayFeedbacks();

        if (igniteDelay > 0f)
            yield return new WaitForSeconds(igniteDelay);

        currentState = CampfireState.Lit;
        RefreshBaseVisual();
        TurnOnFireEffects();

        _isBusy = false;
    }

    private IEnumerator AddWoodAndIgniteRoutine()
    {
        _isBusy = true;

        // Сначала списываем палки
        if (!TrySpendAllCost(costToAddWood))
        {
            _isBusy = false;
            yield break;
        }

        PlayOneShot(addWoodClip);
        addWoodFeedbackPlayer?.PlayFeedbacks();

        if (addWoodDelay > 0f)
            yield return new WaitForSeconds(addWoodDelay);

        currentState = CampfireState.WithWood;
        RefreshBaseVisual();

        // Потом списываем поджиг
        if (!TrySpendAllCost(costToIgnite))
        {
            _isBusy = false;
            yield break;
        }

        PlayOneShot(igniteClip);
        igniteFeedbackPlayer?.PlayFeedbacks();

        if (igniteDelay > 0f)
            yield return new WaitForSeconds(igniteDelay);

        currentState = CampfireState.Lit;
        RefreshBaseVisual();
        TurnOnFireEffects();

        _isBusy = false;
    }

    private void ApplyStateVisualsImmediate()
    {
        RefreshBaseVisual();

        bool isLit = currentState == CampfireState.Lit;

        if (fireVisual != null)
            fireVisual.SetActive(isLit);

        if (warmZone != null)
            warmZone.SetActive(isLit);

        if (light2D != null)
            light2D.enabled = isLit;

        if (fireLightFlicker != null)
            fireLightFlicker.enabled = isLit;

        if (campfireLoopAudioSource != null)
        {
            if (isLit)
            {
                if (!campfireLoopAudioSource.isPlaying)
                    campfireLoopAudioSource.Play();
            }
            else
            {
                campfireLoopAudioSource.Stop();
            }
        }
    }

    private void RefreshBaseVisual()
    {
        if (baseRenderer == null)
            return;

        switch (currentState)
        {
            case CampfireState.Empty:
                baseRenderer.sprite = spriteEmpty;
                break;

            case CampfireState.WithWood:
                baseRenderer.sprite = spriteWithWood;
                break;

            case CampfireState.Lit:
                baseRenderer.sprite = spriteLitBase;
                break;
        }
    }

    private void TurnOnFireEffects()
    {
        if (fireVisual != null)
            fireVisual.SetActive(true);

        if (warmZone != null)
            warmZone.SetActive(true);

        if (campfireLoopAudioSource != null && !campfireLoopAudioSource.isPlaying)
            campfireLoopAudioSource.Play();

        if (light2D != null)
            light2D.enabled = true;

        if (fireLightFlicker != null)
            fireLightFlicker.enabled = true;
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (oneShotAudioSource != null && clip != null)
            oneShotAudioSource.PlayOneShot(clip);
    }

    private bool HasEnoughResources(ResourceCost[] costs)
    {
        if (G.Resources == null || costs == null)
            return false;

        for (int i = 0; i < costs.Length; i++)
        {
            if (!G.Resources.CanSpend(costs[i].type, costs[i].amount))
                return false;
        }

        return true;
    }

    private bool TrySpendAllCost(ResourceCost[] costs)
    {
        if (G.Resources == null || costs == null)
            return false;

        int spentCount = 0;

        for (int i = 0; i < costs.Length; i++)
        {
            if (!G.Resources.TrySpend(costs[i].type, costs[i].amount))
            {
                for (int j = 0; j < spentCount; j++)
                    G.Resources.Add(costs[j].type, costs[j].amount);

                return false;
            }

            spentCount++;
        }

        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out _))
        {
            _playerColliderInRange = other;
            if (currentState != CampfireState.Lit)
            {
                other.GetComponent<InteractorDisplayer>().Show(gameObject.GetComponent<HintData>().hintText);
            }

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _playerColliderInRange)
        {
            _playerColliderInRange = null;
            other.GetComponent<InteractorDisplayer>().Hide();
        }
    }
}