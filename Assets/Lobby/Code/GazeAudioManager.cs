using System.Collections.Generic;
using UnityEngine;

public class GazeAudioManager : MonoBehaviour
{
    public static GazeAudioManager Instance { get; private set; }

    private readonly List<GazeAudioTrigger> _triggers = new List<GazeAudioTrigger>();
    private GazeAudioTrigger _focused;   // der aktuell spielende

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(GazeAudioTrigger t)
    {
        if (!_triggers.Contains(t)) _triggers.Add(t);
    }

    public void Unregister(GazeAudioTrigger t)
    {
        if (_triggers.Contains(t)) _triggers.Remove(t);
        if (_focused == t) _focused = null;
    }

    /// <summary>
    /// Wird vom Trigger aufgerufen, wenn sein Blick-Check positiv ist.
    /// </summary>
    public void RequestFocus(GazeAudioTrigger requester)
    {
        if (_focused == requester) return;

        // neuen Fokus setzen
        _focused = requester;

        // alle anderen stummschalten
        for (int i = 0; i < _triggers.Count; i++)
        {
            var tr = _triggers[i];
            if (tr == null) continue;
            if (tr == requester) tr.PlayFocused();
            else tr.Mute();
        }
    }

    /// <summary>
    /// Trigger meldet: nicht mehr angeschaut.
    /// Falls er der aktuelle Fokus war, wird er stumm; kein automatischer Wechsel.
    /// </summary>
    public void ReleaseFocus(GazeAudioTrigger requester)
    {
        if (_focused == requester) _focused = null;
        requester.Mute();
    }
}
