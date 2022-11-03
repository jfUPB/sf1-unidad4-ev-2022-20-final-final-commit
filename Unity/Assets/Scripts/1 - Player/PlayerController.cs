using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Serial Coms")]
    public SerialController serialController;
    StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
    
    private float waitTime = 0.060f;
    private float time = 0.0f;
    
    public string message;
    string catcher;
    public string[] recived = new string [3];
    public string[] recivedT = new string [3];
    
    private float x, y;
    
    [Header("Movement & Jumping")]
    public float PlayerSpeed;
    public float JumpForce;
    public float JumpTime;
    public Transform feetPos;
    public float checkRad;
    public LayerMask whatIsGorund;


    [Header("Health")]
    public float godModeTime = 1.5f;
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Miscelanious")]
    public Vector2 anglesToRotate;

    bool m_FacingRight = true;
    bool godModeOn;

    private bool isGrounded;
    private float JumpTimeTimer;
    private float godModeOnTimer;
    private bool isJumping;
    private float moveInput;
    private int currentScene;

    private Rigidbody2D rb;
    public Animator animator;
    public GameObject HealthBar;
    public GameObject OverScreen;
    public GameObject Spear;

    public UnityEvent interactAction;

    void Start()
    {
        //Coms related
        serialController = GameObject.Find("SerialController").GetComponent<SerialController>();
        //Debug.Log("Control Movement");
        //Game related
        animator = GetComponent<Animator>(); 
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    void FixedUpdate()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------
        time += Time.deltaTime;

        if (time > waitTime)
        {
            time = time - waitTime;
            Debug.Log("Sending com");
            serialController.SendSerialMessage("com\n");
        }
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------
        message = serialController.ReadSerialMessage();
        Debug.Log(message);

        if(message != null)
        {
            // Debug.Log(message);
            catcher = message;
            if (stringComparer.Equals("A", catcher))
            {
                rb.velocity = Vector2.left * 10;
                Debug.Log("[A]");
            }
            if (stringComparer.Equals("D", catcher))
            {
                rb.velocity = Vector2.right * 10;
                Debug.Log("[D]");
            }
            if (stringComparer.Equals("W", catcher))
            {
                rb.velocity = Vector2.up * 2;
                Debug.Log("[W]");
            }
        }
        // moveInput = Input.GetAxisRaw("Horizontal");
        // rb.velocity = new Vector2(moveInput * PlayerSpeed, rb.velocity.y);

        // If the input is moving the player right and the player is facing left...
        if (moveInput > 0 && !m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (moveInput < 0  && m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));

        if (godModeOn)
        {
            godModeOnTimer -= Time.deltaTime;
            if (godModeOnTimer < 0)
                godModeOn = false;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            godModeOn = true;
        }

        isGrounded = Physics2D.OverlapCircle(feetPos.position, checkRad, whatIsGorund);

        if (isGrounded == true && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            JumpTimeTimer = JumpTime;
            animator.Play("Timoff_Meleeanim");
            rb.velocity = Vector2.up * JumpForce;
        }

        if (Input.GetKey(KeyCode.Space) && isJumping == true)
        {
            if(JumpTimeTimer > 0)
            {
                rb.velocity = Vector2.up * JumpForce;
                JumpTimeTimer -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }
    }

    public void ChangeHealth (int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log(currentHealth + "/" + maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);

        if (currentHealth <= 0)
        {
            death();
        }
    }

    void death()
    {
       if(currentHealth <= 0)
       {
            StartCoroutine("Respawn");
       }
    }

    IEnumerator Respawn()
    {
        Gameover();
        yield return new WaitForSeconds(3.2f);
        OverScreen.SetActive(false);
        HealthBar.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Gameover()
    {
        OverScreen.SetActive(true);
        HealthBar.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeathZone"))
        {
            death();
        }
    }

    private void Flip()
    {
        m_FacingRight = !m_FacingRight;

        transform.Rotate(0f, 180f, 0f);
    }

    /*private void Com()
    {
        //---------------------------------------------------------------------
        // Send data
        //---------------------------------------------------------------------
        Debug.Log("Sending com");
        time += Time.deltaTime;

        if (time > waitTime)
        {
            time = time - waitTime;
            serialController.SendSerialMessage("com\n");
        }
        //---------------------------------------------------------------------
        //---------------------------------------------------------------------
        // Receive data
        //---------------------------------------------------------------------
        message = serialController.ReadSerialMessage();
        Debug.Log(message);
        if (message != null)
        {
           recived = message.Split(',');
           Debug.Log(recived.Length);
           Debug.Log("com" + recived[0]);
           if (recived[0] != "" && recived[1] != "")
           {
               recivedT[0] = recived[0];
               recivedT[1] = recived[1];
               x = Math.Abs(Convert.ToSingle(recivedT[0]));
               y = Math.Abs(Convert.ToSingle(recivedT[1]));

               if (x > 5)
               {
                   Input.GetKeyDown(KeyCode.A);
               }
               if (x < -5)
               {
                   Input.GetKeyDown(KeyCode.D);
               }
               if (y > 10)
               {
                   Input.GetKeyDown(KeyCode.Space);                 
               }
           }
        }

        // Check if the message is plain data or a connect/disconnect event.
        // if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_CONNECTED))
        //     Debug.Log("Connection established");
        // else if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_DISCONNECTED))
        //     Debug.Log("Connection attempt failed or disconnection detected");
        // else
        //     Debug.Log("Message arrived: " + message);
        //---------------------------------------------------------------------
    }*/
}