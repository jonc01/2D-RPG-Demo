using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    [SerializeField] bool m_noBlood = false;
    public PlayerMovement movement;
    public CharacterController2D controller;
    public GameObject TextPopupsPrefab;
    public Transform playerLocation;

    //textPopups for referencce in PlayerMovement
    public GameObject tempShowDmg;

    public float maxHealth = 100;
    public float currentHealth;
    public HealthBar healthBar;
    public HealthBar experienceBar;
    public bool isAlive = true;

    public Transform attackPoint;
    public float attackRange = 0.46f;
    public float attackHeavyRange = 0.58f;
    public float attackDamageLight = 10f;
    public float attackDamageHeavy = 15f;
    public Collider2D[] hitEnemies;
    public bool canAttack = true;

    public float attackTime = 0.25f; //0.25 seems good, give or take .1 seconds
    //bool canMove = true;

    //Block check
    private const float minBlockDuration = 0.25f;
    private float currentBlockDuration = 0f;
    private bool blockIsHeld = false;

    public LayerMask enemyLayers;
    public LayerMask chestLayers;

    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;

    //ability cooldown
    public float dodgeCD = 1;
    private float allowDodge = 0;
    int blockCounter;

    //weapon specific
    public float knockback = 1f;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        //blockIsHeld = false;
        isAlive = true;
        movement.canMove = true;
        canAttack = true;
    }
    // Update is called once per frame
    void Update()
    {
        blockIsHeld = false; //fix this, shouldn't be in Update, write coroutine

        m_timeSinceAttack += Time.deltaTime;
        //Attack Animations
        if (Input.GetButtonDown("Fire1") && m_timeSinceAttack > 0.25f && (blockIsHeld == false) && canAttack)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            animator.SetTrigger("Attack" + m_currentAttack);

            if (m_currentAttack == 3) {
                AttackHeavy(); //can add parameter to Attack(10), for additional 10 damage on top of player damage
                StartCoroutine(IsAttacking());
            }
            else
            {
                //movement.canMove = false;
                Attack();
                StartCoroutine(IsAttacking());
                //Attack();
            }
                // Reset timer
                m_timeSinceAttack = 0.0f;
                //
        }

        //Block/Alt attack
        if (Input.GetButton("Fire2") && canAttack)
        {
            /*private const float minBlockDuration = 0.25f;
            private float currentBlockDuration = 0f;
            */
            //currentBlockDuration = Time.timeSinceLevelLoad;
            blockIsHeld = true;
            
            if(blockCounter < 100)
            {
                animator.SetTrigger("Block");
            }
            else
            {
                animator.ResetTrigger("Block");
                animator.SetBool("IdleBlock", true);
                Debug.Log("IdleBlock: " + animator.GetBool("IdleBlock"));
                
            }
            blockCounter++;
            if (blockIsHeld == true)
            {
                Debug.Log("HOLDING block");
                movement.runSpeed = 0f;
                animator.SetBool("IdleBlock", true);
            }
            /*else
            {
                Debug.Log("NOT HOLDING block");
                animator.ResetTrigger("Block");
                animator.SetBool("IdleBlock", false);
            }*/
            //Blocking(true);
        }
        if (blockIsHeld == false)
        {
            //Debug.Log("NOT HOLDING block");
            movement.runSpeed = movement.defaultRunSpeed;
            animator.ResetTrigger("Block");
            animator.SetBool("IdleBlock", false);
        }
        //else if (Input.GetButtonUp("Fire2"))
        //{
        //blockIsHeld = false;
        //Blocking(false);

        //animator.SetBool("IdleBlock", false);
        //}


        /////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////
        //testing health bar, hurt, and death animations
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RevivePlayer(1.0f); //1.0 = 100%, 0.5 = 50%
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            HealPlayer(50f); //how much health to heal
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////
    }

    IEnumerator IsAttacking()
    {
        movement.canMove = false;
        animator.SetBool("isAttacking", true);
        //Attack();
        movement.rb.velocity = new Vector2(0, 0); //stop player from moving
        //yield return new WaitForSeconds(attackTime);
        yield return new WaitForSeconds(0.3f);
        movement.canMove = true;
        animator.SetBool("isAttacking", false);
        //movement.canMove = true;
        //movement.runSpeed = movement.defaultRunSpeed;
    }

    void Attack()
    {
        //Attack range, detect enemies in range
        hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        //movement.rb.AddForce(Vector2.right * 10f);
        //damage enemies
        foreach (Collider2D enemy in hitEnemies) //loop through enemies hit
        {
            //Debug.Log("We Hit " + enemy.name);
            if(enemy.GetComponent<Enemy>() != null)
                enemy.GetComponent<Enemy>().TakeDamage(attackDamageLight); //attackDamage + additional damage from parameter
            
            if(enemy.GetComponent<StationaryEnemy>() != null)
                enemy.GetComponent<StationaryEnemy>().TakeDamage(attackDamageLight);
            
        }
    }

    void AttackHeavy()
    {
        //animator.SetTrigger("Attack3");
        hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackHeavyRange, enemyLayers);
        //Collider2D[] hitEnemies = Physics2D.OverlapAreaAll(attackPoint.position, attackHeavyRange, enemyLayers);
        //damage enemies
        Vector3 changeLocation = playerLocation.position;
        if (controller.m_FacingRight)
        {
            //go right
            changeLocation.x += .1f;
            playerLocation.position = changeLocation;
            //movement.rb.AddForce(Vector2.right * 10f);
        }
        else
        {
            //go left
            changeLocation.x -= .1f;
            playerLocation.position = changeLocation;
            //movement.rb.AddForce(Vector2.left * 10f);
        }
        foreach (Collider2D enemy in hitEnemies) //loop through enemies hit
        {
            //Debug.Log("We Hit " + enemy.name);
            if (enemy.GetComponent<Enemy>() != null)
            {
                enemy.GetComponent<Enemy>().TakeDamage(attackDamageHeavy); //attackDamage + additional damage from parameter
                enemy.GetComponent<Enemy>().GetKnockback(knockback);
            }
            
            if (enemy.GetComponent<StationaryEnemy>() != null)
                enemy.GetComponent<StationaryEnemy>().TakeDamage(attackDamageHeavy);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        Gizmos.DrawWireSphere(attackPoint.position, attackHeavyRange);
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth > 0) {
            currentHealth -= damage;
            healthBar.SetHealth(currentHealth);
            animator.SetTrigger("Hurt");
            if (TextPopupsPrefab) {
                ShowTextPopup(damage);
            }
        }

        //hurt animation
        if (currentHealth <= 0){
            Die();
        }
        
    }
    void ShowTextPopup(float damageAmount)
    {
        var showDmg = Instantiate(TextPopupsPrefab, transform.position, Quaternion.identity, transform);
        showDmg.GetComponent<TextMeshPro>().text = damageAmount.ToString();
        tempShowDmg = showDmg;

        if (controller.m_FacingRight)
        {
            FlipTextAgain(0);
        }
        else
        {
            FlipTextAgain(180);
        }

    }

    public void FlipTextAgain(float rotateAgain) //gets called in PlayerMovement to flip with player
    {
        tempShowDmg.GetComponent<TextPopups>().FlipText(rotateAgain);
    }

    public void HealPlayer(float healAmount)
    {
        if (isAlive && currentHealth > 0)
        {
            currentHealth += healAmount;
            healthBar.SetHealth(currentHealth);
            //animator.SetTrigger("Hurt");
            if (TextPopupsPrefab)
            {
                ShowTextPopupHeal(healAmount);
            }
            if(currentHealth > maxHealth)
            {
                currentHealth = maxHealth; //can't overheal, can implement an overheal/shield later
            }
        }

        //hurt animation
        if (currentHealth <= 0)
        {
            Die();
        }

    }

    void ShowTextPopupHeal(float healAmount)
    {
        var showHeal = Instantiate(TextPopupsPrefab, transform.position, Quaternion.identity, transform);
        showHeal.GetComponent<TextMeshPro>().text = healAmount.ToString();
        showHeal.GetComponent<TextMeshPro>().color = new Color32(35, 220, 0, 255);
        tempShowDmg = showHeal;

        if (controller.m_FacingRight)
        {
            FlipTextAgain(0);
        }
        else
        {
            FlipTextAgain(180);
        }

    }

    void Blocking(bool isBlocking)
    {
        if(isBlocking == true)
        {
            Debug.Log("HOLDING");
            //movement.runSpeed = 0f;
            movement.rb.velocity = new Vector2(0, 0);
            animator.SetBool("IdleBlock", true);
        }
        else
        {
            //Debug.Log("NOT HOLDING");
            //movement.runSpeed = movement.defaultRunSpeed;
            animator.ResetTrigger("Block");
            animator.SetBool("IdleBlock", false);


        }
        
    }

    void Die()
    {
        Debug.Log("Player died.");
        //Die animation
        isAlive = false;
        animator.SetBool("noBlood", m_noBlood);
        animator.SetTrigger("Death");
        movement.rb.velocity = new Vector2(0, 0); //stop player from moving
        movement.canMove = false;
        canAttack = false;
        //disable player object
        //kill player
    }

    void RevivePlayer(float spawnHpPercentage)
    {
        isAlive = true;
        movement.canMove = true;
        canAttack = true;
        currentHealth = (spawnHpPercentage * maxHealth);
        if (isAlive && currentHealth > 0)
        {
            healthBar.SetHealth(currentHealth);
            if (TextPopupsPrefab)
            {
                ShowTextPopupHeal(spawnHpPercentage);
            }
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth; //can't overheal, can implement an overheal/shield later
            }
        }
    }
}
