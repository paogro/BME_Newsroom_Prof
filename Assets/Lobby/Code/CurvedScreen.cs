using UnityEngine;

[ExecuteAlways] // <<< wichtig: läuft auch im Editor
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CurvedFromQuad : MonoBehaviour
{
    public MeshRenderer sourceQuad;

    public float radius = 2.5f;
    public int segmentsX = 70;
    public int segmentsY = 10;
    public bool faceInwards = true;
    public bool disableSourceQuad = true;
    public bool copyTransformFromSource = true;

    void OnEnable() { TryBuild(); }
    void OnValidate() { TryBuild(); } // bei Parameter-Änderung neu bauen

    void TryBuild()
    {
        if (!sourceQuad) return;

        if (copyTransformFromSource)
        {
            transform.position = sourceQuad.transform.position;
            transform.rotation = sourceQuad.transform.rotation;
        }

        var lossy = sourceQuad.transform.lossyScale;
        float width = Mathf.Max(0.001f, lossy.x);
        float height = Mathf.Max(0.001f, lossy.y);
        float theta = Mathf.Clamp(width / Mathf.Max(0.001f, radius), 0.01f, Mathf.PI * 1.99f);
        float angleDeg = theta * Mathf.Rad2Deg;

        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        mf.sharedMesh = GenerateCurvedMesh(radius, angleDeg, height, segmentsX, segmentsY, faceInwards);

        // Material übernehmen, falls vorhanden
        if (sourceQuad.sharedMaterials != null && sourceQuad.sharedMaterials.Length > 0)
            mr.sharedMaterials = sourceQuad.sharedMaterials;

        if (disableSourceQuad) sourceQuad.gameObject.SetActive(false);
    }

    Mesh GenerateCurvedMesh(float radius, float angleDegrees, float height, int segX, int segY, bool inward)
    {
        var mesh = new Mesh { name = "CurvedFromQuadMesh" };
        int vx = segX + 1, vy = segY + 1;
        var vertices = new Vector3[vx * vy];
        var uvs = new Vector2[vx * vy];
        var tris = new int[segX * segY * 6];

        float startA = -angleDegrees * 0.5f * Mathf.Deg2Rad;
        float endA = angleDegrees * 0.5f * Mathf.Deg2Rad;

        for (int y = 0; y < vy; y++)
        {
            float v = (float)y / segY;
            float yPos = Mathf.Lerp(-height * 0.5f, height * 0.5f, v);

            for (int x = 0; x < vx; x++)
            {
                float u = (float)x / segX;
                float a = Mathf.Lerp(startA, endA, u);
                float px = Mathf.Sin(a) * radius;
                float pz = Mathf.Cos(a) * radius;

                int i = y * vx + x;
                vertices[i] = new Vector3(px, yPos, pz);
                uvs[i] = new Vector2(u, v);
            }
        }

        int t = 0;
        for (int y = 0; y < segY; y++)
            for (int x = 0; x < segX; x++)
            {
                int i = y * vx + x, iR = i + 1, iU = i + vx, iUR = iU + 1;
                if (inward)
                {
                    tris[t++] = i;   tris[t++] = iUR; tris[t++] = iR;
                    tris[t++] = i;   tris[t++] = iU;  tris[t++] = iUR;
                }
                else
                {
                    tris[t++] = i;   tris[t++] = iR;  tris[t++] = iUR;
                    tris[t++] = i;   tris[t++] = iUR; tris[t++] = iU;
                }
            }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
