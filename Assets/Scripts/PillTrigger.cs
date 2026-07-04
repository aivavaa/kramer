using UnityEngine;

public class PillTrigger : MonoBehaviour
{
    // Bir obje bu tetikleyicinin içine girdiðinde otomatik çalýþýr
    private void OnTriggerEnter(Collider other)
    {
        // Eðer çarpan obje bizim karakterimiz ise (Tag kontrolü)
        if (other.CompareTag("Player"))
        {
            // GameManager'daki o kýrýlma aný fonksiyonunu çalýþtýr
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActivateOneMoreTime();
            }
            else
            {
                Debug.LogError("Sahnede GameManager bulunamadý!");
            }

            // Hapý sahneden yok et (Yutmuþ olduk)
            Destroy(gameObject);
        }
    }
}