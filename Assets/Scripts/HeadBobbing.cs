using UnityEngine;

public class HeadBobbing : MonoBehaviour
{
    [Header("Baðlantýlar")]
    public FirstPersonMovement playerMovement;
    public Rigidbody playerRigidbody;

    [Header("Yalpalama Ayarlarý")]
    public float walkBobSpeed = 12f;
    public float runBobSpeed = 16f;
    public float bobAmountY = 0.05f; // Yukarý aþaðý sekme
    public float tiltAmountZ = 1.5f; // Saða sola yatma (Þizofrenik yalpalama)

    private float defaultPosY;
    private float timer;

    void Start()
    {
        defaultPosY = transform.localPosition.y;

        if (playerMovement == null) playerMovement = GetComponentInParent<FirstPersonMovement>();
        if (playerRigidbody == null) playerRigidbody = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        // Karakter yerdeyse ve hareket ediyorsa
        if (playerMovement.IsGrounded && playerRigidbody.linearVelocity.magnitude > 0.1f)
        {
            float currentSpeed = playerMovement.IsRunning ? runBobSpeed : walkBobSpeed;
            timer += Time.deltaTime * currentSpeed;

            // Yukarý aþaðý sekme (Sinüs dalgasý)
            float newY = defaultPosY + (Mathf.Sin(timer) * bobAmountY);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);

            // Saða sola kafa yatýrma (Cosinüs dalgasý)
            float tiltZ = Mathf.Cos(timer) * tiltAmountZ;
            // Kameranýn mevcut X ve Y fare dönüþlerini bozmadan sadece Z'yi (yatmayý) ekliyoruz
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, tiltZ);
        }
        else
        {
            // Durduðumuzda kamerayý yumuþakça merkeze ve düz açýya al
            timer = 0;
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, defaultPosY, transform.localPosition.z), Time.deltaTime * 5f);

            Quaternion resetRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, resetRotation, Time.deltaTime * 5f);
        }
    }
}