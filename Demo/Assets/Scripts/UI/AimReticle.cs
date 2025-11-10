using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AimReticle : MonoBehaviour
{
    [Tooltip("Image used as the circular reticle (centered). Should be a hollow circle sprite.")]
    public Image reticleImage;

    [Tooltip("Reference to your PlayerMagicSystem component")]
    public PlayerMagicSystem playerMagic;

    [Tooltip("How fast the reticle scales when changing aim state")]
    public float scaleLerpSpeed = 12f;

    [Tooltip("How fast the reticle fades when the spell selector opens")]
    public float fadeSpeed = 8f;

    // optional override camera (if null, use Camera.main)
    public Camera overrideCamera;

    private RectTransform _rt;
    private float _targetRadiusPixels;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        if (reticleImage == null)
        {
            Debug.LogError("AimReticle: assign reticleImage in the Inspector");
            enabled = false;
            return;
        }

        if (playerMagic == null)
        {
            Debug.LogError("AimReticle: assign playerMagic in the Inspector");
            enabled = false;
            return;
        }

        // ensure the image doesn't block raycasts
        reticleImage.raycastTarget = false;
    }

    void LateUpdate()
    {
        Camera cam = overrideCamera != null ? overrideCamera : Camera.main;
        if (cam == null) return;

        // pick the spread angle depending on aim state
        float angleDeg = playerMagic.IsAiming ? playerMagic.AimSpreadAngle : playerMagic.HipSpreadAngle;
        // convert to radians
        float theta = angleDeg * Mathf.Deg2Rad;

        // compute pixel radius: (tan(theta) / tan(camFov/2)) * (Screen.height/2)
        float camHalfFovRad = (cam.fieldOfView * Mathf.Deg2Rad) * 0.5f;
        float pixelRadius = 0f;
        if (camHalfFovRad > 0f)
        {
            pixelRadius = (Mathf.Tan(theta) / Mathf.Tan(camHalfFovRad)) * (Screen.height * 0.5f);
        }

        // target size in pixels (diameter)
        _targetRadiusPixels = pixelRadius;

        // smooth scale
        float currentSize = _rt.sizeDelta.x * 0.5f;
        float newRadius = Mathf.Lerp(currentSize, _targetRadiusPixels, Time.deltaTime * scaleLerpSpeed);

        float diameter = Mathf.Max(2f, newRadius * 2f);
        _rt.sizeDelta = new Vector2(diameter, diameter);

        // Fade reticle when spell selector UI is open
        float targetAlpha = playerMagic.IsSpellSelectorOpen ? 0f : 1f;
        Color c = reticleImage.color;
        float newA = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
        reticleImage.color = new Color(c.r, c.g, c.b, newA);
    }
}