﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StationaryEnemy : MonoBehaviour
{
    //this is for support stationary enemies, can make non-support stationary separate
    //this script only targets enemies that are "friendly" to this object, and heals them
    public GameObject TextPopupsPrefab;
    public Transform player;
    public LayerMask playerLayers;
    public Transform enemyAllies;
    public LayerMask enemyLayers;

    public float maxHealth = 100;
    float currentHealth;
    public HealthBar healthBar;

    int experiencePoints = 10;
    public Animator enAnimator;
    public bool isAlive;

    [SerializeField]
    public Rigidbody2D rb;
    float aggroRange = 3f; //when to start chasing player //might extend to aggro to player before enemy enters screen
    float enAttackRange = 1.5f; //when to start attacking player, stop enemy from clipping into player
    public Transform enAttackPoint;
    public EnemyController enController;

    [Space]
    public float enAttackDamage = 10f; //can just heal off of this value
    public float enAttackSpeed = .4f; //lower value for lower delays between attacks
    public float enAttackAnimSpeed = .7f; //lower value for shorter animations
    bool enCanAttack = true;

    void Start()
    {
        //Stats
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        isAlive = true;
        enCanAttack = true;
        //AI aggro
        /*if(rb != null)
            rb = GetComponent<Rigidbody2D>();*/
    }

    void Update()
    {
        if (rb != null && enController != null && isAlive && enemyAllies != null) //check if object has rigidbody
        {
            //checking distance to player for aggro range
            float distToPlayer = Vector2.Distance(transform.position, enemyAllies.position);

            //range <= 3
            if (distToPlayer <= aggroRange)
            {
                Debug.Log("Stationary enemy attacking...");
                StartCoroutine(IsAttacking());
                //enCanAttack = true;
            }
            else
            {
                //StopChase();
            }
        }
        else if (!isAlive)
        {
            //moveSpeed = 0;
            if (rb != null)
                rb.velocity = new Vector2(0, 0);
        }

    }


    void Attack() //for Stationary, can be buffing abilities, ex: totem
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(enAttackPoint.position, enAttackRange, enemyLayers);
        //                                                                                  targetting enemy allies only
        //damage enemies
        foreach (Collider2D enemy in hitEnemies) //loop through enemies hit
        {
            Debug.Log("We are healing " + enemy.name);
            if(enemy != null && enemy.GetComponent<Enemy>() != null)
                enemy.GetComponent<Enemy>().TakeDamage(-enAttackDamage); //negative damage for healing don't need Heal() function
        }
    }

    IEnumerator IsAttacking()
    {
        if (enCanAttack)
        {
            //rb.velocity = new Vector2(0, 0); //stop moving
            /*enAnimator.SetBool("isAttacking", true);
            enAnimator.SetTrigger("Attack");
            //enAnimator.SetBool("inCombat", true);
            enAnimator.SetBool("isRunning", false);*/
            //
            enCanAttack = false;
            yield return new WaitForSeconds(enAttackAnimSpeed); //time when damage actually registers

            Attack();
            //enCanMove = true; //might cause enemy to slide while attacking is player moves
            //yield return new WaitForSeconds(.5f);
            yield return new WaitForSeconds(enAttackSpeed); //cooldown between attacks
            //enAnimator.SetBool("isAttacking", false);
            enCanAttack = true;
            //enAnimator.SetBool("isAttacking", false);
            //enAnimator.SetTrigger("Attack");
        }
        //enCanAttack = false;
        //enCanAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (enAttackPoint == null)
            return;

        Gizmos.DrawWireSphere(enAttackPoint.position, enAttackRange);
    }

    public void TakeDamage(float damage)
    {
        if (isAlive == true)
        {
            currentHealth -= damage;
            healthBar.SetHealth(currentHealth);
            //show damage/heal numbers
            if (TextPopupsPrefab)
            {
                ShowTextPopup(damage);
            }
            
            //hurt animation
            if (enAnimator != null)
            {
                enAnimator.SetTrigger("Hurt");
            }
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    void ShowTextPopup(float damageAmount)
    {
        var showDmg = Instantiate(TextPopupsPrefab, transform.position, Quaternion.identity, transform);
        showDmg.GetComponent<TextMeshPro>().text = damageAmount.ToString();

        /*if (enController.enFacingRight) //player facing right by default
            showDmg.transform.Rotate(0f, 0f, 0f);*/

    }

    public void GiveExperience(int experiencePoints)
    {
        Debug.Log("Give player " + experiencePoints + " XP");
        //give xp
        //
    }

    void Die()
    {
        //Die animation
        if (enAnimator != null)
        {
            enAnimator.SetTrigger("Death");
        }

        //give player exp
        GiveExperience(experiencePoints);

        //hide hp bar
        //GetComponent

        //disable enemy object
        isAlive = false;

        StartCoroutine(DeleteEnemyObject());
    }

    IEnumerator DeleteEnemyObject()
    {
        yield return new WaitForSeconds(3f);
        Destroy(this.gameObject);
    }
}
