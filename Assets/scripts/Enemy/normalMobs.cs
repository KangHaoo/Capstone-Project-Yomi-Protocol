using UnityEngine;
using UnityEngine.AI;

public class normalmobs : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround;

    public float health;

    public int meleeDamage = 10;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    bool alreadyAttacked;

    public float timeBetweenAttacks = 1.5f;  // The time between attacks
    private float attackCooldownTimer = 0f;   // Timer to track cooldown


    //States
    public float sightRange, meleeRange;
    public bool playerInSightRange, playerInMeleeRange;


    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Countdown the cooldown timer if it's greater than 0
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        // Existing code for detecting the player and handling behavior
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sightRange);
        playerInSightRange = false;
        playerInMeleeRange = false;

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                player = hitCollider.transform;
                playerInSightRange = true;

                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= meleeRange)
                {
                    playerInMeleeRange = true;
                }

                break; // Stop after finding the first player
            }
        }

        // Choose behavior based on player proximity
        if (!playerInSightRange && !playerInMeleeRange) Patroling();
        else if (playerInSightRange && !playerInMeleeRange) ChasePlayer();
        else if (playerInSightRange && playerInMeleeRange) AttackPlayer();
    }


    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        // Stop moving and face the player
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        // Check if the attack cooldown timer is done
        if (attackCooldownTimer <= 0f)
        {
            // Melee attack - damage the player
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(meleeDamage);
            }

            // Reset the cooldown timer to the time specified between attacks
            attackCooldownTimer = timeBetweenAttacks;
        }
    }


    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
