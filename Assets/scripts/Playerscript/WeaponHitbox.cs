using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    public int damage = 20;
    private bool canDamage = false;

    public void EnableDamage() => canDamage = true;
    public void DisableDamage() => canDamage = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Enemy"))
        {
            normalmobs enemy = other.GetComponent<normalmobs>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
