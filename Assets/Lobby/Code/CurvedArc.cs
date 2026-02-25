
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CurvedArc : MonoBehaviour
{
    public Transform center;          // z.B. deine Kamera
    public float radius = 2.0f;       // Abstand vom Mittelpunkt
    public float height = 1.2f;       // Höhe des Panels
    [Range(0.01f, 360f)] public float arcAngleDeg = 120f; // Bogenlänge
    public float startAngleDeg = 0f;  // Startwinkel um Y
    public int segmentsX = 96;        // Auflösung entlang des Bogens
    public int segmentsY = 12;        // Auflösung vertikal
    public bool faceInwards = true;   // Vorderseite zeigt nach innen

    void OnEnable()  => Build();
    void OnValidate()=> Build();

    void Build()
    {
        if (!center) return;
        transform.position = center.position;
        transform.rotation = Quaternion.identity;

        var mf = GetComponent<MeshFilter>();
        mf.sharedMesh = Generate(radius, height, startAngleDeg, arcAngleDeg, segmentsX, segmentsY, faceInwards);
    }

    Mesh Generate(float r, float h, float startDeg, float arcDeg, int sx, int sy, bool inward)
    {
        var m = new Mesh { name = "CurvedArc" };
        int vx = sx + 1, vy = sy + 1;
        var vtx = new Vector3[vx * vy];
        var uv  = new Vector2[vx * vy];
        var tri = new int[sx * sy * 6];

        float a0 = startDeg * Mathf.Deg2Rad;
        float a1 = (startDeg + arcDeg) * Mathf.Deg2Rad;

        for (int y = 0; y < vy; y++)
        {
            float v = y / (float)sy;
            float yPos = Mathf.Lerp(-h * 0.5f, h * 0.5f, v);

            for (int x = 0; x < vx; x++)
            {
                float u = x / (float)sx;
                float a = Mathf.Lerp(a0, a1, u);
                float px = Mathf.Sin(a) * r;
                float pz = Mathf.Cos(a) * r;
                int i = y * vx + x;
                vtx[i] = new Vector3(px, yPos, pz);
                uv[i]  = new Vector2(u, v);
            }
        }

        int t = 0;
        for (int y = 0; y < sy; y++)
        for (int x = 0; x < sx; x++)
        {
            int i = y * vx + x, iR = i + 1, iU = i + vx, iUR = iU + 1;
            if (inward)
            {
                tri[t++] = i;   tri[t++] = iUR; tri[t++] = iR;
                tri[t++] = i;   tri[t++] = iU;  tri[t++] = iUR;
            }
            else
            {
                tri[t++] = i;   tri[t++] = iR;  tri[t++] = iUR;
                tri[t++] = i;   tri[t++] = iUR; tri[t++] = iU;
            }
        }

        m.vertices = vtx;
        m.uv = uv;
        m.triangles = tri;
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
