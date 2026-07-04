using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton yapýsý: Sahnedeki her kod bu GameManager'a kolayca ulaþabilsin diye.
    public static GameManager Instance;

    public bool isHunterMode = false; // Oyunun durumu

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Hap (Küp) alýndýðýnda bu fonksiyon çalýþacak
    public void ActivateOneMoreTime()
    {
        isHunterMode = true;
        Debug.Log("ÝLAÇ ALINDI! ONE MORE TIME BAÞLADI!");

        // 1. DÜÞMANLARI KAÇIR (Senin kýsmýn)
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in allEnemies)
        {
            enemy.StartFleeing();
        }

        // 2. GÖRSEL VE SES EFEKTLERÝ (Arkadaþýnýn kýsmý)
        TriggerHunterVisuals();
    }

    private void TriggerHunterVisuals()
    {
        // TODO: (Arkadaþýn için) FOV artýrma kodunu buraya yaz.
        // TODO: (Arkadaþýn için) Post-Processing / Mor renk paleti geçiþini buraya yaz.
        // TODO: (Arkadaþýn için) Kaset/VHS sesini tersine çevirme kodunu buraya ekle.
    }
}