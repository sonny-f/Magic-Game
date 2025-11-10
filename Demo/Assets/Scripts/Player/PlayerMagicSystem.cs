using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMagicSystem : MonoBehaviour
{
    [SerializeField] private spell Fireball;
    [SerializeField] private spell WaterProj;
    private spell spellToCast;

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

    private void Awake()
    {
        playerControls = new InputSystem_Actions();

        currentMana = maxMana;

        canAttack = true;

        spellSelector.SetActive(false);

        spellToCast = Fireball;
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


        bool isSpellCastHeldDown = playerControls.Player.SpellCast.ReadValue<float>() > 0.1f;
        bool hasEnoughMana = currentMana - spellToCast.SpellToCast.ManaCost >= 0f;

        if(!castingMagic && isSpellCastHeldDown && hasEnoughMana)
        {
            castingMagic = true;
            currentMana -= spellToCast.SpellToCast.ManaCost;
            currentCastTimer = 0f;
            currentManaRechargeTimer = 0f;
            CastSpell();
            StartCoroutine(attackCooldown());
            canAttack = false;
        }

        if(castingMagic)
        {
            currentCastTimer += Time.deltaTime;

            if(currentCastTimer > timeBetweenCasts)
            {
                castingMagic = false;
            }
        }

        if(currentMana < maxMana && !castingMagic)
        {
            currentManaRechargeTimer += Time.deltaTime;

            if(currentManaRechargeTimer > TimeToWaitForRecharge)
            {
                currentMana += manaRechargeRate * Time.deltaTime;
                if (currentMana > maxMana)
                {
                    currentMana = maxMana;
                }
            }
        }

        manaBar.fillAmount = currentMana/100;

        if(Input.GetKeyDown(KeyCode.Q))
        {
            spellSelector.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void CastSpell()
    {
        var instance = Instantiate(spellToCast, castPoint.position, castPoint.rotation);
        // ensure the spell's forward matches the cast point (player) forward
        instance.transform.forward = castPoint.forward;

        // debug: draw ray so you can see the cast direction in Scene view
        Debug.DrawRay(castPoint.position, castPoint.forward * 2f, Color.red, 2f);
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
}
