using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Controla el movimiento y las acciones del personaje Knight3.
/// </summary>
public class Knight3Movement : MonoBehaviour
{
    [Header("Movement")]
    /// <summary>Velocidad de movimiento del personaje.</summary>
    public float moveSpeed = 5f;
    /// <summary>Fuerza de salto del personaje.</summary>
    public float jumpForce = 10f;

    [Header("Audio")]
    /// <summary>Fuente de audio para los pasos del personaje.</summary>
    public AudioSource footstepsAudioSource;
    /// <summary>Fuente de audio para el ataque del personaje.</summary>
    public AudioSource attackAudioSource;
    /// <summary>Fuente de audio para cuando el personaje recibe daño.</summary>
    public AudioSource damagedSound;

    [Header("Combat")]
    /// <summary>Indica si el personaje está atacando.</summary>
    public bool isAttacking = false;
    /// <summary>Collider usado para detectar los ataques del personaje.</summary>
    public CapsuleCollider2D attackCollider;
    /// <summary>Tiempo de espera entre daños recibidos.</summary>
    public float damageCooldown = 0.5f;
    /// <summary>Cantidad de daño que inflige el personaje con cada ataque.</summary>
    public int attackDamage = 10;

    [Header("UI")]
    /// <summary>Joystick virtual para el control en dispositivos móviles.</summary>
    public Joystick joystick;
    /// <summary>Botón de salto para la interfaz móvil.</summary>
    public Button jumpButton;
    /// <summary>Botón de ataque para la interfaz móvil.</summary>
    public Button attackButton;
    /// <summary>Botón de bloqueo para la interfaz móvil.</summary>
    public Button blockButton;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public GameObject healthBarObject;
    public float invincibilityDuration = 1f;
    private bool isInvincible = false;
    private HealthBar healthBar; 

    private Animator animator;
    private Rigidbody2D rb;

    /// <summary>
    /// Inicializa los componentes y configura la UI.
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        ConfigureUI();
        InitializeHealth();
    }

    /// <summary>
    /// Maneja la lógica de movimiento, salto, ataque y bloqueo en cada frame.
    /// </summary>
    void Update()
    {
        HandleMovement();
        HandleJumpInput();
        HandleAttackInput();
        HandleBlockInput();
    }

    /// <summary>
    /// Maneja el movimiento del personaje.
    /// </summary>
    void HandleMovement()
    {
        float move = 0f;
        bool isMoving = false;

#if UNITY_STANDALONE
        if (Input.GetKey(KeyCode.A))
        {
            move = -moveSpeed;
            transform.localScale = new Vector3(-1, 1, 1);
            isMoving = true;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            move = moveSpeed;
            transform.localScale = new Vector3(1, 1, 1);
            isMoving = true;
        }
#elif UNITY_ANDROID || UNITY_IOS
        if (joystick != null && (Mathf.Abs(joystick.Horizontal) > 0.2f))
        {
            move = moveSpeed * joystick.Horizontal;
            transform.localScale = new Vector3(Mathf.Sign(joystick.Horizontal), 1, 1);
            isMoving = true;
        }
#endif

        rb.velocity = new Vector2(move, rb.velocity.y);
        animator.SetBool("Walking", isMoving);
        PlayFootstepSound(isMoving && isGrounded());
    }

    /// <summary>
    /// Maneja la entrada para el salto.
    /// </summary>
    void HandleJumpInput()
    {
#if UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded())
        {
            PerformJump();
        }
#endif
        UpdateJumpAnimations();
    }

    /// <summary>
    /// Maneja la entrada para el ataque.
    /// </summary>
    void HandleAttackInput()
    {
#if UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
#endif
    }

    /// <summary>
    /// Maneja la entrada para el bloqueo.
    /// </summary>
    void HandleBlockInput()
    {
#if UNITY_STANDALONE
        if (Input.GetMouseButton(1))
        {
            StartBlocking();
        }
        else
        {
            StopBlocking();
        }
#endif
    }

    /// <summary>
    /// Actualiza las animaciones relacionadas con el salto.
    /// </summary>
    void UpdateJumpAnimations()
    {
        if (isGrounded())
        {
            animator.SetBool("Jumping", false);
        }

        bool isFalling = rb.velocity.y < 0 && !isGrounded();
        animator.SetBool("Falling", isFalling);
    }

    /// <summary>
    /// Verifica si el personaje está en el suelo.
    /// </summary>
    /// <returns>True si el personaje está en el suelo, false en caso contrario.</returns>
    private bool isGrounded()
    {
        return Mathf.Abs(rb.velocity.y) < 0.001f;
    }

    /// <summary>
    /// Reproduce o detiene el sonido de pasos.
    /// </summary>
    /// <param name="shouldPlay">Indica si se debe reproducir el sonido.</param>
    private void PlayFootstepSound(bool shouldPlay)
    {
        if (shouldPlay && !footstepsAudioSource.isPlaying)
        {
            footstepsAudioSource.Play();
        }
        else if (!shouldPlay && footstepsAudioSource.isPlaying)
        {
            footstepsAudioSource.Stop();
        }
    }

    /// <summary>
    /// Realiza el salto del personaje.
    /// </summary>
    public void PerformJump()
    {
        if (isGrounded())
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// Realiza el ataque del personaje.
    /// </summary>
    public void PerformAttack()
    {
        attackCollider.enabled = true;
        isAttacking = true;
        attackAudioSource.Play();
        animator.SetTrigger("Attacking");
        StartCoroutine(ResetAttack());
    }

    /// <summary>
    /// Inicia el bloqueo del personaje.
    /// </summary>
    public void StartBlocking()
    {
        animator.SetBool("Blocking", true);
        // Aquí puedes añadir lógica adicional para el bloqueo, como reducir el daño recibido
    }

    /// <summary>
    /// Detiene el bloqueo del personaje.
    /// </summary>
    public void StopBlocking()
    {
        animator.SetBool("Blocking", false);
    }

    /// <summary>
    /// Reinicia el estado de ataque después de un tiempo.
    /// </summary>
    /// <returns>IEnumerator para ser usado con StartCoroutine.</returns>
    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(0.5f);
        attackCollider.enabled = false;
        isAttacking = false;
    }

    /// <summary>
    /// Configura la UI según la plataforma.
    /// </summary>
    private void ConfigureUI()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (jumpButton != null) jumpButton.gameObject.SetActive(true);
        if (attackButton != null) attackButton.gameObject.SetActive(true);
        if (blockButton != null) blockButton.gameObject.SetActive(true);
        if (joystick != null) joystick.gameObject.SetActive(true);

        jumpButton.onClick.AddListener(PerformJump);
        attackButton.onClick.AddListener(PerformAttack);
        
        if (blockButton != null)
        {
            EventTrigger trigger = blockButton.gameObject.GetComponent<EventTrigger>() ?? blockButton.gameObject.AddComponent<EventTrigger>();
            
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDownEntry.callback.AddListener((data) => { StartBlocking(); });
            trigger.triggers.Add(pointerDownEntry);

            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUpEntry.callback.AddListener((data) => { StopBlocking(); });
            trigger.triggers.Add(pointerUpEntry);

            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            pointerExitEntry.callback.AddListener((data) => { StopBlocking(); });
            trigger.triggers.Add(pointerExitEntry);
        }
#elif UNITY_STANDALONE
        if (jumpButton != null) jumpButton.gameObject.SetActive(false);
        if (attackButton != null) attackButton.gameObject.SetActive(false);
        if (blockButton != null) blockButton.gameObject.SetActive(false);
        if (joystick != null) joystick.gameObject.SetActive(false);
#endif
    }

    private void InitializeHealth()
    {
        currentHealth = maxHealth;
        if (healthBarObject != null)
        {
            healthBar = healthBarObject.GetComponent<HealthBar>();
            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHealth);
                healthBar.SetHealth(currentHealth);
            }
            else
            {
                Debug.LogError("No se encontró el componente HealthBar en el objeto asignado.");
            }
        }
        else
        {
            Debug.LogError("No se ha asignado el objeto HealthBar en el Inspector.");
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvincible) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        if (damagedSound != null)
        {
            damagedSound.Play();
        }

        animator.SetTrigger("Damaged");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeTemporarilyInvincible());
        }
    }

    private IEnumerator BecomeTemporarilyInvincible()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private void Die()
    {
        // Implementar lógica de muerte del jugador
        Debug.Log("El jugador ha muerto");
        // Por ejemplo: desactivar el GameObject, mostrar pantalla de game over, etc.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyAttack"))
        {
            EnemyAttack enemyAttack = other.GetComponent<EnemyAttack>();
            if (enemyAttack != null)
            {
                TakeDamage(enemyAttack.damageAmount);
            }
            else
            {
                // Daño por defecto si no se encuentra el componente EnemyAttack
                TakeDamage(10f);
            }
        }
    }

    /// <summary>
    /// Maneja las colisiones con triggers.
    /// </summary>
    /// <param name="other">El collider con el que se ha producido la colisión.</param>
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     Debug.Log($"OnTriggerEnter2D: Colisión detectada con {other.gameObject.name}, tag: {other.tag}");
    //     if (other.CompareTag("NPCAttack"))
    //     {
    //         TakeDamage();
    //     }
    // }

    /// <summary>
    /// Aplica el daño al personaje.
    /// </summary>
    // private void TakeDamage()
    // {
    //     Debug.Log("TakeDamage() llamado");
    //     animator.Play("Damaged");  // Intenta reproducir la animación directamente
    //     if (damagedSound != null)
    //     {
    //         damagedSound.Play();
    //     }
    // }
}