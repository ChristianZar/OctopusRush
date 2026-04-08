using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject startMenuPanel;
    public GameObject helpPanel;
    public GameObject creditsPanel;
    public string gameSceneName = "MainScene";

    [Header("Achievements")]
    public AchievementPanel achievementPanel;

    [Header("Skin Shop")]
    public SkinShopPanel skinShopPanel;

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

    public void ShowAchievements()
    {
        if (achievementPanel == null) return;
        achievementPanel.onClose = () =>
        {
            if (startMenuPanel != null) startMenuPanel.SetActive(true);
        };
        achievementPanel.Show();
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
    }

    public void ShowSkinShop()
    {
        if (skinShopPanel == null) return;
        skinShopPanel.onClose = () =>
        {
            if (startMenuPanel != null) startMenuPanel.SetActive(true);
        };
        skinShopPanel.Show();
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}