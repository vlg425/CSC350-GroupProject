using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler, ICanvasRaycastFilter
{
    // ... (Previous Variables: itemData, Width, Height, CurrentShape, SaveData vars, Components) ...
    public InventoryItemSO itemData;
    public int Height { get; private set; }
    public int Width { get; private set; }
    public bool[] CurrentShape { get; private set; }
    public int GridStartX { get; private set; }
    public int GridStartY { get; private set; }
    public bool IsRotated { get; private set; }
    private RectTransform rectTransform;
    private Image image;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start() { if (itemData != null) Initialize(itemData); }

    public void Initialize(InventoryItemSO data)
    {
        itemData = data;
        image.sprite = data.icon;
        image.color = data.color;
        Width = itemData.width;
        Height = itemData.height;
        if (itemData.shape != null) CurrentShape = (bool[])itemData.shape.Clone();
        else CurrentShape = new bool[Width * Height]; 
        rectTransform.sizeDelta = new Vector2(Width * 100, Height * 100);
    }
    
    public void SetGridPosition(int r, int c) { GridStartX = r; GridStartY = c; }

    public void Rotate()
    {
        IsRotated = !IsRotated; 
        rectTransform.localRotation *= Quaternion.Euler(0, 0, -90);
        int oldWidth = Width; int oldHeight = Height;
        Width = oldHeight; Height = oldWidth;
        if (CurrentShape != null && CurrentShape.Length > 0) {
            bool[] newShape = new bool[CurrentShape.Length];
            for (int r = 0; r < Height; r++) {
                for (int c = 0; c < Width; c++) {
                    int oldRow = oldHeight - 1 - c; int oldCol = r;
                    int newIndex = r * Width + c; int oldIndex = oldRow * oldWidth + oldCol;
                    if(oldIndex >= 0 && oldIndex < CurrentShape.Length) newShape[newIndex] = CurrentShape[oldIndex];
                }
            }
            CurrentShape = newShape;
        }
    }

    // --- UPDATED: Pass the full eventData to the manager ---
    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.OnItemClicked(this, eventData);
    }
    // ------------------------------------------------------

    public void SetPickedUpState(bool pickedUp)
    {
        canvasGroup.alpha = pickedUp ? 0.6f : 1f; 
        canvasGroup.blocksRaycasts = !pickedUp; 
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, eventCamera, out localPoint);
        float normalizedX = localPoint.x + (rectTransform.rect.width / 2);
        float normalizedY = localPoint.y + (rectTransform.rect.height / 2);
        float cellWidth = rectTransform.rect.width / Width;
        float cellHeight = rectTransform.rect.height / Height;
        int col = Mathf.FloorToInt(normalizedX / cellWidth);
        int row = Mathf.FloorToInt((rectTransform.rect.height - normalizedY) / cellHeight);
        if (row < 0 || row >= Height || col < 0 || col >= Width) return false; 
        return IsPartOfShape(row, col);
    }

    private bool IsPartOfShape(int r, int c)
    {
        if (CurrentShape == null || CurrentShape.Length == 0) return true;
        int index = r * Width + c;
        if (index >= 0 && index < CurrentShape.Length) return CurrentShape[index];
        return true; 
    }
}