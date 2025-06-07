using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public Transform shopContentParent;
    public TextMeshProUGUI coinText;
    public GameObject itemButtonPrefab;

    private int playerCoins;
    private PlayerProfile currentPlayer;
    private string playerName;
    private List<GameObject> shopItemButtons = new List<GameObject>(); // if needed

    void Start()
    {
        // Get Current player name from memory
        playerName = PlayerPrefs.GetString("PlayerName", "Guest");

        // Load the player updated information from database
        PlayerCSVManager.LoadPlayerToMemory(playerName);

        // Get the latest player balance
        playerCoins = PlayerPrefs.GetInt("PlayerCoins", 0);

        UpdateCoinDisplay();

        GenerateShopItems();
    }

    void UpdateCoinDisplay()
    {
        coinText.text = $"Coins: {playerCoins}";
    }

    void GenerateShopItems()
    {
        var shopItems = ShopCSVManager.LoadShopItems();
        var inventory = InventoryCSVManager.GetPlayerInventory(playerName);

        for (int i = 0; i < shopItems.Count; i++)
        {
            ShopItem item = shopItems[i];

            GameObject itemGO = Instantiate(itemButtonPrefab, shopContentParent);
            shopItemButtons.Add(itemGO);

            TextMeshProUGUI nameText = itemGO.transform.Find("ItemNameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = itemGO.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
            Image avatarImage = itemGO.transform.Find("AvatarImage").GetComponent<Image>();
            Button button = itemGO.GetComponent<Button>();

            nameText.text = item.ItemName;
            priceText.text = $"Cost: {item.Price}";

            Sprite sprite = Resources.Load<Sprite>(item.AvatarPath);
            if (sprite) avatarImage.sprite = sprite;

            int owned = inventory.Count(inv => inv.ItemName == item.ItemName);
            if (owned >= item.LimitPerPlayer)
            {
                priceText.text = "Max Owned";
                button.interactable = false;
            }

            // Avoid closure issue with loop variable
            ShopItem currentItem = item;

            button.onClick.AddListener(() => {
                TryBuy(currentItem, button, priceText);
            });
        }
    }

    void RefreshShopUI()
    {
        // Clear old buttons
        foreach (var go in shopItemButtons)
        {
            Destroy(go);
        }
        shopItemButtons.Clear();

        // Re-fetch player and rebuild shop
        currentPlayer = PlayerProfile.GetFromMemory();
        UpdateCoinDisplay();
        GenerateShopItems();
    }

    void TryBuy(ShopItem item, Button button, TextMeshProUGUI priceText)
    {
        var inventory = InventoryCSVManager.GetPlayerInventory(playerName);
        int owned = inventory.Count(i => i.ItemName == item.ItemName);

        if (owned >= item.LimitPerPlayer)
        {
            Debug.LogWarning("Limit reached.");
            return;
        }

        if (playerCoins < item.Price)
        {
            Debug.LogWarning("Not enough coins.");
            return;
        }

        playerCoins -= item.Price;
        PlayerPrefs.SetInt("PlayerCoins", playerCoins);

        // Read the player state from memory
        currentPlayer = PlayerProfile.GetFromMemory();

        // Save the player state to database
        PlayerCSVManager.UpdatePlayerProfile(currentPlayer);
        UpdateCoinDisplay();

        var newItem = new InventoryItem
        {
            PlayerName = playerName,
            ItemName = item.ItemName,
            Category = item.Category,
            AcquiredDate = DateTime.UtcNow.ToString("o"),
            AcquiredFrom = "ShopPurchase"
        };

        InventoryCSVManager.AddInventoryItem(newItem);

        Debug.Log($"✅ Purchased {item.ItemName}");

        // Lock button visually
        RefreshShopUI();
    }
}
