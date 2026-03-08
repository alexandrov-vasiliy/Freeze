using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Отвечает за анимации и звуки монстра: появление перед игроком, полз под землёй при преследовании, уход под землю перед отходом к точке.
/// </summary>
[RequireComponent(typeof(MonsterChaseRetreat))]
public class MonsterView : MonoBehaviour
{
    [Header("Аниматор")]
    [SerializeField]
    [Tooltip("Если не задан — берётся GetComponent на этом объекте")]
    private Animator animator;

    [Header("Параметры аниматора")]
    [SerializeField]
    [Tooltip("Триггер: анимация появления перед игроком")]
    private string appearTriggerName = "Appear";

    [SerializeField]
    [Tooltip("Булев параметр: ползёт под землёй (включить при Chase)")]
    private string crawlBoolName = "IsCrawling";

    [SerializeField]
    [Tooltip("Триггер: анимация ухода под землю")]
    private string disappearTriggerName = "Disappear";

    [Header("Анимация ухода")]
    [SerializeField]
    [Tooltip("Длительность анимации ухода в секундах; после неё вызывается колбэк. Можно использовать Animation Event вместо этого.")]
    private float disappearAnimationDuration = 1f;

    [Header("Звуки анимаций (кроме Move)")]
    [SerializeField]
    [Tooltip("Звук появления — когда монстр подходит / появляется перед игроком; по очереди")]
    private AudioClip[] appearSounds = Array.Empty<AudioClip>();

    [SerializeField]
    [Tooltip("Звук преследования — когда монстр начинает идти за игроком; по очереди")]
    private AudioClip[] chaseSounds = Array.Empty<AudioClip>();

    [SerializeField]
    [Tooltip("Звук при уходе под землю; по очереди при каждом уходе к точке")]
    private AudioClip[] disappearSounds = Array.Empty<AudioClip>();

    [SerializeField]
    [Tooltip("Источник звука (если не задан — GetComponent)")]
    private AudioSource audioSource;

    private MonsterChaseRetreat _monsterChaseRetreat;
    private int _appearTriggerHash;
    private int _crawlBoolHash;
    private int _disappearTriggerHash;
    private Action _onRetreatReady;
    private Coroutine _disappearCoroutine;
    private int _nextAppearSoundIndex;
    private int _nextChaseSoundIndex;
    private int _nextDisappearSoundIndex;
    private bool _isDisappearing;
    private bool _wasMovingLastFrame;
    private const float AnimatorNormalSpeed = 1f;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        _appearTriggerHash = Animator.StringToHash(appearTriggerName);
        _crawlBoolHash = Animator.StringToHash(crawlBoolName);
        _disappearTriggerHash = Animator.StringToHash(disappearTriggerName);
    }

    private void OnEnable()
    {
        _monsterChaseRetreat = GetComponent<MonsterChaseRetreat>();
        if (_monsterChaseRetreat != null)
            _monsterChaseRetreat.OnChaseStarted += OnChaseStarted;
    }

    private void OnDisable()
    {
        if (_monsterChaseRetreat != null)
            _monsterChaseRetreat.OnChaseStarted -= OnChaseStarted;
        if (_disappearCoroutine != null)
        {
            StopCoroutine(_disappearCoroutine);
            _disappearCoroutine = null;
        }
        _onRetreatReady = null;
    }

    private void Update()
    {
        if (animator == null || _monsterChaseRetreat == null)
            return;
        var state = _monsterChaseRetreat.CurrentState;
        bool isChase = state == MonsterChaseRetreat.MonsterState.Chase;
        bool isRetreat = state == MonsterChaseRetreat.MonsterState.Retreat;
        bool isStandingNearPlayer = _monsterChaseRetreat.IsStandingNearPlayer;
        bool isMoving = isChase && !isStandingNearPlayer;
        if (isMoving && !_wasMovingLastFrame)
            PlayNextChaseSound();
        _wasMovingLastFrame = isMoving;
        // Анимация полза: при преследовании, при отходе к точкам и когда стоим рядом с игроком (заморозим на последнем кадре)
        bool showCrawl = isChase || isRetreat || (isStandingNearPlayer && state == MonsterChaseRetreat.MonsterState.Idle);
        animator.SetBool(_crawlBoolHash, showCrawl);
        // Заморозка на последнем кадре, когда стоит у игрока; разморозка при движении и при анимации ухода
        bool freezeAnimation = isStandingNearPlayer && !_isDisappearing;
        animator.speed = freezeAnimation ? 0f : AnimatorNormalSpeed;
    }

    private void OnChaseStarted()
    {
        if (animator != null)
            animator.SetTrigger(_appearTriggerHash);
        PlayNextAppearSound();
    }

    private void PlayNextAppearSound()
    {
        if (appearSounds == null || appearSounds.Length == 0)
            return;
        AudioSource source = GetAudioSource();
        if (source == null)
            return;
        AudioClip clip = appearSounds[_nextAppearSoundIndex];
        if (clip != null)
            source.PlayOneShot(clip);
        _nextAppearSoundIndex = (_nextAppearSoundIndex + 1) % appearSounds.Length;
    }

    private void PlayNextChaseSound()
    {
        if (chaseSounds == null || chaseSounds.Length == 0)
            return;
        AudioSource source = GetAudioSource();
        if (source == null)
            return;
        AudioClip clip = chaseSounds[_nextChaseSoundIndex];
        if (clip != null)
            source.PlayOneShot(clip);
        _nextChaseSoundIndex = (_nextChaseSoundIndex + 1) % chaseSounds.Length;
    }

    private void PlayNextDisappearSound()
    {
        if (disappearSounds == null || disappearSounds.Length == 0)
            return;
        AudioSource source = GetAudioSource();
        if (source == null)
            return;
        AudioClip clip = disappearSounds[_nextDisappearSoundIndex];
        if (clip != null)
            source.PlayOneShot(clip);
        _nextDisappearSoundIndex = (_nextDisappearSoundIndex + 1) % disappearSounds.Length;
    }

    private AudioSource GetAudioSource()
    {
        return audioSource != null ? audioSource : GetComponent<AudioSource>();
    }

    /// <summary>
    /// Запускает анимацию ухода под землю; по окончании вызывает onRetreatReady (через длительность или Animation Event).
    /// </summary>
    public void RequestRetreat(Action onRetreatReady)
    {
        if (onRetreatReady == null)
            return;
        if (_disappearCoroutine != null)
            StopCoroutine(_disappearCoroutine);
        _onRetreatReady = onRetreatReady;
        _isDisappearing = true;
        if (animator != null)
            animator.speed = AnimatorNormalSpeed;
        PlayNextDisappearSound();
        if (animator != null)
            animator.SetTrigger(_disappearTriggerHash);
        _disappearCoroutine = StartCoroutine(WaitDisappearAndNotify());
    }

    private IEnumerator WaitDisappearAndNotify()
    {
        yield return new WaitForSeconds(disappearAnimationDuration);
        _disappearCoroutine = null;
        _isDisappearing = false;
        Action callback = _onRetreatReady;
        _onRetreatReady = null;
        callback?.Invoke();
    }

    /// <summary>
    /// Вызвать из Animation Event в клипе ухода под землю, чтобы уведомить о конце анимации без ожидания по таймеру.
    /// </summary>
    public void OnDisappearAnimationFinished()
    {
        if (_disappearCoroutine != null)
        {
            StopCoroutine(_disappearCoroutine);
            _disappearCoroutine = null;
        }
        _isDisappearing = false;
        Action callback = _onRetreatReady;
        _onRetreatReady = null;
        callback?.Invoke();
    }
}
