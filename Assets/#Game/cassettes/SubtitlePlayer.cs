using System.Collections;
using TMPro;
using UnityEngine;

public class SubtitlePlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private float typingSpeed = 0.04f;
    [SerializeField] private float delayBetweenLines = 1f;

    private Coroutine _playRoutine;

    public void Play(string[] lines)
    {
        if (lines == null || lines.Length == 0 || subtitleText == null)
            return;

        if (_playRoutine != null)
            StopCoroutine(_playRoutine);

        _playRoutine = StartCoroutine(PlayRoutine(lines));
    }

    private IEnumerator PlayRoutine(string[] lines)
    {
        foreach (var line in lines)
        {
            subtitleText.text = string.Empty;

            foreach (char c in line)
            {
                subtitleText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(delayBetweenLines);
        }

        subtitleText.text = string.Empty;
        _playRoutine = null;
    }
}