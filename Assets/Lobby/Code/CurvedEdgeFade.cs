using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
public class CurvedEdgeFade : MonoBehaviour
{
    [Header("Fade-Einstellungen")]
    [Range(0.0f, 1.0f)]
    public float innerWidth = 0.6f;

    MeshFilter mf;
    Mesh runtimeMesh; // getrennte Runtime-Kopie

    void OnEnable()
    {
        mf = GetComponent<MeshFilter>();
        ApplyFade();
    }

    void LateUpdate()
    {
        ApplyFade();
    }

    void OnValidate()
    {
        ApplyFade();
    }

    void ApplyFade()
    {
        if (!mf) return;

        Mesh mesh;

        // --- Wichtig ---
        // im Editor-mode => sharedMesh verwenden (sonst Mesh-Leak)
        // im Play-mode   => instanzierte mf.mesh verwenden
        if (!Application.isPlaying)
        {
            mesh = mf.sharedMesh;
        }
        else
        {
            // eigene Runtime-Kopie anlegen (nur einmal)
            if (runtimeMesh == null)
            {
                runtimeMesh = Instantiate(mf.sharedMesh);
                mf.mesh = runtimeMesh;
            }
            mesh = runtimeMesh;
        }

        if (!mesh) return;

        var uvs = mesh.uv;
        if (uvs == null || uvs.Length == 0) return;

        var colors = new Color[uvs.Length];

        float innerHalf = innerWidth * 0.5f;

        for (int i = 0; i < uvs.Length; i++)
        {
            float u = uvs[i].x;
            float fromCenter = Mathf.Abs(u - 0.5f);

            float alpha;
            if (fromCenter <= innerHalf)
            {
                alpha = 1f;
            }
            else
            {
                float t = Mathf.InverseLerp(innerHalf, 0.5f, fromCenter);
                alpha = 1f - t;
            }

            colors[i] = new Color(1f, 1f, 1f, alpha);
        }

        // nur Colors setzen – Asset bleibt unangetastet
        mesh.colors = colors;
    }
}
