using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public Transform scoreTarget;      // Main Camera (auto-scroll)

    [Header("UI (TMP)")]
    public TMP_Text scoreText;         // "Meters"
    public TMP_Text lastText;          // "Last"
    public TMP_Text bestText;          // "Best"

    private float startX;
    private float bestScore;

    const string BEST_KEY = "BEST_SCORE";
    const string LAST_KEY = "LAST_SCORE";

    void Start()
    {
        // IMPORTANT: drag Main Camera into this in Inspector
        if (scoreTarget == null)
            scoreTarget = Camera.main.transform;

        startX = scoreTarget.position.x;
        bestScore = PlayerPrefs.GetFloat(BEST_KEY, 0f);

        UpdateUI(0f);
    }

    void Update()
    {
        float meters = Mathf.Max(0f, scoreTarget.position.x - startX);

        if (meters > bestScore)
        {
            bestScore = meters;
            PlayerPrefs.SetFloat(BEST_KEY, bestScore);
        }

        UpdateUI(meters);
    }

    public void SaveLastRun()
    {
        float meters = Mathf.Max(0f, scoreTarget.position.x - startX);
        PlayerPrefs.SetFloat(LAST_KEY, meters);
        PlayerPrefs.Save();

    }

    void UpdateUI(float meters)
{
    float last = PlayerPrefs.GetFloat(LAST_KEY, 0f);

    if (scoreText != null)
    {
        scoreText.text =
            $"Meters: {Mathf.FloorToInt(meters)}\n" +
            $"Last: {Mathf.FloorToInt(last)}\n" +
            $"Best: {Mathf.FloorToInt(bestScore)}";
    }
}

}
