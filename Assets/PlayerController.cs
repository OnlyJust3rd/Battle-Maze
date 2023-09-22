using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 5f, buffMovementSpeed, ghostMovementSpeed;
    public bool isPlayer1;
    public bool isControlledByBot = false;

    public GameObject playerSprite;
    public GameObject normalTrail, fastTrail, ghostTrail;
    public ParticleSystem deadParticle, spawnParticle;
    public GameObject eaterLabel, iceLabel, lightOutLabel;
    public LayerMask itemLayer;

    private SuperBotAI bot;
    private Vector2 moveDir;
    private Rigidbody2D rb;
    private float currentMovementSpeed;
    private Vector3 spawnPoint;
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bot = GetComponent<SuperBotAI>();
        currentMovementSpeed = movementSpeed;
        spawnPoint = transform.position;

        playerSprite.SetActive(false);

        fastTrail.SetActive(false);
        normalTrail.SetActive(true);
    }

    private void Update()
    {
        // movement input
        if (isPlayer1)
        {
            moveDir.x = Input.GetAxisRaw("Horizontal1");
            moveDir.y = Input.GetAxisRaw("Vertical1");
        }
        else
        {
            moveDir.x = Input.GetAxisRaw("Horizontal2");
            moveDir.y = Input.GetAxisRaw("Vertical2");
        }

        if (!GameManager.instance.playable || isDead) moveDir = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!GameManager.instance.playable) return;

        // movement stuff
        if (!isControlledByBot)
        {
            rb.MovePosition(rb.position + moveDir * currentMovementSpeed * Time.fixedDeltaTime);

            if (isEater)
            {
                bot.isNeedLine = true;
                bot.target = FindTargetItem();
            }
            else bot.isNeedLine = false;
        }
        else
        {
            if (!realTarget || realTarget.CompareTag("Player") || isEater) bot.target = FindTargetItem();
            rb.MovePosition(rb.position + bot.desireDirection * currentMovementSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player") && !ghosting)
        {
            if (isEater)
            {
                isEater = false;
                GetScore(300);
                StopCoroutine(EaterSound());
                StartCoroutine(col.gameObject.GetComponent<PlayerController>().Dead());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone"))
        {
            StartCoroutine(Dead());
        }

        if (ghosting) return;

        if (other.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            FindObjectOfType<AudioManager>().Play("get coin");

            if (GameManager.instance.playable) GetScore(10);
        }

        if (other.CompareTag("Item/Fruit"))
        {
            other.GetComponent<ItemScript>().PickItem();
            FindObjectOfType<AudioManager>().Play("get item");

            if (GameManager.instance.playable) GetScore(200);
        }

        if (other.CompareTag("Item/Eye"))
        {
            other.GetComponent<ItemScript>().PickItem();
            FindObjectOfType<AudioManager>().Play("get item");
            GetScore(50);

            int whichOne = 1;
            if (isPlayer1) whichOne = 0;

            Transform lightSource = GameManager.instance.GetComponent<MapGenerator>().lights[whichOne].transform.GetChild(0).transform;
            if(lightSource.localScale.x < 20) lightSource.localScale = new Vector3(lightSource.localScale.x + 2.5f, lightSource.localScale.y + 2.5f, 1);
        }

        if (other.CompareTag("Item/GottaGoFast"))
        {
            other.GetComponent<ItemScript>().PickItem();
            FindObjectOfType<AudioManager>().Play("get item");
            GetScore(50);

            StartCoroutine(GottaGoFast());
        }

        if (other.CompareTag("Item/DefinitelyNotCopyPacman"))
        {
            other.GetComponent<ItemScript>().PickItem();
            FindObjectOfType<AudioManager>().Play("get item");
            GetScore(50);

            StartCoroutine(PacmanGhost());
        }

        if (other.CompareTag("Item/SlowBro"))
        {
            other.GetComponent<ItemScript>().PickItem();
            FindObjectOfType<AudioManager>().Play("get item");
            GetScore(50);
            StartCoroutine(ShowItem(iceLabel));

            PlayerController targetPlayer;
            if (isPlayer1) targetPlayer = GameManager.instance.players[1];
            else targetPlayer = GameManager.instance.players[0];

            StartCoroutine(targetPlayer.SlowBro());
        }

        if (other.CompareTag("Item/LightOut"))
        {
            other.GetComponent<ItemScript>().PickItem();
            FindObjectOfType<AudioManager>().Play("get item");
            GetScore(50);
            StartCoroutine(ShowItem(lightOutLabel));

            GameObject targetLight;

            if (isPlayer1) targetLight = GameManager.instance.GetComponent<MapGenerator>().lights[1].transform.GetChild(0).gameObject;
            else targetLight = GameManager.instance.GetComponent<MapGenerator>().lights[0].transform.GetChild(0).gameObject;

            StartCoroutine(LightBlinking(targetLight));
        }

        if (other.CompareTag("Item/Ghost"))
        {
            other.GetComponent<ItemScript>().PickItem();
            FindObjectOfType<AudioManager>().Play("get item");
            GetScore(50);

            StartCoroutine(Ghost());
        }
    }
    Transform realTarget;
    private Transform FindTargetItem()
    {
        Collider2D[] targetItems = Physics2D.OverlapCircleAll(transform.position, 100, itemLayer);

        if (isPlayer1) realTarget = GameManager.instance.players[1].transform;
        else realTarget = GameManager.instance.players[0].transform;

        if (isEater) return realTarget;

        float lastDistance = 0;

        print("name : " + targetItems.Length);
        foreach (Collider2D targetItem in targetItems)
        {
            float currentDistance = Vector2.Distance(transform.position, targetItem.transform.position);

            if (currentDistance > lastDistance)
            {
                lastDistance = currentDistance;
                realTarget = targetItem.transform;
            }
        }

        return realTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, 100);
    }

    private IEnumerator ShowItem(GameObject itemLabel)
    {
        itemLabel.SetActive(true);
        yield return new WaitForSeconds(2);
        itemLabel.SetActive(false);
    }
    private int eaterAudio = 0;
    private IEnumerator EaterSound()
    {
        FindObjectOfType<AudioManager>().Play("eater warn");
        yield return new WaitForSeconds(1);

        if (eaterAudio < 13 && isEater)
        {
            eaterAudio++;
            StartCoroutine(EaterSound());
        }
        else eaterAudio = 0;
    }

    private float pacmanEaterCounter = 0;
    public bool isEater = false;
    private IEnumerator PacmanGhost()
    {
        isEater = true;

        Color normalColor = playerSprite.GetComponent<SpriteRenderer>().color;
        StartCoroutine(EaterSound());

        ghosting = true;
        eaterLabel.SetActive(true);
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameManager.instance.GetComponent<MapGenerator>().maze.GetComponent<Collider2D>());
        playerSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .5f);
        currentMovementSpeed = ghostMovementSpeed;
        fastTrail.SetActive(false);
        normalTrail.SetActive(false);
        ghostTrail.SetActive(true);

        yield return new WaitForSeconds(2.5f);

        ghosting = false;
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameManager.instance.GetComponent<MapGenerator>().maze.GetComponent<Collider2D>(), false);
        playerSprite.GetComponent<SpriteRenderer>().color = normalColor;
        currentMovementSpeed = movementSpeed;
        fastTrail.SetActive(false);
        normalTrail.SetActive(true);
        ghostTrail.SetActive(false);

        StartCoroutine(PacmanEater());
    }

    private IEnumerator PacmanEater()
    {
        normalTrail.SetActive(false);
        fastTrail.SetActive(true);
        Collider2D col;
        if (isPlayer1) col = GameManager.instance.players[1].GetComponent<Collider2D>();
        else col = GameManager.instance.players[0].GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col, false);
        currentMovementSpeed = buffMovementSpeed;

        yield return new WaitForSeconds(.0000000000001f);

        if (pacmanEaterCounter < 10 && isEater)
        {
            pacmanEaterCounter += Time.deltaTime;
            StartCoroutine(PacmanEater());
        }
        else
        {
            pacmanEaterCounter = 0;
            eaterLabel.SetActive(false);
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col);
            isEater = false;

            normalTrail.SetActive(true);
            fastTrail.SetActive(false);
            currentMovementSpeed = movementSpeed;
        }
    }

    private int slowBroCounter = 10;
    public IEnumerator SlowBro()
    {
        float multiplyer = slowBroCounter / 11.0f;
        currentMovementSpeed = ghostMovementSpeed * multiplyer;
        //print(currentMovementSpeed);

        yield return new WaitForSeconds(1.5f);

        if (slowBroCounter > 0)
        {
            slowBroCounter--;
            StartCoroutine(SlowBro());
        }
        else
        {
            slowBroCounter = 10;
            currentMovementSpeed = movementSpeed;
        }
    }

    private int blinkCounter = 5;
    private IEnumerator LightBlinking(GameObject targetLight)
    {
        targetLight.SetActive(false);
        yield return new WaitForSeconds(1);
        targetLight.SetActive(true);
        yield return new WaitForSeconds(.1f);
        targetLight.SetActive(false);
        yield return new WaitForSeconds(1);
        targetLight.SetActive(true);
        yield return new WaitForSeconds(.1f);
        targetLight.SetActive(false);
        yield return new WaitForSeconds(1);
        targetLight.SetActive(true);
        
        yield return new WaitForSeconds(1);

        if (blinkCounter > 0)
        {
            blinkCounter--;
            StartCoroutine(LightBlinking(targetLight));
        }
        else blinkCounter = 5;
    }

    private bool ghosting = false;
    private IEnumerator Ghost()
    {
        Color normalColor = playerSprite.GetComponent<SpriteRenderer>().color;

        ghosting = true;
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameManager.instance.GetComponent<MapGenerator>().maze.GetComponent<Collider2D>());
        playerSprite.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .5f);
        currentMovementSpeed = ghostMovementSpeed;
        fastTrail.SetActive(false);
        normalTrail.SetActive(false);
        ghostTrail.SetActive(true);

        yield return new WaitForSeconds(5);

        ghosting = false;
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameManager.instance.GetComponent<MapGenerator>().maze.GetComponent<Collider2D>(), false);
        playerSprite.GetComponent<SpriteRenderer>().color = normalColor;
        currentMovementSpeed = movementSpeed;
        fastTrail.SetActive(false);
        normalTrail.SetActive(true);
        ghostTrail.SetActive(false);
    }

    private bool isGottaGoFast = false;
    private IEnumerator GottaGoFast()
    {
        GetScore(50);

        isGottaGoFast = true;
        normalTrail.SetActive(false);
        fastTrail.SetActive(true);
        currentMovementSpeed = buffMovementSpeed;

        yield return new WaitForSeconds(20);

        isGottaGoFast = false;
        normalTrail.SetActive(true);
        fastTrail.SetActive(false);
        currentMovementSpeed = movementSpeed;
    }

    public IEnumerator Dead()
    {
        FindObjectOfType<AudioManager>().Play("die");
        GetScore(-200);
        isDead = true;
        deadParticle.Play();
        currentMovementSpeed = movementSpeed;

        playerSprite.SetActive(false);
        normalTrail.SetActive(false);
        fastTrail.SetActive(false);

        yield return new WaitForSeconds(3f);

        spawnParticle.Play();
        transform.position = spawnPoint;

        yield return new WaitForSeconds(1f);

        playerSprite.SetActive(true);
        normalTrail.SetActive(!isGottaGoFast);
        fastTrail.SetActive(isGottaGoFast);

        isDead = false;
    }

    public IEnumerator SpawnEffect()
    {
        spawnParticle.Play();
        FindObjectOfType<AudioManager>().Play("spawn");

        yield return new WaitForSeconds(1);

        playerSprite.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        playerSprite.SetActive(true);
        normalTrail.SetActive(!isGottaGoFast);
        fastTrail.SetActive(isGottaGoFast);
    }

    private void GetScore(int score)
    {
        if (!GameManager.instance.playable) return;
        // GREEN
        if (isPlayer1)
        {
            GameManager.instance.greenScore += score;
            GameManager.instance.greenScoreLabel.GetComponent<Animator>().SetTrigger("GetScore");
        }
        // RED
        else
        {
            GameManager.instance.redScore += score;
            GameManager.instance.redScoreLabel.GetComponent<Animator>().SetTrigger("GetScore");
        }
    }
}
