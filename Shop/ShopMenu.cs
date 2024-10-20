using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Mirror;

public class ShopMenu : NetworkBehaviour
{
    [Header("Shop UI")]
    [SerializeField] private PlayerManager currentPlayerManager;
    public TMP_Text shopBalance;
    public TMP_Text currentPistolDamageText;
    public TMP_Text currentKnifeDamageText;
    public TMP_Text currentMaxHealthText;
    public TMP_Text[] currentCostText;

    [Header("Weapon Upgrades")]
    public int damageIncrease = 10;
    [SerializeField] private int weaponUpgradeLevel = 0;
    public int initialUpgradeCost = 1;
    public int pistolInflationRate = 4;
    public int knifeInflationRate = 5;

    [Header("Health & Max Health Upgrades")]
    public int healthCost = 2;
    public int healthAmount = 20;
    public int initialMaxHealthCost = 1;
    public int maxHealthInflationRate = 5;

    // Weapon Interface & References
    public interface IWeapon
    {
        int Damage { get; set; }
    }

    void Update()
    {
        if(currentPlayerManager != null)
        {
            shopBalance.text = currentPlayerManager.currencyText.text;
        }
    }

    #region Shop Logic
    public void SetCurrentPlayer(NetworkIdentity playerIdentity)
    {
        currentPlayerManager = playerIdentity.GetComponentInParent<PlayerManager>();
        if(currentPlayerManager != null)
        {
            shopBalance.text = currentPlayerManager.currencyText.text;
        }
    }

    public void UpgradeWeapon(IWeapon weapon, int inflation)
    {
        if (currentPlayerManager.currencyBalance >= initialUpgradeCost)
        {
            weapon.Damage += damageIncrease; // Damage Upgrade
            currentPlayerManager.CmdDeductCurrency(initialUpgradeCost); // Deduct Currency
            weaponUpgradeLevel++; // Upgrade Level
            initialUpgradeCost += inflation; // Cost Inflation!!!
        }
        else
        {
            Debug.Log("Insufficient Funds! Cannot Afford Upgrade.");
        }
    }

    public void PurchaseHealth()
    {
        if (currentPlayerManager.currencyBalance >= healthCost)
        {
            if(currentPlayerManager.maxHealth != currentPlayerManager.currentHealth)
            {
                currentPlayerManager.CmdPurchaseHealth(healthAmount);
                currentPlayerManager.CmdDeductCurrency(healthCost);
            }
            else
            {
                Debug.Log("You are Full on Health.");
            }
        }
        else
        {
            Debug.Log("Insufficient Funds! Cannot Afford Health.");
        }
    }

    public void PurchaseMaxHealthIncrease()
    {
        if (currentPlayerManager.currencyBalance >= initialMaxHealthCost)
        {
            int maxHealthAmount = currentPlayerManager.healthStatPoints * currentPlayerManager.shopPurchaseMaxHealthModifier;
            currentPlayerManager.CmdSetMaxHealth(maxHealthAmount);
            currentPlayerManager.CmdDeductCurrency(initialMaxHealthCost);
            initialMaxHealthCost += maxHealthInflationRate;
        }
        else
        {
            Debug.Log("Insufficient Funds! Cannot Afford Health Upgrade.");
        }
    }

    #endregion

    #region Weapon Upgrades

    public void UpgradePistol()
    {
        Gun gunRef = currentPlayerManager.gameObject.GetComponentInChildren<Gun>(true);
        UpgradeWeapon(gunRef, pistolInflationRate);
    }

    public void UpgradeKnife()
    {
        knife knifeRef = currentPlayerManager.gameObject.GetComponentInChildren<knife>(true);
        UpgradeWeapon(knifeRef, knifeInflationRate);
    }

    #endregion
}
