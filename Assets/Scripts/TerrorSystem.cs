using UnityEngine;
using UnityEngine.UI; // REQUIRED to talk to UI elements

public class TerrorSystem : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Transform player;       
    public Transform[] enemies;    
    
    [Header("UI Elements")]
    public Slider terrorMeter;         // The UI Slider we just made
    public GameObject gameOverScreen;  // The Bayilma panel

    [Header("Terror Settings")]
    public float currentTerror = 0f;
    public float maxTerror = 100f; 
    public float terrorRadius = 15f; 
    
    public float baseIncreaseRate = 5f; 
    public float decreaseRate = 3f;

    private bool isFainted = false; // Prevents the game over from triggering multiple times

    void Update()
    {
        // Stop calculating if the player is already dead
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

        if (currentTerror >= maxTerror)
        {
            TriggerFaintState();
        }
    }

    void TriggerFaintState()
    {
        isFainted = true;
        Debug.Log("100% TERROR REACHED: BAYILMA EKRANI TETIKLENDI!");

        // 1. Activate the black screen
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        // 2. Freeze the game
        Time.timeScale = 0f; 
        
        // 3. Unlock the mouse cursor so you can click restart buttons later
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}