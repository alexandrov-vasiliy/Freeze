using TMPro;
using UnityEngine;

public class CassetteData : MonoBehaviour
{
    [Header("Cassette Audio")]
    [SerializeField] private AudioClip cassetteAudio;
    [SerializeField] private float volume = 1f;

    [Header("Cassette Subtitles")]
    [TextArea(2, 5)]
    [SerializeField] private string[] subtitles;

    public AudioClip CassetteAudio => cassetteAudio;
    public float Volume => volume;
    public string[] Subtitles => subtitles;
}