using UnityEngine;
using UnityEngine.UI;

public class KeyBarUI : MonoBehaviour
{
    public Image fillImage;      // drag keyBarFill here
    public int keysToFull = 5;

    private int keysCollected = 0;

    void Start()
    {
        UpdateBar();
    }

    public void AddKey(int amount = 1)
    {
        keysCollected += amount;
        keysCollected = Mathf.Clamp(keysCollected, 0, keysToFull);
        UpdateBar();

        // Accumulate lifetime keys for the skin shop
        if (SkinManager.Instance != null)
            SkinManager.Instance.AddLifetimeKeys(amount);
    }

    public bool IsFull()
    {
        return keysCollected >= keysToFull;
    }

    public bool SpendKeys(int amount)
{
    if (keysCollected < amount) return false;

    keysCollected -= amount;
    keysCollected = Mathf.Clamp(keysCollected, 0, keysToFull);
    UpdateBar();
    return true;
}

public void ResetKeys()
{
    keysCollected = 0;
    UpdateBar();
}

    void UpdateBar()
    {
        if (fillImage == null) return;

        float percent = (float)keysCollected / keysToFull;
        fillImage.fillAmount = Mathf.Clamp01(percent);
    }
}
