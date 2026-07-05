using UnityEngine;

public class PlayerFearAudio : MonoBehaviour
{
    [Header("Korku Sesleri (Normal Mod)")]
    public AudioSource heartbeatSource;
    public AudioSource breathingSource;

    [Header("Aksiyon Müziði (Ýlaç Alýnýnca)")]
    public AudioSource hunterMusicSource;

    [Header("Mesafe Ayarlarý")]
    public float detectionRadius = 15f;
    public float panicRadius = 3f;

    [Header("Ses Efekt Ayarlarý")]
    public float maxVolume = 1f;
    public float maxMusicVolume = 0.7f;
    public float maxHeartbeatPitch = 1.5f;

    private GameObject[] enemies;

    void Start()
    {
        // Baþlangýçta korku seslerini sessizce baþlatýyoruz
        if (heartbeatSource != null) { heartbeatSource.volume = 0f; heartbeatSource.loop = true; if (!heartbeatSource.isPlaying) heartbeatSource.Play(); }
        if (breathingSource != null) { breathingSource.volume = 0f; breathingSource.loop = true; if (!breathingSource.isPlaying) breathingSource.Play(); }

        // Aksiyon müziðinin baþtan ÇALMADIÐINDAN emin oluyoruz
        if (hunterMusicSource != null) { hunterMusicSource.Stop(); hunterMusicSource.loop = true; }
    }

    void Update()
    {
        // --- ÝLAÇ ALINDI MI KONTROLÜ ---
        if (GameManager.Instance != null && GameManager.Instance.isHunterMode)
        {
            // Ýlacý aldýðýmýz için eski korku seslerini yavaþça susturuyoruz
            if (heartbeatSource != null) heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, 0f, Time.deltaTime * 3f);
            if (breathingSource != null) breathingSource.volume = Mathf.Lerp(breathingSource.volume, 0f, Time.deltaTime * 3f);

            // YENÝ ÞARKIYI BAÞLAT
            if (hunterMusicSource != null && !hunterMusicSource.isPlaying)
            {
                hunterMusicSource.volume = maxMusicVolume; // Sesi direkt belirlediðimiz seviyeden baþlasýn
                hunterMusicSource.Play();
            }

            return; // Ýlaç alýndýðý için aþaðýdaki kodlarý KESÝNLÝKLE okuma
        }

        // --- ÝLAÇ ALINMADIYSA (NORMAL OYUN DÖNGÜSÜ) ---
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            CalmDown();
            return;
        }

        float closestDistance = Mathf.Infinity;

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

        // Düþman yaklaþýnca kalp ve nefes seslerinin dinamik artýþý
        if (closestDistance <= detectionRadius)
        {
            float fearFactor = 1f - Mathf.Clamp01((closestDistance - panicRadius) / (detectionRadius - panicRadius));

            if (heartbeatSource != null) heartbeatSource.volume = Mathf.Lerp(0f, maxVolume, fearFactor);
            if (breathingSource != null) breathingSource.volume = Mathf.Lerp(0f, maxVolume, fearFactor);

            if (heartbeatSource != null) heartbeatSource.pitch = Mathf.Lerp(1f, maxHeartbeatPitch, fearFactor);
            if (breathingSource != null) breathingSource.pitch = Mathf.Lerp(1f, maxHeartbeatPitch, fearFactor);
        }
        else
        {
            CalmDown();
        }
    }

    private void CalmDown()
    {
        if (heartbeatSource != null) heartbeatSource.volume = Mathf.Lerp(heartbeatSource.volume, 0f, Time.deltaTime * 2f);
        if (breathingSource != null) breathingSource.volume = Mathf.Lerp(breathingSource.volume, 0f, Time.deltaTime * 2f);

        if (heartbeatSource != null) heartbeatSource.pitch = Mathf.Lerp(heartbeatSource.pitch, 1f, Time.deltaTime * 2f);
        if (breathingSource != null) breathingSource.pitch = Mathf.Lerp(breathingSource.pitch, 1f, Time.deltaTime * 2f);
    }
}