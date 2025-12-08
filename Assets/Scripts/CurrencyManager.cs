using UnityEngine;
using TMPro; 

public class CurrencyManager : MonoBehaviour
{
    public int currentGold = 100;
    public TextMeshProUGUI goldText;

    private void Start() => UpdateUI();

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    void UpdateUI()
    {
        if (goldText != null) goldText.text = "Gold: " + currentGold;
    }
}