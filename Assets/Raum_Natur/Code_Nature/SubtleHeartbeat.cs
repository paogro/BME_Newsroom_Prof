using UnityEngine;

public class SubtleHeartbeat : MonoBehaviour
{
    [Header("Scale Einstellungen")]
    [Tooltip("Normale Größe (1 = aktuelle Größe beibehalten).")]
    public float baseScale = 1f;

    [Tooltip("Wie stark der Puls ist (0.05 = +5%).")]
    public float pulseStrength = 0.07f;

    [Tooltip("Geschwindigkeit des Pulses.")]
    public float pulseSpeed = 2f;

    private Vector3 _initialScale;

    private void Start()
    {
        // ursprüngliche Größe merken
        _initialScale = transform.localScale;
    }

    private void Update()
    {
        // Wert zwischen 0 und 1, der wie ein Herzschlag hoch/runter geht
        float t = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) * 0.5f;

        // skaliere leicht um die Basisgröße herum
        float scaleFactor = baseScale + t * pulseStrength;

        transform.localScale = _initialScale * scaleFactor;
    }
}
