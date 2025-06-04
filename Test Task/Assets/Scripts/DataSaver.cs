using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DataSaver : MonoBehaviour
{
    public static DataSaver Instance;

    [System.Serializable]
    public class InventoryItem
    {
        public string itemId;
        public int amount;
    }

    [System.Serializable]
    public class GameData
    {
        public List<InventoryItem> inventoryItems = new();
        public string activeWeaponId;
    }

    [Header("Inventory")]
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] InventorySlot[] inventorySlots;
    [SerializeField] private Button inventoryToggleButton;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound; 
    private AudioSource audioSource; 

    [Header("Debug")]
    [SerializeField] GameData gameData = new();

    private string savePath;
    private GameObject player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "save.txt");
            Debug.Log($"Save path: {savePath}");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Получаем или добавляем AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("Added AudioSource to DataSaver.");
        }
    }

    void Start()
    {
        player = GameObject.Find("Player");
        if (player == null)
            Debug.LogError("Player object not found!");

        // Настраиваем кнопку инвентаря
        if (inventoryToggleButton != null)
        {
            inventoryToggleButton.onClick.AddListener(ToggleInventory);
            Debug.Log("Inventory toggle button assigned.");
        }
        else
        {
            Debug.LogError("Inventory toggle button not assigned in DataSaver!");
        }

        LoadData();
        InitInventory();
        UpdatePlayerWeapons();
        RefreshInventoryUI();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleInventory();
        }
    }
    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        player = GameObject.Find("Player");
        if (player == null)
            Debug.LogError("Player object not found in new scene!");

        InitInventory();
        UpdatePlayerWeapons();
        RefreshInventoryUI();
    }

    void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("Inventory panel not assigned!");
            return;
        }

        bool newState = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(newState);

        if (newState)
        {
            RefreshInventoryUI();
        }

        Debug.Log($"Inventory panel toggled to: {newState}");
    }

    void InitInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryPanel not assigned!");
            return;
        }

        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            inventorySlots = inventoryPanel.GetComponentsInChildren<InventorySlot>(true);
            Debug.Log($"Found {inventorySlots.Length} inventory slots.");

            foreach (var slot in inventorySlots)
            {
                if (slot.clearButton != null)
                {
                    slot.clearButton.onClick.RemoveAllListeners();
                    slot.clearButton.onClick.AddListener(() => ClearSlot(slot));
                }

                if (slot.selectButton != null)
                {
                    slot.selectButton.onClick.RemoveAllListeners();
                    slot.selectButton.onClick.AddListener(() => SelectWeapon(slot));
                }
            }
        }

        inventoryPanel.SetActive(false);
    }

    void ClearSlot(InventorySlot slot)
    {
        if (slot == null || !slot.IsOccupied)
        {
            Debug.Log($"Slot {slot?.name} is null or not occupied, skipping clearSlot.");
            return;
        }

        var item = gameData.inventoryItems.Find(x => x.itemId == slot.ItemID);
        if (item != null)
        {
            gameData.inventoryItems.Remove(item);
            if (gameData.activeWeaponId == item.itemId)
            {
                gameData.activeWeaponId = gameData.inventoryItems.Exists(x => x.itemId == "gun_makarov" || x.itemId == "gun_ak")
                    ? gameData.inventoryItems.Find(x => x.itemId == "gun_makarov" || x.itemId == "gun_ak").itemId
                    : "";
            }
            Debug.Log($"Removed item {item.itemId} from inventoryItems.");
        }

        slot.ClearSlot();
        SaveData();
        UpdatePlayerWeapons();
        RefreshInventoryUI();
        Debug.Log($"Cleared slot {slot.name} and refreshed inventory.");
    }

    void SelectWeapon(InventorySlot slot)
    {
        if (slot == null || !slot.IsOccupied || (slot.ItemID != "gun_makarov" && slot.ItemID != "gun_ak"))
        {
            Debug.Log($"Cannot select weapon: Slot {slot?.name} is invalid or not a weapon.");
            return;
        }

        gameData.activeWeaponId = slot.ItemID;
        SaveData();
        UpdatePlayerWeapons();
        RefreshInventoryUI();
        Debug.Log($"Selected weapon: {slot.ItemID}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        string id = null;
        if (other.CompareTag("bullet_obj"))
        {
            id = "bullet";
        }
        else if (other.CompareTag("gun_makarov_obj"))
        {
            id = "gun_makarov";
        }
        else if (other.CompareTag("gun_ak_obj"))
        {
            id = "gun_ak";
        }

        if (id == null) return;

        // Воспроизводим звук подбора
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
            Debug.Log($"Played pickup sound for {id}");
        }
        else
        {
            Debug.LogWarning($"Pickup sound not assigned or AudioSource missing for {id}");
        }

        var item = gameData.inventoryItems.Find(x => x.itemId == id);
        if (item == null)
        {
            item = new InventoryItem { itemId = id, amount = 0 };
            gameData.inventoryItems.Add(item);
        }

        item.amount += id == "bullet" ? 39 : 1;
        Debug.Log($"Picked up {id}, new amount: {item.amount}");

        if (string.IsNullOrEmpty(gameData.activeWeaponId) && (id == "gun_makarov" || id == "gun_ak"))
        {
            gameData.activeWeaponId = id;
        }

        RefreshInventoryUI();
        UpdatePlayerWeapons();
        SaveData();
        Destroy(other.gameObject);
    }

    public bool SpendBullets(int amount)
    {
        var bulletItem = gameData.inventoryItems.Find(x => x.itemId == "bullet");
        if (bulletItem == null)
        {
            Debug.LogWarning("Bullet item not found in inventory!");
            return false;
        }

        if (bulletItem.amount < amount)
        {
            Debug.LogWarning($"Not enough bullets! Required: {amount}, Available: {bulletItem.amount}");
            return false;
        }

        bulletItem.amount -= amount;
        Debug.Log($"Spent {amount} bullet(s). Bullets left: {bulletItem.amount}");

        // Удаляем патроны из инвентаря, если их стало 0
        if (bulletItem.amount == 0)
        {
            gameData.inventoryItems.Remove(bulletItem);
            Debug.Log("Removed bullet from inventory as amount reached 0.");
        }

        SaveData();
        RefreshInventoryUI();
        return true;
    }

    void RefreshInventoryUI()
    {
        foreach (var slot in inventorySlots)
        {
            if (slot != null)
                slot.ClearSlot();
        }

        for (int i = 0; i < Mathf.Min(gameData.inventoryItems.Count, inventorySlots.Length); i++)
        {
            var item = gameData.inventoryItems[i];
            var sprite = Resources.Load<Sprite>($"Items/{item.itemId}");
            if (sprite != null && inventorySlots[i] != null)
            {
                inventorySlots[i].SetItem(sprite, item.itemId, item.amount);
                Debug.Log($"Updated slot {i} with item {item.itemId}, amount: {item.amount}");
            }
        }
    }

    void UpdatePlayerWeapons()
    {
        if (player == null)
        {
            Debug.LogWarning("Player object is null, cannot update weapons.");
            return;
        }

        bool hasMakarov = gameData.inventoryItems.Exists(x => x.itemId == "gun_makarov");
        bool hasAk = gameData.inventoryItems.Exists(x => x.itemId == "gun_ak");

        if (player.transform.childCount > 9)
        {
            var makarovObj = player.transform.GetChild(9).gameObject;
            makarovObj.SetActive(hasMakarov && gameData.activeWeaponId == "gun_makarov");
            Debug.Log($"Makarov (child 9) active: {makarovObj.activeSelf}");
        }
        else
        {
            Debug.LogWarning("Player does not have enough child objects for Makarov (index 9)");
        }

        if (player.transform.childCount > 10)
        {
            var akObj = player.transform.GetChild(10).gameObject;
            akObj.SetActive(hasAk && gameData.activeWeaponId == "gun_ak");
            Debug.Log($"AK (child 10) active: {akObj.activeSelf}");
        }
        else
        {
            Debug.LogWarning("Player does not have enough child objects for AK (index 10)");
        }
    }

    public string GetActiveWeaponId()
    {
        return gameData.activeWeaponId;
    }

    public void SaveData()
    {
        try
        {
            string json = JsonUtility.ToJson(gameData, true);
            File.WriteAllText(savePath, json);
            Debug.Log("Game saved");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                JsonUtility.FromJsonOverwrite(json, gameData);
                Debug.Log("Game loaded");
                UpdatePlayerWeapons();
                RefreshInventoryUI();
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Clear Save")]
    void ClearSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            gameData = new GameData();
            UpdatePlayerWeapons();
            RefreshInventoryUI();
            Debug.Log("Save cleared");
        }
    }
#endif

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}