using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AltarVictoryLoader : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Ссылка на костёр. Если не указана, попробует найти на этом же объекте.")]
    private CampfireSpot campfireSpot;

    [SerializeField]
    [Tooltip("Название победной сцены из Build Settings")]
    private string victorySceneName = "VictoryScene";

    [SerializeField]
    [Tooltip("Задержка перед загрузкой победной сцены")]
    private float delayBeforeLoad = 2f;

    private bool _isLoadingStarted;

    private void Awake()
    {
        if (campfireSpot == null)
            campfireSpot = GetComponent<CampfireSpot>();
    }

    private void Update()
    {
        if (_isLoadingStarted || campfireSpot == null)
            return;

        if (campfireSpot.IsLit)
        {
            StartCoroutine(LoadVictorySceneRoutine());
        }
    }

    private IEnumerator LoadVictorySceneRoutine()
    {
        _isLoadingStarted = true;

        if (delayBeforeLoad > 0f)
            yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(victorySceneName);
    }
}
