using UnityEngine;

public class UIToWorldFollower : MonoBehaviour
{
    public RectTransform uiTarget;  // Takip edilecek buton
    public Camera uiCamera;         // Canvas'ta kullandığın kamera
    public float worldZ = 9.8f;     // UI'nın biraz arkasında

    void LateUpdate()
    {
        if (!uiTarget || !uiCamera) return;

        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, uiTarget.position);
        screenPos.z = worldZ;
        transform.position = uiCamera.ScreenToWorldPoint(screenPos);
    }
}