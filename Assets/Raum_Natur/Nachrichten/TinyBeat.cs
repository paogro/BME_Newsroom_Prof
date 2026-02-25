using UnityEngine;

public class TinyBeat : MonoBehaviour
{
    [Header("Wie stark soll der Effekt sein?")]
    [Range(0f, 0.2f)]
    public float pulseStrength = 0.04f; // 0.04 = 4% kleiner

    [Header("Wie schnell soll der Puls sein?")]
    public float pulseSpeed = 1.5f; // Zyklen pro Sekunde

    [Header("Leicht versetzt starten? (für mehrere Texte)")]
    public bool randomOffset = true;

    private Vector3 originalScale;
    private float timeOffset;

    private void Awake()
    {
        originalScale = transform.localScale;
        timeOffset = randomOffset ? Random.Range(0f, Mathf.PI * 2f) : 0f;
    }

    private void Update()
    {
        // t geht von 0 bis 1 und wieder zurück
        float t = (Mathf.Sin((Time.time + timeOffset) * pulseSpeed) + 1f) * 0.5f; // 0..1

        // Nur kleiner als original, nie größer:
        // 1   -> 0.96 bei pulseStrength=0.04 (also 4% kleiner)
        float factor = 1f - t * pulseStrength;

        transform.localScale = originalScale * factor;
    }

    private void OnDisable()
    {
        // Sicherheit: beim Deaktivieren wieder auf Ursprung
        transform.localScale = originalScale;
    }
}
