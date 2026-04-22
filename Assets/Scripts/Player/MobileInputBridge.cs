using UnityEngine;

public class MobileInputBridge : MonoBehaviour
{
    public static MobileInputBridge Instance { get; private set; }

    public bool SwimHeld  { get; set; }
    public bool InkHeld   { get; set; }
    public bool ShootHeld { get; set; }

    // One-frame pulse for chest interact (mirrors GetKeyDown behaviour)
    public bool InteractDown { get; private set; }
    public void TriggerInteract() { InteractDown = true; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void LateUpdate()
    {
        InteractDown = false;
    }
}
