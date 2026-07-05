using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // 4. Durumu ekledik: Kaçýþ (GameManager tarafýndan tetiklenecek)
    public enum AIState { Wandering, Chasing, Attacking, Fleeing }
    public AIState currentState = AIState.Wandering;

    [Header("Hedef")]
    public Transform playerTarget;
    public float eyeHeight = 1f;

    [Header("Görüþ & Algýlama (Wander Modu)")]
    public float viewRadius = 15f;       // Görme mesafesi
    [Range(0, 360)]
    public float viewAngle = 90f;        // Görüþ açýsý
    public float closeAwarenessRadius = 3f; // Bu mesafeye girersen arkasý dönük olsa bile seni HÝSSEDER

    [Header("Kovalama Ayarlarý (Chase Modu)")]
    public float loseRadius = 25f;       // Peþini býrakmasý için arayý ne kadar açman gerektiði

    [Header("Saldýrý Ayarlarý (Attack Modu)")]
    public float attackRadius = 2f;      // Sana vurmak için duracaðý mesafe

    [Header("Gezinme Ayarlarý")]
    public float wanderRadius = 20f;
    private Vector3 patrolCenter;

    [Header("Kaçýþ Ayarlarý (Hunter Modu)")]
    public float fleeDistance = 15f;     // Ýlacý aldýðýnda senden ne kadar uzaða kaçmaya çalýþacak

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolCenter = transform.position;

        if (playerTarget == null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }

        SetRandomWanderDestination();
    }

    // GameManager'ýn (veya hapýn) çaðýracaðý fonksiyon
    public void StartFleeing()
    {
        currentState = AIState.Fleeing;
        agent.isStopped = false;
        agent.speed += 2f; // Panikledikleri için biraz daha hýzlý koþarlar
    }

    void Update()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // --- 1. STATE GEÇÝÞ KONTROLLERÝ (Karar Verme Merkezi) ---
        switch (currentState)
        {
            case AIState.Wandering:
                if (distanceToPlayer <= closeAwarenessRadius || CanSeePlayer(distanceToPlayer))
                {
                    currentState = AIState.Chasing;
                }
                break;

            case AIState.Chasing:
                if (distanceToPlayer > loseRadius)
                {
                    currentState = AIState.Wandering;
                    SetRandomWanderDestination();
                }
                else if (distanceToPlayer <= attackRadius)
                {
                    currentState = AIState.Attacking;
                }
                break;

            case AIState.Attacking:
                if (distanceToPlayer > attackRadius)
                {
                    currentState = AIState.Chasing;
                    agent.isStopped = false;
                }
                break;

            case AIState.Fleeing:
                // Kaçýþ modundayken artýk geri dönmez, sürekli kaçar (yakalanýp yok edilene kadar)
                break;
        }

        // --- 2. STATE EYLEMLERÝ (Fiziksel Hareketler) ---
        switch (currentState)
        {
            case AIState.Wandering: PerformWandering(); break;
            case AIState.Chasing: PerformChasing(); break;
            case AIState.Attacking: PerformAttacking(); break;
            case AIState.Fleeing: PerformFleeing(); break;
        }
    }

    // --- GÖRÜÞ SÝSTEMÝ ---
    private bool CanSeePlayer(float distance)
    {
        if (distance > viewRadius) return false;

        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        if (angleToPlayer < viewAngle / 2f)
        {
            return CheckLineOfSight();
        }
        return false;
    }

    private bool CheckLineOfSight()
    {
        RaycastHit hit;
        Vector3 startPos = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = playerTarget.position + Vector3.up * eyeHeight;
        Vector3 rayDir = (targetPos - startPos).normalized;

        if (Physics.Raycast(startPos, rayDir, out hit, viewRadius))
        {
            if (hit.transform == playerTarget) return true;
        }
        return false;
    }

    // --- DAVRANIÞLAR ---
    private void PerformChasing()
    {
        agent.isStopped = false;
        agent.SetDestination(playerTarget.position);
    }

    private void PerformWandering()
    {
        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetRandomWanderDestination();
        }
    }

    private void PerformAttacking()
    {
        agent.isStopped = true;

        Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
        lookDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    private void PerformFleeing()
    {
        // Oyuncudan düþmana doðru bir yön vektörü çiz (Ters yön)
        Vector3 runDirection = (transform.position - playerTarget.position).normalized;

        // Düþmanýn kendi pozisyonundan o ters yöne doðru fleeDistance kadar uzaðý hedefle
        Vector3 targetPos = transform.position + (runDirection * fleeDistance);

        NavMeshHit hit;
        // O hedef noktanýn NavMesh üzerinde yürünebilir bir yer olup olmadýðýný kontrol et
        if (NavMesh.SamplePosition(targetPos, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void SetRandomWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += patrolCenter;

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, -1))
        {
            agent.SetDestination(navHit.position);
        }
    }

    // --- SALDIRI ALMA SÝSTEMÝ ---
    public void TakeDamage()
    {
        // Sadece bizden kaçarken (Phase 2'de) hasar alabilirler
        if (currentState == AIState.Fleeing)
        {
            Debug.Log("Düþman vuruldu ve yok edildi!");

            // --- YENÝ EKLENEN KISIM: GameManager'a ölüm sinyali gönder ---
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EnemyDied();
            }

            Destroy(gameObject);
        }
    }

    // Görselleþtirmeler (Gizmos)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeAwarenessRadius);

        Vector3 fovLine1 = Quaternion.AngleAxis(viewAngle / 2, Vector3.up) * transform.forward;
        Vector3 fovLine2 = Quaternion.AngleAxis(-viewAngle / 2, Vector3.up) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, fovLine1 * viewRadius);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, fovLine2 * viewRadius);
    }
}