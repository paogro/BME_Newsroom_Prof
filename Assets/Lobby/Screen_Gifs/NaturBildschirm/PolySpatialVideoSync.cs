using UnityEngine;
using UnityEngine.Video;
using Unity.PolySpatial;  // wichtig!

[RequireComponent(typeof(VideoPlayer))]
public class PolySpatialVideoSync : MonoBehaviour
{
    public RenderTexture targetTexture;

    VideoPlayer _videoPlayer;

    void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();

        if (targetTexture != null)
        {
            _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            _videoPlayer.targetTexture = targetTexture;
        }
    }

    void LateUpdate()
    {
        if (targetTexture != null)
        {
            PolySpatialObjectUtils.MarkDirty(targetTexture);
        }
    }
}
