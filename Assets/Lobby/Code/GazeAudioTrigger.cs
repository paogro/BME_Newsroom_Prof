using UnityEngine;
using UnityEngine.XR;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class GazeAudioTrigger : MonoBehaviour
{
    [Header("Raycast")]
    public LayerMask hitLayers;
    public float maxDistance = 10f;

    [Header("Blick-Stabilisierung")]
    public float dwellOnMs = 120f;   // wie lange hinschauen, bevor Fokus übernommen wird
    public float dwellOffMs = 150f;  // wie lange wegschauen, bevor Fokus abgegeben wird

    [Header("Audio Fade")]
    public float fadeInTime = 0.12f;
    public float fadeOutTime = 0.20f;
    public float targetVolume = 1f;

    private AudioSource _audio;
    private bool _isGazeOnThis;   // rohes Raycast-Ergebnis (ohne dwell)
    private float _timerMs;
    private Coroutine _fadeCo;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.volume = 0f;
    }

    void OnEnable()  { if (GazeAudioManager.Instance) GazeAudioManager.Instance.Register(this); }
    void OnDisable() { if (GazeAudioManager.Instance) GazeAudioManager.Instance.Unregister(this); }

    void Update()
{
    var dev = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
    if (!dev.isValid) return;

    if (dev.TryGetFeatureValue(CommonUsages.centerEyePosition, out Vector3 eyePos) &&
        dev.TryGetFeatureValue(CommonUsages.centerEyeRotation, out Quaternion eyeRot))
    {
        Vector3 dir = eyeRot * Vector3.forward;

        // EIN Raycast, EIN Hit-Name
        if (Physics.Raycast(eyePos, dir, out RaycastHit hitInfo, maxDistance, hitLayers, QueryTriggerInteraction.Ignore))
        {
            bool hitThis = hitInfo.collider != null && hitInfo.collider.gameObject == gameObject;

            // optionales Logging zum Testen:
            // Debug.Log("Hit: " + hitInfo.collider.gameObject.name);

            // Dwell/Hysterese-Logik
            if (hitThis != _isGazeOnThis)
            {
                _isGazeOnThis = hitThis;
                _timerMs = 0f;
            }
            else
            {
                _timerMs += Time.deltaTime * 1000f;
                if (_isGazeOnThis && _timerMs >= dwellOnMs)
                    GazeAudioManager.Instance?.RequestFocus(this);
                else if (!_isGazeOnThis && _timerMs >= dwellOffMs)
                    GazeAudioManager.Instance?.ReleaseFocus(this);
            }
        }
        else
        {
            // Kein Treffer -> Dwell-Off zählen
            if (_isGazeOnThis)
            {
                _timerMs += Time.deltaTime * 1000f;
                if (_timerMs >= dwellOffMs)
                {
                    _isGazeOnThis = false;
                    _timerMs = 0f;
                    GazeAudioManager.Instance?.ReleaseFocus(this);
                }
            }
            // optional: Debug.Log("No hit");
        }
    }
}

#if UNITY_EDITOR
void OnDrawGizmosSelected()
{
    var dev = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
    if (dev.isValid &&
        dev.TryGetFeatureValue(CommonUsages.centerEyePosition, out Vector3 eyePos) &&
        dev.TryGetFeatureValue(CommonUsages.centerEyeRotation, out Quaternion eyeRot))
    {
        Vector3 dir = eyeRot * Vector3.forward;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePos, dir * maxDistance);
    }
}
#endif


    // Vom Manager aufgerufen
    public void PlayFocused()
    {
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeTo(targetVolume, fadeInTime, playIfSilent: true));
    }

    // Vom Manager aufgerufen
    public void Mute()
    {
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeTo(0f, fadeOutTime, stopAtZero: true));
    }

    private IEnumerator FadeTo(float target, float time, bool playIfSilent = false, bool stopAtZero = false)
    {
        if (playIfSilent && !_audio.isPlaying) _audio.Play();
        float start = _audio.volume;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            _audio.volume = Mathf.Lerp(start, target, t / time);
            yield return null;
        }
        _audio.volume = target;
        if (stopAtZero && target <= 0.0001f) _audio.Stop();
    }
}
