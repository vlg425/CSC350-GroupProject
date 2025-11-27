// InventoryGrid.cs (Full Code)

public class InventoryGrid
{
    int width; // Renamed from width
    int height; // Renamed from height
    float cellSize;
    

    // Constructor parameters now accept (rows, columns)
    public InventoryGrid(int height, int width, float cellSize)
    {
        this.height = height;
        this.width = width;
        this.cellSize = cellSize;
        // Array initialized as [Rows, Columns]
        // gridArray = new InventoryItemSO[height, width]; 

        // for(int x = 0; x < gridArray.GetLength(0); x++) // Iterating through Rows (x)
        // {
        //     for(int y = 0; y < gridArray.GetLength(1); y++) // Iterating through Columns (y)
        //     {
        //         gridArray[x, y] = null;
        //     }
        // }
    }
}