using UnityEngine;

public class PlayerFearAudio : MonoBehaviour
{
    [Header("Ses Kaynaklarý")]
    public AudioSource heartbeatSource;
    public AudioSource breathingSource;

    [Header("Mesafe Ayarlarý")]
    public float detectionRadius = 15f; // Seslerin duyulmaya baþlayacaðý maksimum mesafe
    public float panicRadius = 3f;      // Seslerin en yüksek ve en hýzlý olacaðý (dibine girdiði) mesafe

    [Header("Ses Efekt Ayarlarý")]
    public float maxVolume = 1f;
    public float maxHeartbeatPitch = 1.5f; // Kalbin ne kadar hýzlý atacaðý (Normal hýz 1'dir)

    private GameObject[] enemies;

    void Start()
    {
        // Baþlangýçta seslerin düzeyini sýfýrla ama arka planda döngüyle çalmaya baþlasýnlar
        if (heartbeatSource != null)
        {
            heartbeatSource.volume = 0f;
            heartbeatSource.loop = true;
            if (!heartbeatSource.isPlaying) heartbeatSource.Play();
        }

        if (breathingSource != null)
        {
            breathingSource.volume = 0f;
            breathingSource.loop = true;
            if (!breathingSource.isPlaying) breathingSource.Play();
        }
    }

    void Update()
    {
        // Sahnedeki "Enemy" etiketli tüm düþmanlarý bul
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Eðer sahnede hiç düþman kalmadýysa (hepsi öldüyse) sakinleþ
        if (enemies.Length == 0)
        {
            CalmDown();
            return;
        }

        float closestDistance = Mathf.Infinity;

        // En yakýn düþmaný hesapla
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
        }

        // Eðer en yakýn düþman algýlama menzilindeyse korku seviyesini ayarla
        if (closestDistance <= detectionRadius)
        {
            // 0 (uzak) ile 1 (çok yakýn) arasýnda bir korku çarpaný hesapla
            float fearFactor = 1f - Mathf.Clamp01((closestDistance - panicRadius) / (detectionRadius - panicRadius));

            // Ses seviyelerini (Volume) mesafeye göre artýr
            if (heartbeatSource != null) heartbeatSource.volume = Mathf.Lerp(0f, maxVolume, fearFactor);
            if (breathingSource != null) breathingSource.volume = Mathf.Lerp(0f, maxVolume, fearFactor);

            // Kalp atýþýný ve nefesi hýzlandýr (Pitch deðerini mesafeye göre artýr)
            if (heartbeatSource != null) heartbeatSource.pitch = Mathf.Lerp(1f, maxHeartbeatPitch, fearFactor);
            if (breathingSource != null) breathingSource.pitch = Mathf.Lerp(1f, maxHeartbeatPitch, fearFactor);
        }
        else
        {
            // Düþman uzaktaysa yavaþça sakinleþ
            CalmDown();
        }
    }

    private void CalmDown()
    {
        // Sesleri küt diye kesmek yerine yavaþça kýs (Daha gerçekçi hissettirir)
        if (heartbeatSource != null) heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, 0f, Time.deltaTime * 2f);
        if (breathingSource != null) breathingSource.volume = Mathf.Lerp(breathingSource.volume, 0f, Time.deltaTime * 2f);

        // Hýzlarý normale (1) döndür
        if (heartbeatSource != null) heartbeatSource.pitch = Mathf.Lerp(heartbeatSource.pitch, 1f, Time.deltaTime * 2f);
        if (breathingSource != null) breathingSource.pitch = Mathf.Lerp(breathingSource.pitch, 1f, Time.deltaTime * 2f);
    }
}