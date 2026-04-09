using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject startMenuPanel;
    public GameObject helpPanel;
    public GameObject creditsPanel;
    public string gameSceneName = "MainScene";

    [Header("Logo — hides when sub-panels open")]
    public GameObject logo;

    [Header("Achievements")]
    public AchievementPanel achievementPanel;

    [Header("Skin Shop")]
    public SkinShopPanel skinShopPanel;

    void Start()
    {
        ShowStartMenu();
    }

    void SetLogo(bool visible)
    {
        // Use Inspector-assigned reference first; fall back to finding by name
        if (logo == null) logo = GameObject.Find("Logo");
        if (logo != null) logo.SetActive(visible);
    }

    public void ShowStartMenu()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(true);
        if (helpPanel      != null) helpPanel.SetActive(false);
        if (creditsPanel   != null) creditsPanel.SetActive(false);
        SetLogo(true);
    }

    public void ShowHelp()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (helpPanel      != null) helpPanel.SetActive(true);
        if (creditsPanel   != null) creditsPanel.SetActive(false);
        SetLogo(false);
    }

    public void ShowCredits()
    {
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        if (helpPanel      != null) helpPanel.SetActive(false);
        if (creditsPanel   != null) creditsPanel.SetActive(true);
        SetLogo(false);
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
            SetLogo(true);
        };
        achievementPanel.Show();
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        SetLogo(false);
    }

    public void ShowSkinShop()
    {
        if (skinShopPanel == null) return;
        skinShopPanel.onClose = () =>
        {
            if (startMenuPanel != null) startMenuPanel.SetActive(true);
            SetLogo(true);
        };
        skinShopPanel.Show();
        if (startMenuPanel != null) startMenuPanel.SetActive(false);
        SetLogo(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
