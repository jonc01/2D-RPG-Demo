using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    public GameObject TextPopupsPrefab;
    public Transform player;
    public LayerMask playerLayers;
    public PlayerCombat playerCombat;

    public float maxHealth = 100;
    float currentHealth;
    public HealthBar healthBar;

    public int experiencePoints = 10;
    public Animator enAnimator;
    public bool isAlive;

    [SerializeField]
    public Rigidbody2D rb;
    float aggroRange = 3f; //when to start chasing player
                        //might extend to aggro to player before enemy enters screen
    float enAttackRange = .5f; //when to start attacking player, stop enemy from clipping into player
    public Transform enAttackPoint;
    public EnemyController enController;
    [Space]
    public float enAttackDamage = 5f;
    public float enAttackSpeed = .6f; //lower value for lower delays between attacks
    public float enAttackAnimSpeed = .4f; //lower value for shorter animations

    [SerializeField]
    bool enCanMove = true;
    bool enCanAttack = true;
    bool isAttacking; //for parry
    bool playerToRight;

    void Start()
    {
        //Stats
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        isAlive = true;
        enCanMove = true;
        enCanAttack = true;
        //AI aggro
        rb = GetComponent<Rigidbody2D>();
        enAnimator.SetBool("isRunning", false);
        //enController.enAnimator.SetBool("isRunning", false);
        //enController.enFacingRight = false; //start facing left (towards player start)
        isAttacking = false;
    }

    void Update()
    {
        if(rb != null && enController != null && isAlive && playerCombat.isAlive) //check if object has rigidbody
        {
            //checking distance to player for aggro range
            float distToPlayer = Vector2.Distance(transform.position, player.position);

            //range <= 3
            if(distToPlayer <= aggroRange && enCanMove)
            {
                //chase player
                //
                enCanMove = true;
                StartChase();
                if (Mathf.Abs(transform.position.x - player.position.x) <= enAttackRange)
                {
                    StopChase();
                    //Attack
                    //StartCoroutine
                }
            }
            else
            {
                //StopChase(); //currently keep chasing forever, or if player outruns aggro range stop chase
            }
        }
        else if (!isAlive)
        {
            if (rb != null)
                rb.velocity = new Vector2(0, 0);
        }
        if (!playerCombat.isAlive)
            StopChase();
    }


    //AI aggro
    void StartChase()
    {
        enAnimator.SetBool("inCombat", true);
        enCanMove = true;
        enAnimator.SetBool("isRunning", true);
        if (transform.position.x < player.position.x) //player is right
        {
            playerToRight = true;
            //player is to right, move right
            rb.velocity = new Vector2(enController.moveSpeed, 0); //moves at moveSpeed
                                                                  //Facing right, flip sprite to face right
            enController.enFacingRight = true;
            enController.Flip();
            if (Mathf.Abs(transform.position.x - player.position.x) <= enAttackRange)
            {
                //Attack();
                //enAnimator.SetTrigger("Attack");
                StartCoroutine(IsAttacking());
            }
            //if(enCanMove)
                //enController.Flip();
        }
        else if (transform.position.x > player.position.x) //player is left
        {
            playerToRight = false;
            //player is to left, move left
            rb.velocity = new Vector2(-enController.moveSpeed, 0);

            enController.enFacingRight = false;
            //if (enCanMove)
            enController.Flip();
            if (Mathf.Abs(transform.position.x - player.position.x) <= enAttackRange)
            {
                //Attack();
                StartCoroutine(IsAttacking());
                //enAnimator.SetBool("inCombat", true);
            }
            //if(enCanMove)
               // enController.Flip();
        }
    }

    void StopChase()
    {
        rb.velocity = new Vector2(0, 0);
        enAnimator.SetBool("isRunning", false);
        //enAnimator.SetBool("inCombat", true);
        enCanMove = false;
    }

    void Attack()
    {
        Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(enAttackPoint.position, enAttackRange, playerLayers);
        
        //damage enemies
        foreach (Collider2D player in hitPlayer) //loop through enemies hit
        {
            Debug.Log("We Hit " + player.name);
            player.GetComponent<PlayerCombat>().TakeDamage(enAttackDamage); //attackDamage + additional damage from parameter
        }
    }

    IEnumerator IsAttacking()
    {
        if (enCanAttack)
        {
            //enCanMove = false;
            //rb.velocity = new Vector2(0, 0); //stop player from moving
            enAnimator.SetBool("isAttacking", true);
            enAnimator.SetTrigger("Attack");
            //enAnimator.SetBool("inCombat", true);
            enAnimator.SetBool("isRunning", false);
            //
            enCanAttack = false;
            enCanMove = false;
            rb.velocity = new Vector2(0, 0);
            yield return new WaitForSeconds(enAttackAnimSpeed); //time when damage actually registers
            
            rb.velocity = new Vector2(0, 0); //stop player from moving
            Attack();
            //enCanMove = true; //might cause enemy to slide while attacking is player moves
            yield return new WaitForSeconds(.5f);
            enCanMove = true;
            yield return new WaitForSeconds(enAttackSpeed); //cooldown between attacks
            enAnimator.SetBool("isAttacking", false);
            enCanAttack = true;
            //enCanMove = true;
            //enAnimator.SetBool("isAttacking", false);
            //enAnimator.SetTrigger("Attack");
        }
        enCanMove = true;
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
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            //show damage/heal numbers
            if (TextPopupsPrefab)
            {
                ShowTextPopup(damage);
            }
            
            //hurt animation
            if (enAnimator != null && damage > 0)
            {
                enAnimator.SetTrigger("Hurt");
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public void GetKnockback(float knockbackAmount)
    {
        if (rb != null) { 
            if (playerToRight) //player to right, knockback left
            {
                Debug.Log("KNOCKBACK PLS");
                rb.AddForce(Vector2.left * 100f);
            }
            else
            {
                Debug.Log("left");
                rb.AddForce(Vector2.right * 100f);
            }
        }

        /*var showKnockback = Instantiate(TextPopupsPrefab, transform.position, Quaternion.identity, transform);
        showKnockback.GetComponent<TextMeshPro>().text = "?!";*/
    }

    void ShowTextPopup(float damageAmount)
    {
        var showDmg = Instantiate(TextPopupsPrefab, transform.position, Quaternion.identity, transform);
        showDmg.GetComponent<TextMeshPro>().text = Mathf.Abs(damageAmount).ToString();
        if(damageAmount < 0)
            showDmg.GetComponent<TextMeshPro>().color = new Color32(35, 220, 0, 255);
        /*if (enController.enFacingRight) //player facing right by default
            showDmg.transform.Rotate(0f, 0f, 0f);*/
    }

    public void GiveExperience(int experiencePoints){
        Debug.Log("Give player " + experiencePoints + " XP");
        //give xp
        //
    }

    void Die()
    {
        Debug.Log("Enemy died.");
        //Die animation
        if(enAnimator != null)
        {
            enAnimator.SetTrigger("Death");
        }

        //give player exp
        GiveExperience(experiencePoints);

        //hide hp bar


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
