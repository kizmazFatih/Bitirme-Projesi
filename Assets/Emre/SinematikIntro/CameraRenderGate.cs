using UnityEngine;
using System.Collections.Generic;

public class CameraRenderGate : MonoBehaviour
{
    public Camera mainCamera;
    public Camera[] otherCameras; // PhotoCapture, PuzzleCamera vs (Main hariç hepsi)

    struct State
    {
        public float depth;
        public int cullingMask;
        public CameraClearFlags clearFlags;
        public int targetDisplay;
        public bool enabled;
    }

    Dictionary<Camera, State> cache = new();

    void Awake()
    {
        if (!mainCamera) mainCamera = Camera.main;

        cache.Clear();
        foreach (var cam in otherCameras)
        {
            if (!cam) continue;
            cache[cam] = new State
            {
                depth = cam.depth,
                cullingMask = cam.cullingMask,
                clearFlags = cam.clearFlags,
                targetDisplay = cam.targetDisplay,
                enabled = cam.enabled
            };
        }
    }

    public void EnterIntro()
    {
        if (mainCamera) mainCamera.depth = 10; // en son render etsin

        foreach (var cam in otherCameras)
        {
            if (!cam || cam == mainCamera) continue;

            // Kamera sahnede kalsın ama ekrana çizmesin
            cam.depth = -100;
            cam.clearFlags = CameraClearFlags.Nothing;
            cam.cullingMask = 0;          // Nothing
            cam.targetDisplay = 0;        // Display 1 (istersen 1 yapıp Display2’ye de atabilirsin)

            var al = cam.GetComponent<AudioListener>();
            if (al) al.enabled = false;
        }
    }

    public void ExitIntroRestore()
    {
        foreach (var kv in cache)
        {
            var cam = kv.Key;
            var st = kv.Value;
            if (!cam) continue;

            cam.depth = st.depth;
            cam.cullingMask = st.cullingMask;
            cam.clearFlags = st.clearFlags;
            cam.targetDisplay = st.targetDisplay;
            cam.enabled = st.enabled;
        }
    }
}
