using UnityEngine;

/// <summary>
/// Hasar alabilecek nesneler için arayüz.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Hasar alma fonksiyonu.
    /// </summary>
    /// <param name="damage">Alınan hasar miktarı.</param>
    void TakeDamage(float damage);
} 