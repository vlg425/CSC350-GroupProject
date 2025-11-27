using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

public class InventorySlot : MonoBehaviour, 
    IPointerClickHandler, 
    IPointerEnterHandler, 
    IPointerExitHandler,
    IDropHandler 
{
    [SerializeField] private TMP_Text InventorySlotText; 
    private InventoryManager inventoryManager;
    private Image InventorySlotImage; 
    private Color defaultColor = Color.white;
    
    private int columns; // Set in Initialize()
    
    private int x; // Row (slow changing)
    private int y; // Column (fast changing)

    public int X => x; 
    public int Y => y; 
    
    void Awake()
    {
        InventorySlotImage = GetComponent<Image>();
        if (InventorySlotImage != null)
        {
            defaultColor = InventorySlotImage.color;
        }
    }
    
    void Start()
    {
        // Start is empty; initialization is moved to Initialize()
    }

    public void Initialize(InventoryManager manager, int columnCount)
    {
        inventoryManager = manager;
        columns = columnCount; // <-- Setting the column count here prevents DivideByZeroException
        
        // Now that 'columns' is guaranteed to be non-zero, it's safe to run this:
        UpdatePositionText();
    }

    // --- MOUSE INTERACTION METHODS ---

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryManager != null)
        {
            // Notify the manager of the click event
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InventorySlotImage != null) InventorySlotImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventorySlotImage != null) InventorySlotImage.color = defaultColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<RectTransform>().position = this.GetComponent<RectTransform>().position;
            // Additional logic for updating inventory data can be added here
        }
    }

    // --- UTILITY METHODS ---

    public void UpdatePositionText()
    {
        // Safety check included for robustness
        if (columns == 0) return; 
        
        int index = transform.GetSiblingIndex();
        
        int x = index / columns; // Row
        int y = index % columns; // Column
        
        if (InventorySlotText != null)
        {
            InventorySlotText.text = $"({x}, {y})"; 
        }
    }
    
    public int GetIndex()
    {
        return transform.GetSiblingIndex();
    }
}