using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject startMenuPanel;
    public GameObject helpPanel;
    public GameObject creditsPanel;
    public string gameSceneName = "MainScene";

    void Start()
    {
        ShowStartMenu();
    }

    public void ShowStartMenu()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(true);
        if (helpPanel != null) helpPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowHelp()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}