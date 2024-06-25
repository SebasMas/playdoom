using System.Collections;
using UnityEngine;

/// <summary>
/// Controla el comportamiento del NPC Crow.
/// </summary>
public class CrowBehaviour : MonoBehaviour
{
    [Header("Target and Movement")]
    /// <summary>Referencia al objeto jugador (Knight3).</summary>
    public GameObject target;
    /// <summary>Velocidad de movimiento del Crow.</summary>
    public float speed = 2f;
    /// <summary>Distancia máxima a la que Crow perseguirá al jugador.</summary>
    public float chasingDistance = 5f;

    [Header("Attack")]
    /// <summary>Tiempo de espera entre ataques.</summary>
    public float attackCooldown = 2f;
    /// <summary>Componente que contiene la información de ataque del Crow.</summary>
    public EnemyAttack attackComponent;
    /// <summary>Indica si Crow está actualmente en estado de ataque.</summary>
    public bool isAttacking = false;

    [Header("Audio")]
    /// <summary>Sonido que se reproduce cuando Crow recibe daño.</summary>
    public AudioSource damaged;
    /// <summary>Sonido que se reproduce cuando Crow ataca.</summary>
    public AudioSource attackSound;
    /// <summary>Sonido que se reproduce cuando Crow muere.</summary>
    public AudioSource dyingSound;

    [Header("Health")]
    /// <summary>Salud máxima de Crow.</summary>
    public int maxHealth = 100;
    /// <summary>Salud actual de Crow.</summary>
    private int currentHealth;
    /// <summary>Referencia a la barra de salud de Crow.</summary>
    public HealthBar healthBar;

    [Header("Colliders")]
    /// <summary>Collider que define el área de ataque de Crow.</summary>
    public Collider2D attackCollider;

    /// <summary>Referencia al SpawnManager que gestiona este Crow.</summary>
    [HideInInspector]
    public SpawnManager spawnManager;

    private Animator animator;
    private float lastDamageTime = 0;
    private float damageCooldown = 0.5f;
    private float lastAttackTime;
    private bool playerInRange = false;

    /// <summary>
    /// Inicializa los componentes y variables necesarias.
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        if (target == null)
        {
            Debug.LogError("No hay target asignado para Crow");
        }
        
        if (attackComponent == null)
        {
            attackComponent = GetComponentInChildren<EnemyAttack>();
            if (attackComponent == null)
            {
                Debug.LogError("No se encontró el componente EnemyAttack en Crow");
            }
        }

        InitializeHealth();
    }

    /// <summary>
    /// Inicializa la salud de Crow y configura la barra de salud.
    /// </summary>
    private void InitializeHealth()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }
    }

    /// <summary>
    /// Actualiza el comportamiento de Crow en cada frame.
    /// </summary>
    void Update()
    {
        if (target != null)
        {
            HandleMovement();
            UpdateOrientation();
            
            if (playerInRange && Time.time > lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
    }

    /// <summary>
    /// Maneja el movimiento de Crow hacia el jugador.
    /// </summary>
    void HandleMovement()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.transform.position);
        bool isMoving = distanceToPlayer < chasingDistance && !playerInRange;
        if (isMoving)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
        animator.SetBool("Walking", isMoving);
    }

    /// <summary>
    /// Actualiza la orientación de Crow para mirar hacia el jugador.
    /// </summary>
    void UpdateOrientation()
    {
        float directionX = target.transform.position.x - transform.position.x;
        if (Mathf.Sign(transform.localScale.x) != Mathf.Sign(directionX))
        {
            transform.localScale = new Vector3(Mathf.Sign(directionX), 1.0f, 1.0f);
        }
    }

    /// <summary>
    /// Realiza el ataque al jugador.
    /// </summary>
    void AttackPlayer()
    {
        isAttacking = true;
        animator.SetTrigger("Attacking");
        lastAttackTime = Time.time;
        
        if (attackSound != null)
        {
            attackSound.Play();
        }

        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }

        Debug.Log("Crow atacó al jugador");
        StartCoroutine(ResetAttackState());
    }

    /// <summary>
    /// Reinicia el estado de ataque después de un tiempo.
    /// </summary>
    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(1.0f);
        isAttacking = false;
        
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }
    }

    /// <summary>
    /// Detecta cuando otros objetos entran en el trigger de Crow.
    /// </summary>
    /// <param name="other">El collider que entró en el trigger.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Knight3Movement playerScript = other.transform.parent.GetComponent<Knight3Movement>();
            if (playerScript != null && playerScript.isAttacking && Time.time > lastDamageTime + damageCooldown)
            {
                TakeDamage(playerScript.attackDamage);
            }
        }
        else if (other.CompareTag("Player") && other.gameObject == target)
        {
            playerInRange = true;
        }
    }

    /// <summary>
    /// Detecta cuando otros objetos salen del trigger de Crow.
    /// </summary>
    /// <param name="other">El collider que salió del trigger.</param>
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject == target)
        {
            playerInRange = false;
        }
    }

    /// <summary>
    /// Maneja la muerte de Crow.
    /// </summary>
    void Die()
    {
        Debug.Log("Crow ha sido derrotado");
        animator.SetTrigger("Die");

        if (dyingSound != null) dyingSound.Play();

        if (spawnManager != null)
        {
            spawnManager.CrowDied(this);
        }
        
        StartCoroutine(FadeAndDestroy());
    }

    /// <summary>
    /// Aplica daño a Crow y verifica si debe morir.
    /// </summary>
    /// <param name="damageAmount">Cantidad de daño a aplicar.</param>
    void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        lastDamageTime = Time.time;
        animator.SetTrigger("TakingDamage");
        if (damaged != null)
        {
            damaged.Play();
        }
        
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
        
        Debug.Log($"Crow recibió {damageAmount} de daño. Salud restante: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Corrutina que se encarga de desvanecer gradualmente el sprite del NPC y luego destruir el objeto.
    /// </summary>
    IEnumerator FadeAndDestroy()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        float fadeTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}