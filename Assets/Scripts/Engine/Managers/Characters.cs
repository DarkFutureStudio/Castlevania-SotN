﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Characters : MonoBehaviour
{
    public float health;
    public float moveSpeed;
    [Space(10)]
    public Transform groundCheck;
    public float groundRadius;
    [Space(10)]
    public bool m_FacingRight;
    [Space(10)]
    public float attackDamage;

    protected Rigidbody2D m_Rigidbody;
    protected Animator m_Animator;
    protected const int m_GroundLayer = ~(1 << 8 | 1 << 10 | 1 << 11);

    public virtual void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
    }
    public virtual void FixedUpdate() => Move();
    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }

    protected void Flip(float horizontal)
    {
        if ((horizontal > 0 && m_FacingRight) || 
            (horizontal < 0 && !m_FacingRight) || (horizontal == 0))
            return;

        m_FacingRight = !m_FacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    private bool CheckArea(
        Vector3 checkPosition, float radius, LayerMask layer, out Collider2D[] target)
    {
        Collider2D[] results = new Collider2D[2];
        int colliders = Physics2D.OverlapCircleNonAlloc
            (checkPosition, radius, results, layer);

        target = results;
        return colliders > 0;
    }
    protected bool CheckGround(out Collider2D[] ground) =>
            CheckArea(groundCheck.position, groundRadius, m_GroundLayer, out ground);

    public abstract void Move();
    public abstract void TakeDamage(float damage);
}
