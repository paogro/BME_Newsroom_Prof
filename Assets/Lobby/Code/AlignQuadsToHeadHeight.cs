using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class AlignQuadsToHeadHeight : MonoBehaviour
{
    [Header("References")]
    public Transform volumeRoot;        // Dein VolumeCamera-Objekt
    public Transform[] quads;           // CurvedQuad_1 ... CurvedQuad_4

    [Header("Behavior")]
    public bool continuous = false;     // true = Y-Höhe live anpassen
    public float defaultEyeY = 1.65f;   // Fallback wenn kein HMD aktiv
    public float heightOffset = -0.1f;  // NEGATIV = leicht unter Augenhöhe

    void Start()
    {
        if (volumeRoot == null) volumeRoot = transform;
        ApplyHeightOnce();
    }

    void LateUpdate()
    {
        if (continuous) ApplyHeightOnce();
    }

    void ApplyHeightOnce()
    {
            // 1⃣ Headset-Position aus dem Input System
            var hmd = InputSystem.GetDevice<XRHMD>();
            float eyeYLocal = defaultEyeY;

            if (hmd != null && hmd.devicePosition != null)
            {
                Vector3 headWorld = hmd.devicePosition.ReadValue();
                eyeYLocal = volumeRoot.InverseTransformPoint(headWorld).y;
            }

            // 2⃣ Quads auf diese Y-Höhe + Offset setzen
            foreach (var q in quads)
            {
                if (q == null) continue;

                Vector3 localPos = volumeRoot.InverseTransformPoint(q.position);
                localPos.y = eyeYLocal + heightOffset;
                q.position = volumeRoot.TransformPoint(localPos);
            }
    }
}
