using UnityEngine;

public class RockVisual : MonoBehaviour
{
    [SerializeField] private Sprite[] rockSprites;

    private void Awake()
{
    if (rockSprites.Length == 0) return;

    int index = Random.Range(0, rockSprites.Length);

    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    sr.sprite = rockSprites[index];
}

}
