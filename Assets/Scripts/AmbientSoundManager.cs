using UnityEngine;
using System.Collections;

public class AmbientSoundManager : MonoBehaviour
{
    [Header("Ses Kaynaðý")]
    public AudioSource audioSource;

    [Header("Ses Havuzu")]
    public AudioClip[] ambientSounds;

    [Header("Zamanlama Ayarlarý (Saniye)")]
    public float minWaitTime = 10f;
    public float maxWaitTime = 35f;

    [Header("Ses Seviyesi Ayarý")]
    [Range(0f, 1f)] // Inspector'da 0 ile 1 arasýnda þýk bir kaydýrma çubuðu oluþturur
    public float ambientVolume = 0.4f; // Baþlangýç olarak %40 seviyesine ayarladýk

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (ambientSounds != null && ambientSounds.Length > 0)
        {
            StartCoroutine(PlayRandomAmbientSounds());
        }
        else
        {
            Debug.LogWarning("AmbientSoundManager: Ses havuzuna hiç ses eklemedin!");
        }
    }

    private IEnumerator PlayRandomAmbientSounds()
    {
        while (true)
        {
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            int randomIndex = Random.Range(0, ambientSounds.Length);
            AudioClip clipToPlay = ambientSounds[randomIndex];

            if (clipToPlay != null)
            {
                // PlayOneShot komutuna ikinci bir özellik olarak belirlediðimiz ses seviyesini (ambientVolume) gönderiyoruz
                audioSource.PlayOneShot(clipToPlay, ambientVolume);
            }
        }
    }
}