using UnityEngine;

public class DestroyFish : MonoBehaviour
{
    public float destroyBehindPlayer = 25f;

    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        // If fish is far behind the player, destroy it
        if (transform.position.x < player.position.x - destroyBehindPlayer)
        {
            Destroy(gameObject);
        }
    }
}