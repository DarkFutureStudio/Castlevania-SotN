﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class Player : Characters
{
    public float jumpSaveTime;
    public float jumpForce;
    [Space(10)]
    public float attackForce;
    public float timeBetweenAttack;
    public Transform attackPosition;
    public float attackRange;
    [Space(10)]
    public float dashForce;
    public float timeBetweenDash;

    private float m_HorizontalInput;
    private float m_JumpSaveTime;
    private bool m_Grounded, m_IsJumping, m_InterruptJumping;
    private float m_TimeBtwAttack, m_TimeBtwDash;
    private bool m_Attack, m_Dash, m_DashLock;
    private Collider2D[] m_GroundColliders;
    private float m_GravityScale;

    int m_AttackID, m_SpeedID, m_IsGroundID;

    private void Update()
    {
        m_JumpSaveTime -= Time.deltaTime;
        m_TimeBtwAttack -= Time.deltaTime;
        m_TimeBtwDash -= Time.deltaTime;

        m_HorizontalInput = Input.GetAxisRaw("Horizontal");
        Flip(m_HorizontalInput);

        InputDetection();

        m_Animator.SetFloat(m_SpeedID, Mathf.Abs(m_HorizontalInput));
    }
    private void InputDetection()
    {
        //jump input determiner
        if (Input.GetButtonDown("Jump"))
            m_JumpSaveTime = jumpSaveTime;
        //we don't want to apply force when we are falling or we are not jumping ofcourse!
        else if (Input.GetButtonUp("Jump"))
            if (m_Rigidbody.velocity.y > 0 && m_IsJumping)
                m_InterruptJumping = true;

        if (Input.GetButtonDown("Fire2") && m_TimeBtwAttack < 0)
        {
            m_TimeBtwAttack = timeBetweenAttack;
            m_Attack = true;
        }
        else if (Input.GetButtonDown("Fire1") && m_TimeBtwDash < 0)
        {
            m_Dash = true;
            m_TimeBtwDash = timeBetweenDash;
        }
    }
    private void JumpStatus()
    {
        if (m_IsJumping && m_Grounded)
            m_IsJumping = false;

        m_Animator.SetFloat("vSpeed", m_Rigidbody.velocity.y);

        if (m_JumpSaveTime > 0 && m_Grounded)
        {
            m_Rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            m_IsJumping = true;
        }
        else if (m_InterruptJumping)
        {
            m_Rigidbody.AddForce(Vector2.up * -m_Rigidbody.velocity.y, ForceMode2D.Impulse);
            m_InterruptJumping = m_IsJumping = false;
        }
    }
    private void Attack()
    {
        m_Attack = false;
        m_Animator.SetTrigger(m_AttackID);

        //get everything in the attack radius and detect if it is enemy
        var enemies = Physics2D.OverlapCircleAll(
            attackPosition.position, attackRange, m_NotGroundLayer);
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Interact"))
                enemy.GetComponent<Interactable>().OnInteract();
            else if (enemy.CompareTag("Enemy"))
            {
                //add force to the opposite direction of enemy
                var rigidbody = enemy.GetComponent<Rigidbody2D>();
                Vector2 direction = (enemy.transform.position - transform.position).normalized;
                rigidbody.AddForce(direction * attackForce, ForceMode2D.Impulse);

                //apply damage to enemy
                var enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                    enemyScript.TakeDamage();
            }
        }
    }
    private void Dash()
    {
        m_Dash = false;

        m_Rigidbody.velocity = Vector2.zero;
        m_Rigidbody.gravityScale = 0;
        m_DashLock = true;

        var forceDir = Vector2.right * dashForce;
        if (!m_FacingRight)
            forceDir = -forceDir;

        m_Rigidbody.AddForce(forceDir, ForceMode2D.Impulse);
    }
    private Vector2 MoveDirection()
    {
        /* we move in the x axis if we are not on the ground
         * and return 'right' vector of the ground object if
         * we are on it
         */
        var direction = Vector2.right;
        if (!m_Grounded)
            return direction;

        foreach (var groundCollider in m_GroundColliders)
        {
            if (groundCollider == null)
                continue;

            direction = groundCollider.transform.right;
        }

        return direction;
    }

    public override void Start()
    {
        base.Start();

        //override this cause animator component is different than base class thought
        m_Animator = GetComponentInChildren<Animator>();

        //Animator parameter Ids
        m_AttackID = Animator.StringToHash("Attack");
        m_SpeedID = Animator.StringToHash("Speed");
        m_IsGroundID = Animator.StringToHash("isGround");

        m_GravityScale = m_Rigidbody.gravityScale;
    }
    public override void FixedUpdate()
    {
        m_Grounded = CheckGround(out m_GroundColliders);

        if (m_Grounded && m_Rigidbody.gravityScale != 0)
            m_Rigidbody.gravityScale = 0;
        else if (!m_Grounded && !m_DashLock)
            m_Rigidbody.gravityScale = m_GravityScale;

        m_Animator.SetBool(m_IsGroundID, m_Grounded);

        if (m_Attack)
            Attack();
        else if (m_Dash)
        {
            Dash();
            return;
        }

        if (m_DashLock)
        {
            if (Mathf.Abs(m_Rigidbody.velocity.x) <= 6)
            {
                m_DashLock = false;
                m_Rigidbody.AddForce(Vector2.right * -m_Rigidbody.velocity.x);
            }
            return;
        }

        //anything about jumping stuff
        JumpStatus();

        base.FixedUpdate();
    }
    public override void Move()
    {
        //debuger...
        Debug.DrawRay(transform.position, m_Rigidbody.velocity, Color.cyan);

        Vector2 movement = MoveDirection() * m_HorizontalInput * moveSpeed;
        m_Rigidbody.AddForce(movement);
    }
    public override void TakeDamage()
    {
        health -= 1;
        if (health <= 0)
            SceneManager.LoadScene(0);
    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackPosition.position, attackRange);
    }
}
