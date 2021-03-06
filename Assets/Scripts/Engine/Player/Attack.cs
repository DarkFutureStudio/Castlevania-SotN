﻿using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private PlayerCommonAbilities m_Abilities;
    private CinemachineImpulseSource m_CameraShake;

    private void Start()
    {
        m_Abilities = GetComponentInParent<PlayerCommonAbilities>();
        m_CameraShake = GetComponentInParent<CinemachineImpulseSource>();
    }

    public void Hit()
    {
        //get everything in the attack radius and detect if it is enemy
        var enemies = Physics2D.OverlapCircleAll(
            m_Abilities.attackPosition.position,
            m_Abilities.attackRange,
            m_Abilities.m_AttackLayer);

        bool enemyFound = false;
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Interact"))
                enemy.GetComponent<Interactable>().OnInteract();
            else if (enemy.CompareTag("Enemy"))
            {
                if (enemy.isTrigger)
                    continue;
                //add force to the opposite direction of enemy
                var rigidbody = enemy.GetComponent<Rigidbody2D>();
                Vector2 direction = (enemy.transform.position - transform.position).normalized;
                rigidbody.AddForce(direction * m_Abilities.attackForce, ForceMode2D.Impulse);

                var enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                    enemyScript.TakeDamage(Player.m_Instance.CurrentDamage);

                enemyFound = true;
            }
            else if (enemy.CompareTag("Breakable"))
                enemy.GetComponentInParent<BreakableWall>().OnInteract();
        }
        if (!enemyFound)
            AudioManager.Play("attack");
        else
        {
            AudioManager.Play("attack_p");
            m_CameraShake.GenerateImpulse();
        }
    }
}
