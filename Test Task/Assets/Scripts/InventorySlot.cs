using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image itemIcon; // Image на дочернем объекте с индексом 0
    [SerializeField] TMP_Text amountText; // Текст количества
    [SerializeField] Button slotButton; // Основная кнопка слота
    [SerializeField] public Button clearButton; // Кнопка очистки (дочерний объект с индексом 2)
    [SerializeField] GameObject clearButtonObject; // Объект кнопки очистки
    [SerializeField] public Button selectButton; // Кнопка выбора оружия (дочерний объект с индексом 3)
    [SerializeField] GameObject selectButtonObject; // Объект кнопки выбора

    public string ItemID { get; private set; }
    public bool IsOccupied => !string.IsNullOrEmpty(ItemID);

    void Awake()
    {
        // Находим itemIcon
        if (itemIcon == null && transform.childCount > 0)
        {
            itemIcon = transform.GetChild(0).GetComponent<Image>();
            if (itemIcon == null)
                Debug.LogWarning($"No Image component found on child 0 of {name}");
        }

        // Находим amountText
        if (amountText == null)
        {
            amountText = GetComponentInChildren<TMP_Text>();
            if (amountText == null)
                Debug.LogWarning($"No TMP_Text component found in {name}");
        }

        // Находим clearButton
        if (clearButtonObject == null && transform.childCount > 2)
            clearButtonObject = transform.GetChild(2).gameObject;

        if (clearButton == null && clearButtonObject != null)
            clearButton = clearButtonObject.GetComponent<Button>();

        if (clearButtonObject != null)
            clearButtonObject.SetActive(false);
        else
            Debug.LogWarning($"Clear button object not found for {name}");

        // Находим selectButton
        if (selectButtonObject == null && transform.childCount > 3)
            selectButtonObject = transform.GetChild(3).gameObject;

        if (selectButton == null && selectButtonObject != null)
            selectButton = selectButtonObject.GetComponent<Button>();

        if (selectButtonObject != null)
            selectButtonObject.SetActive(false);
        else
            Debug.LogWarning($"Select button object not found for {name}");

        // Назначаем обработчик для slotButton
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(ToggleButtons);
        }
        else
        {
            Debug.LogWarning($"Slot button not assigned for {name}");
        }
    }

    public void SetItem(Sprite sprite, string itemId, int amount = 1)
    {
        if (itemIcon == null)
        {
            itemIcon = transform.childCount > 0 ? transform.GetChild(0).GetComponent<Image>() : null;
            if (itemIcon == null)
                Debug.LogWarning($"Failed to find itemIcon in {name}");
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = sprite;
            itemIcon.enabled = sprite != null;
        }

        ItemID = itemId;
        UpdateAmount(amount);
        Debug.Log($"Set item {itemId} in {name}, amount: {amount}");
    }

    public void UpdateAmount(int amount)
    {
        if (amountText != null)
        {
            amountText.text = amount > 1 ? amount.ToString() : "";
            Debug.Log($"Updated amountText for {ItemID} in {name}: {amountText.text}");
        }
    }

    public void ClearSlot()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (amountText != null)
            amountText.text = "";

        ItemID = "";

        if (clearButtonObject != null)
            clearButtonObject.SetActive(false);

        if (selectButtonObject != null)
            selectButtonObject.SetActive(false);

        Debug.Log($"Cleared slot {name}");
    }

    void ToggleButtons()
    {
        if (!IsOccupied) return;

        bool showButtons = clearButtonObject != null && !clearButtonObject.activeSelf;
        if (clearButtonObject != null)
        {
            clearButtonObject.SetActive(showButtons);
        }

        // Показываем selectButton только для оружия, если оно не активно
        if (selectButtonObject != null && (ItemID == "gun_makarov" || ItemID == "gun_ak"))
        {
            bool isActiveWeapon = DataSaver.Instance != null && DataSaver.Instance.GetActiveWeaponId() == ItemID;
            selectButtonObject.SetActive(showButtons && !isActiveWeapon);
        }

        if (showButtons)
        {
            foreach (var slot in FindObjectsOfType<InventorySlot>())
            {
                if (slot != this && slot != null)
                {
                    if (slot.clearButtonObject != null)
                        slot.clearButtonObject.SetActive(false);
                    if (slot.selectButtonObject != null)
                        slot.selectButtonObject.SetActive(false);
                }
            }
            Debug.Log($"Toggled buttons for slot {name}: clearButton={showButtons}, selectButton={(selectButtonObject != null ? selectButtonObject.activeSelf : false)}");
        }
    }
}