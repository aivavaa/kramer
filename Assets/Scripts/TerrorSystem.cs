using UnityEngine;

public class TerrorSystem : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Transform player;       // Your player character
    public Transform[] enemies;    // Array to hold all your enemies

    [Header("Terror Settings")]
    public float currentTerror = 0f;
    public float maxTerror = 100f; // 100% max as requested
    public float terrorRadius = 15f; // How close enemies need to be to trigger terror
    
    // Speeds for the meter
    public float baseIncreaseRate = 5f; 
    public float decreaseRate = 3f;

    void Update()
    {
        bool enemyIsNear = false;
        float closestDistance = Mathf.Infinity;

        // 1. Check the distance to every enemy in the scene
        foreach (Transform enemy in enemies)
        {
            if (enemy != null)
            {
                // Calculate distance between player and this specific enemy
                float distance = Vector3.Distance(player.position, enemy.position);

                // If they are inside the danger zone
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

        // 2. Increase or Decrease Terror based on distance
        if (enemyIsNear)
        {
            // "Uzaklığına göre artan" - The closer they are, the faster it rises
            // This math creates a multiplier: closer to 0 distance = higher multiplier
            float distanceMultiplier = 1f - (closestDistance / terrorRadius); 
            
            // Increase terror. Time.deltaTime ensures it rises smoothly over time, not per frame
            currentTerror += (baseIncreaseRate + (distanceMultiplier * 10f)) * Time.deltaTime;
        }
        else
        {
            // "Düşman uzaklaştığında metrenin yavaşça düşmesi"
            currentTerror -= decreaseRate * Time.deltaTime;
        }

        // 3. Keep the number locked between 0 and 100
        currentTerror = Mathf.Clamp(currentTerror, 0f, maxTerror);

        // 4. Trigger Game Over at 100%
        if (currentTerror >= maxTerror)
        {
            TriggerFaintState();
        }
    }

    void TriggerFaintState()
    {
        // For now, this just prints to the console so you know it works.
        // Later, you will load your Game Over scene or UI here.
        Debug.Log("100% TERROR REACHED: BAYILMA EKRANI TETIKLENDI!");
    }
}