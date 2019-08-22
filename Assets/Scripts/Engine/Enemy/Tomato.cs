﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tomato : MonoBehaviour
{
    public float moveSpeed;
    public int health;
    public float collisionForce = 4;

    [Space(10)]
    public Transform detectionPos;
    public float detectionRadius;

    private Rigidbody2D m_Rigidbody;
    private bool m_IsPlayerFound, m_FacingRight = false;
    private Transform m_PlayerTransform;
    private Animator m_Animator;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (!m_IsPlayerFound)
            return;

        Vector2 direction = GetDirection();
        //detect face direction
        if (direction.x > 0 && !m_FacingRight)
            Flip();
        else if (direction.x < 0 && m_FacingRight)
            Flip();

        var desireMove = Vector2.right * direction * moveSpeed * Time.deltaTime;
        m_Rigidbody.MovePosition((Vector2)transform.position + desireMove);
    }
    void Flip()
    {
        m_FacingRight = !m_FacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    void DetectPlayer()
    {
        Collider2D[] results = new Collider2D[2];

        var overlapNum = Physics2D.OverlapCircleNonAlloc
            (detectionPos.position, detectionRadius, results);
        if (overlapNum > 0)
            foreach (var colliders in results)
            {
                if (colliders == null)
                    continue;
                else if (colliders.tag == "Player")
                {
                    m_IsPlayerFound = m_Rigidbody.simulated = true;
                    m_PlayerTransform = colliders.transform;
                }
            }
    }
    Vector2 GetDirection()
    {
        var deltaPosition = m_PlayerTransform.position - transform.position;
        return deltaPosition.normalized;
    } //a vector from player to us
    private void Update()
    {
        if (!m_IsPlayerFound) //until player enter check for it
            DetectPlayer();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //applly force to the player in opposite direction of player sees enemy
        if (collision.gameObject.CompareTag("Player"))
        {
            var animator = collision.gameObject.GetComponent<Animator>();
            animator.SetTrigger("Hit");

            m_Animator.SetTrigger("AttackEnemy");

            var rigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
            rigidbody.AddForce(GetDirection() * collisionForce, ForceMode2D.Impulse);
        }
    } //if hit player what happens?
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(detectionPos.position, detectionRadius);
    }
}
