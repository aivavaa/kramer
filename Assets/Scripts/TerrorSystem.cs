using UnityEngine;
using UnityEngine.UI; // REQUIRED to talk to UI elements

public class TerrorSystem : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Transform player;       
    public Transform[] enemies;    
    
    [Header("UI Elements")]
    public Slider terrorMeter;         
    public GameObject gameOverScreen;  

    [Header("Expressions")]
    public Image expressionUI;         // The UI Image component where the face is displayed
    public Sprite[] expressionSprites; // Array of 5 sprites (0 = calm, 4 = terrified)

    [Header("Terror Settings")]
    public float currentTerror = 0f;
    public float maxTerror = 100f; 
    public float terrorRadius = 15f; 
    
    public float baseIncreaseRate = 5f; 
    public float decreaseRate = 3f;

    private bool isFainted = false; 

    void Update()
    {
        if (isFainted) return; 

        bool enemyIsNear = false;
        float closestDistance = Mathf.Infinity;

        foreach (Transform enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(player.position, enemy.position);

                if (distance < terrorRadius)
                {
                    enemyIsNear = true;
                    if (distance < closestDistance) 
                    {
                        closestDistance = distance;
                    }
                }
            }
        }

        if (enemyIsNear)
        {
            float distanceMultiplier = 1f - (closestDistance / terrorRadius); 
            currentTerror += (baseIncreaseRate + (distanceMultiplier * 10f)) * Time.deltaTime;
        }
        else
        {
            currentTerror -= decreaseRate * Time.deltaTime;
        }

        currentTerror = Mathf.Clamp(currentTerror, 0f, maxTerror);

        // UPDATE THE VISUAL UI METER
        if (terrorMeter != null)
        {
            terrorMeter.value = currentTerror;
        }

        // UPDATE THE EXPRESSION
        UpdateExpression();

        if (currentTerror >= maxTerror)
        {
            TriggerFaintState();
        }
    }

    void UpdateExpression()
    {
        // Make sure we have the UI Image and sprites assigned
        if (expressionUI == null || expressionSprites == null || expressionSprites.Length == 0) return;

        // Calculate terror as a percentage (0.0 to 1.0)
        float terrorPercent = currentTerror / maxTerror;

        // Map the percentage to an index in the array
        // Multiplying by the array length gives us a number from 0 to 5
        int spriteIndex = Mathf.FloorToInt(terrorPercent * expressionSprites.Length);

        // Clamp to ensure the index never goes out of bounds (e.g., if terror is exactly at 100%)
        spriteIndex = Mathf.Clamp(spriteIndex, 0, expressionSprites.Length - 1);

        // Apply the sprite to the UI Image
        expressionUI.sprite = expressionSprites[spriteIndex];
    }

    void TriggerFaintState()
    {
        isFainted = true;
        Debug.Log("100% TERROR REACHED: BAYILMA EKRANI TETIKLENDI!");

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        Time.timeScale = 0f; 
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}