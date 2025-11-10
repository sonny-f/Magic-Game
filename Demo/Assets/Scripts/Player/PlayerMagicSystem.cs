using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class PlayerMagicSystem : MonoBehaviour
{
    [SerializeField] private spell Fireball;
    [SerializeField] private spell WaterProj;
    private spell spellToCast;

    [SerializeField] private bool hasInfiniteMana;

    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float currentMana;
    [SerializeField] private float manaRechargeRate = 2f;
    [SerializeField] private float TimeToWaitForRecharge = 2f;
    private float currentManaRechargeTimer;
    [SerializeField] private float timeBetweenCasts = 0.25f;
    [SerializeField] private Transform castPoint;
    private float currentCastTimer;
    public Image manaBar;
    public GameObject spellSelector;
    public Button fireball;
    public Button waterProj;
    private bool canAttack;

    private bool castingMagic = false;

    private InputSystem_Actions playerControls;

    // Aiming fields
    [Header("Aiming")]
    [Tooltip("Max spread (degrees) when hip-firing")]
    public float HipSpreadAngle = 10f;
    [Tooltip("Max spread (degrees) when aiming")]
    public float AimSpreadAngle = 2f;
    [Tooltip("Target FOV when aiming")]
    public float AimFOV = 50f;
    [Tooltip("How fast FOV interpolates")]
    public float AimFOVSpeed = 8f;

    private bool _isAiming;

    // expose aim state for UI
    public bool IsAiming => _isAiming;

    // expose spell selector state for UI
    public bool IsSpellSelectorOpen => spellSelector != null && spellSelector.activeSelf;

    // Cinemachine support / fallback camera
    private CinemachineVirtualCamera _vcam;
    private Camera _fallbackCamera;
    private float _baseFOV;

    private void Awake()
    {
        playerControls = new InputSystem_Actions();

        currentMana = maxMana;

        canAttack = true;

        spellSelector.SetActive(false);

        spellToCast = Fireball;

        // find Cinemachine vcam (if present) or fallback Camera.main
        // Use the new APIs on newer Unity versions to avoid obsolete warnings.
#if UNITY_2023_2_OR_NEWER
        _vcam = Object.FindFirstObjectByType<CinemachineVirtualCamera>();
#elif UNITY_2023_1_OR_NEWER
        _vcam = Object.FindAnyObjectByType<CinemachineVirtualCamera>();
#else
        // Older Unity: no new API available — fall back to the classic call.
        _vcam = FindObjectOfType<CinemachineVirtualCamera>();
#endif
        if (_vcam != null)
        {
            _baseFOV = _vcam.m_Lens.FieldOfView;
        }
        else
        {
            _fallbackCamera = Camera.main;
            _baseFOV = _fallbackCamera != null ? _fallbackCamera.fieldOfView : 60f;
        }
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        if (hasInfiniteMana)
        {
            currentMana = maxMana;
        }

        // update aim state (right mouse button held)
        _isAiming = Input.GetMouseButton(1);

        // update FOV smoothly
        UpdateAimFOV(_isAiming);

        bool isSpellCastHeldDown = playerControls.Player.SpellCast.ReadValue<float>() > 0.1f;
        bool hasEnoughMana = currentMana - spellToCast.SpellToCast.ManaCost >= 0f;

        if (canAttack && !castingMagic && isSpellCastHeldDown && hasEnoughMana)
        {
            castingMagic = true;
            currentMana -= spellToCast.SpellToCast.ManaCost;
            currentCastTimer = 0f;
            currentManaRechargeTimer = 0f;
            CastSpell();
        }

        if (castingMagic)
        {
            currentCastTimer += Time.deltaTime;

            if (currentCastTimer > timeBetweenCasts)
            {
                castingMagic = false;
            }
        }

        if (currentMana < maxMana && !castingMagic)
        {
            currentManaRechargeTimer += Time.deltaTime;

            if (currentManaRechargeTimer > TimeToWaitForRecharge)
            {
                currentMana += manaRechargeRate * Time.deltaTime;
                if (currentMana > maxMana)
                {
                    currentMana = maxMana;
                }
            }
        }

        manaBar.fillAmount = currentMana / maxMana;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            spellSelector.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void CastSpell()
    {
        // choose spread depending on aim state
        float spread = _isAiming ? AimSpreadAngle : HipSpreadAngle;

        // Use the player's view/camera forward as the cone axis so vertical aim is possible.
        // Prefer the actual rendered Camera (Camera.main) because Cinemachine drives it.
        Camera cam = Camera.main != null ? Camera.main : _fallbackCamera;
        Vector3 coneAxis = cam != null ? cam.transform.forward : castPoint.forward;

        // compute a random direction within cone around coneAxis (allows up/down)
        Vector3 dir = RandomDirectionInCone(coneAxis, spread);

        // instantiate with rotation matching the computed direction
        Quaternion rot = Quaternion.LookRotation(dir);
        var instance = Instantiate(spellToCast, castPoint.position, rot);

        // debug: draw ray so you can see the cast direction in Scene view
        Debug.DrawRay(castPoint.position, dir * 2f, Color.red, 2f);

        canAttack = false;
        StartCoroutine(attackCooldown());
    }

    // Sample a random direction inside a cone defined by forward and maxAngleDegrees.
    private Vector3 RandomDirectionInCone(Vector3 forward, float maxAngleDegrees)
    {
        if (maxAngleDegrees <= 0f) return forward.normalized;

        float maxAngleRad = maxAngleDegrees * Mathf.Deg2Rad;
        // sample cos(theta) uniformly between cos(maxAngle) and 1
        float cosTheta = Mathf.Cos(maxAngleRad);
        float z = Random.Range(cosTheta, 1f);
        float phi = Random.Range(0f, Mathf.PI * 2f);
        float sinTheta = Mathf.Sqrt(1f - z * z);
        Vector3 local = new Vector3(sinTheta * Mathf.Cos(phi), sinTheta * Mathf.Sin(phi), z);

        // rotate local (where forward is Vector3.forward) to align with 'forward'
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, forward.normalized);
        return rot * local;
    }

    private IEnumerator attackCooldown()
    {
        yield return new WaitForSeconds(1f);
        canAttack = true;
    }

    public void fireBall()
    {
        spellToCast = Fireball;
        spellSelector.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void water()
    {
        spellToCast = WaterProj;
        spellSelector.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UpdateAimFOV(bool aiming)
    {
        float target = aiming ? AimFOV : _baseFOV;
        float t = Time.deltaTime * AimFOVSpeed;

        if (_vcam != null)
        {
            float current = _vcam.m_Lens.FieldOfView;
            _vcam.m_Lens.FieldOfView = Mathf.Lerp(current, target, t);
        }
        else if (_fallbackCamera != null)
        {
            _fallbackCamera.fieldOfView = Mathf.Lerp(_fallbackCamera.fieldOfView, target, t);
        }
    }
}
