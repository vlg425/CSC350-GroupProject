using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    public InventoryItemSO itemData;

    public int Height { get; private set; }
    public int Width { get; private set; }
    public bool[] CurrentShape { get; private set; }

    private RectTransform rectTransform;
    private Image image;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        if (itemData != null) Initialize(itemData);
    }

    public void Initialize(InventoryItemSO data)
    {
        itemData = data;
        image.sprite = data.icon;
        
        // --- NEW: Apply Color ---
        image.color = data.color;
        // ------------------------
        
        Width = itemData.width;
        Height = itemData.height;
        
        if (itemData.shape != null)
            CurrentShape = (bool[])itemData.shape.Clone();
        else
            CurrentShape = new bool[Width * Height]; 

        // Set size assuming 100x100 slots for now
        rectTransform.sizeDelta = new Vector2(Width * 100, Height * 100);
    }

    public void Rotate()
    {
        rectTransform.localRotation *= Quaternion.Euler(0, 0, -90);

        int oldWidth = Width;
        int oldHeight = Height;
        Width = oldHeight;
        Height = oldWidth;

        if (CurrentShape != null && CurrentShape.Length > 0)
        {
            bool[] newShape = new bool[CurrentShape.Length];
            for (int r = 0; r < Height; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    int oldRow = oldHeight - 1 - c;
                    int oldCol = r;
                    int newIndex = r * Width + c;
                    int oldIndex = oldRow * oldWidth + oldCol;
                    
                    if(oldIndex >= 0 && oldIndex < CurrentShape.Length)
                        newShape[newIndex] = CurrentShape[oldIndex];
                }
            }
            CurrentShape = newShape;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.Instance.OnItemClicked(this);
        }
    }

    public void SetPickedUpState(bool pickedUp)
    {
        canvasGroup.alpha = pickedUp ? 0.6f : 1f; 
        canvasGroup.blocksRaycasts = !pickedUp; 
    }
}