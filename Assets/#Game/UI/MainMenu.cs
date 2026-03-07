using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private MainMenu startMenuPanel;

    private void Awake()
    {
        startMenuPanel = G.mainMenu;
        startMenuPanel.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        startMenuPanel.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
}