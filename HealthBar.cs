using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la barra de salud del personaje en la interfaz de usuario.
/// </summary>
public class HealthBar : MonoBehaviour
{
    /// <summary>
    /// Referencia a la imagen que representa el relleno de la barra de salud.
    /// </summary>
    public Image fillImage;

    /// <summary>
    /// Valor máximo de salud del personaje.
    /// </summary>
    private float maxHealth;

    /// <summary>
    /// Valor actual de salud del personaje.
    /// </summary>
    private float currentHealth;

    /// <summary>
    /// Establece el valor máximo de salud y actualiza la barra de salud.
    /// </summary>
    /// <param name="health">El valor máximo de salud a establecer.</param>
    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    /// <summary>
    /// Actualiza el valor actual de salud y refleja el cambio en la barra de salud.
    /// </summary>
    /// <param name="health">El nuevo valor de salud a establecer.</param>
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        UpdateHealthBar();
    }

    /// <summary>
    /// Actualiza visualmente la barra de salud basándose en la salud actual y máxima.
    /// </summary>
    private void UpdateHealthBar()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = currentHealth / maxHealth;
        }
    }
}