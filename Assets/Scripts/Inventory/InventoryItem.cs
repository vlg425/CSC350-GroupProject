using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler, ICanvasRaycastFilter
{
    public InventoryItemSO itemData;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool[] CurrentShape { get; private set; }
    
    // Save Data
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public int RotationIndex { get; private set; } 

    private RectTransform rt;
    private CanvasGroup cg;
    private Image img;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        cg = GetComponent<CanvasGroup>();
    }

    public void Initialize(InventoryItemSO data)
    {
        itemData = data;
        img.sprite = data.icon;
        img.color = data.color;
        Width = data.width;
        Height = data.height;
        RotationIndex = 0;
        
        CurrentShape = data.shape != null ? (bool[])data.shape.Clone() : new bool[Width * Height];
        rt.sizeDelta = new Vector2(Width * 100, Height * 100);
    }

    public void SetGridPosition(int x, int y) { GridX = x; GridY = y; }

    public void Rotate()
    {
        RotationIndex++;
        if (RotationIndex > 3) RotationIndex = 0;

        rt.localRotation *= Quaternion.Euler(0, 0, -90);
        (Width, Height) = (Height, Width); 

        if (CurrentShape?.Length > 0)
        {
            bool[] newShape = new bool[CurrentShape.Length];
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    newShape[r * Width + c] = CurrentShape[(Width - 1 - c) * Height + r]; 
            CurrentShape = newShape;
        }
    }

    public void OnPointerClick(PointerEventData evt) => InventoryManager.Instance.OnItemClicked(this, evt);

    public void SetPickedUpState(bool state)
    {
        cg.alpha = state ? 0.6f : 1f;
        cg.blocksRaycasts = !state;
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, sp, cam, out Vector2 loc);
        float x = loc.x + (rt.rect.width / 2);
        float y = (rt.rect.height / 2) - loc.y; 
        int c = Mathf.FloorToInt(x / (rt.rect.width / Width));
        int r = Mathf.FloorToInt(y / (rt.rect.height / Height));
        if (r < 0 || r >= Height || c < 0 || c >= Width) return false;
        return IsPartOfShape(r, c);
    }

    public bool IsPartOfShape(int r, int c)
    {
        if (CurrentShape == null || CurrentShape.Length == 0) return true;
        int i = r * Width + c;
        return i >= 0 && i < CurrentShape.Length ? CurrentShape[i] : true;
    }
}