using UnityEngine;

public class FishSwim : MonoBehaviour
{
    [Header("Normal Swim")]
    public float swimSpeed = 4.5f;
    public Vector2 normalDir = Vector2.left;
    public float returnSmooth = 6f;

    [Header("Wiggle")]
    public float wiggleAmount = 0.25f;
    public float wiggleSpeed = 6f;

    [Header("Scatter")]
    public float scareRadius = 5.5f;
    public float scatterSpeed = 12f;          // not too crazy
    public float scatterDirChangeCooldown = 0.15f; // prevents jitter
    public float scatterSmooth = 14f;          // how quickly it turns toward scatter dir

    private Transform player;
    private Vector2 currentDir;
    private Vector2 scatterDir;
    private float nextScatterPickTime;
    private float wiggleOffset;

    void Start()
    {
        currentDir = normalDir.normalized;
        wiggleOffset = Random.Range(0f, 100f);

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        PickScatterDirection(); // initial random
    }

    void Update()
    {
if (player == null)
{
    var p = GameObject.FindGameObjectWithTag("Player");
    if (p != null) player = p.transform;
}

        // small wiggle so fish feel alive (doesn't change direction)
        float wiggle = Mathf.Sin((Time.time + wiggleOffset) * wiggleSpeed) * wiggleAmount;
        Vector3 wiggleMove = new Vector3(0f, wiggle, 0f) * Time.deltaTime;

        bool scared = false;

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            scared = dist <= scareRadius;
        }

        if (scared)
        {
            // pick a new random scatter direction occasionally while scared
            if (Time.time >= nextScatterPickTime)
            {
                PickScatterDirection();
                nextScatterPickTime = Time.time + scatterDirChangeCooldown;
            }

           currentDir = Vector2.MoveTowards(currentDir, scatterDir, Time.deltaTime * (scatterSmooth * 2f)).normalized;
            // move
            Vector3 move = (Vector3)(currentDir * scatterSpeed * Time.deltaTime);
            transform.position += move + wiggleMove;
        }
        else
        {
            // return to normal left direction smoothly
            currentDir = Vector2.Lerp(currentDir, normalDir.normalized, Time.deltaTime * returnSmooth).normalized;

            Vector3 move = (Vector3)(currentDir * swimSpeed * Time.deltaTime);
            transform.position += move + wiggleMove;
        }

        

// Face movement direction (sprite originally faces LEFT)
if (currentDir.x != 0)
{
    Vector3 scale = transform.localScale;

    if (currentDir.x > 0)
        scale.x = -Mathf.Abs(scale.x);   // moving RIGHT -> flip
    else
        scale.x = Mathf.Abs(scale.x);    // moving LEFT -> keep original

    transform.localScale = scale;
}
    }

  void PickScatterDirection()
{
    if (player == null)
    {
        scatterDir = Random.insideUnitCircle.normalized;
        return;
    }

    // Base "away from player"
    Vector2 away = ((Vector2)transform.position - (Vector2)player.position).normalized;

    // Perpendicular (sideways) direction (either up-ish or down-ish)
    Vector2 perp = new Vector2(-away.y, away.x);
    if (Random.value < 0.5f) perp = -perp;

    // Mix away + sideways so the school spreads out
    // Bigger sideways -> more "scatter", but still mostly away
    float sideways = Random.Range(0.35f, 0.85f);

    Vector2 dir = (away + perp * sideways).normalized;

    // Safety: ensure we aren't pointing toward player
    if (Vector2.Dot(dir, away) < 0.2f)
        dir = away;

    scatterDir = dir;
}
}