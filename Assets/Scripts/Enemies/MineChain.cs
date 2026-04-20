using System.Collections.Generic;
using UnityEngine;

/// Draws a sagging chain (quadratic bezier per link) between a cluster of mines.
/// Created automatically by MineSpawner; no manual setup needed.
[RequireComponent(typeof(LineRenderer))]
public class MineChain : MonoBehaviour
{
    [Header("Chain Look")]
    public float sagAmount      = 0.35f;  // how much the chain droops between mines
    public float chainWidth     = 0.07f;
    public Color chainColor     = new Color(0.30f, 0.25f, 0.15f, 1f); // dark rusty brown
    public int   segmentsPerLink = 10;    // curve smoothness

    private LineRenderer lr;
    private List<MineBehavior> mines = new List<MineBehavior>();

    public void Init(List<MineBehavior> cluster)
    {
        mines = new List<MineBehavior>(cluster);

        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace      = true;
        lr.startWidth         = chainWidth;
        lr.endWidth           = chainWidth;
        lr.numCapVertices     = 4;
        lr.material           = new Material(Shader.Find("Sprites/Default"));
        lr.startColor         = chainColor;
        lr.endColor           = chainColor;
        lr.sortingLayerName   = "Default";
        lr.sortingOrder       = 1;
    }

    void LateUpdate()
    {
        // Remove destroyed mines from list
        mines.RemoveAll(m => m == null);

        if (mines.Count == 0) { Destroy(gameObject); return; }

        if (mines.Count == 1)
        {
            lr.positionCount = 1;
            lr.SetPosition(0, mines[0].transform.position);
            return;
        }

        // Build curved line: quadratic bezier with sag for each link
        var pts = new List<Vector3>();

        for (int i = 0; i < mines.Count - 1; i++)
        {
            Vector3 a   = mines[i].transform.position;
            Vector3 b   = mines[i + 1].transform.position;
            // Control point: midpoint pushed downward for catenary feel
            Vector3 mid = (a + b) * 0.5f + Vector3.down * sagAmount;

            for (int s = 0; s <= segmentsPerLink; s++)
            {
                float t = s / (float)segmentsPerLink;
                // Quadratic bezier: B(t) = (1-t)²·A + 2(1-t)t·mid + t²·B
                Vector3 p = Mathf.Pow(1f - t, 2f) * a
                          + 2f * (1f - t) * t * mid
                          + t * t * b;
                pts.Add(p);
            }
        }

        lr.positionCount = pts.Count;
        lr.SetPositions(pts.ToArray());
    }
}
