﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public int worthiness = 50;
    public float lifeTime = 3;
    [Space(10)]
    public float distanceUnder;
    [Space(10)]
    public Identity whatIsThisShit;

    public enum Identity { coin, heart };

    private float m_Timer;
    private Rigidbody2D m_Rigidbody;
    private const int m_GroundLayer = ~(1 << 8);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
            if (whatIsThisShit == Identity.coin)
                GameManager.Coin += worthiness;
            else if (whatIsThisShit == Identity.heart)
                GameManager.Hearts += worthiness;
        }
    }

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer >= lifeTime)
            Destroy(gameObject);
    }
    private void FixedUpdate()
    {
        RaycastHit2D[] results = new RaycastHit2D[2];

        int ray = Physics2D.RaycastNonAlloc(
            transform.position, Vector2.down, results, distanceUnder, m_GroundLayer);

        if (ray > 1)
            m_Rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Vector2.down * distanceUnder);
    }
#endif
}
