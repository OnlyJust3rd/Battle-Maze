using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public float timeLimit;
    public int greenScore, redScore;
    public PlayerController[] players;

    [Header("UI")]
    public Text timerLabel;
    public Text greenScoreLabel;
    public Text redScoreLabel;

    public Text winLabel;
    public GameObject resultLabel;

    [HideInInspector]
    public bool isGeneratingMap;

    [HideInInspector]
    public bool playable = false;

    private float currentTime;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        currentTime = timeLimit;
        Time.timeScale = 1;
        Physics2D.IgnoreCollision(players[0].GetComponent<Collider2D>(), players[1].GetComponent<Collider2D>());
        FindObjectOfType<AudioManager>().Play("bgm");

        StartCoroutine(CoinBlink());
    }

    private bool playSoundOnce = false;
    private bool playTimeAttackOnce = false;

    private void Update()
    {
        if (currentTime >= 0 && !isGeneratingMap)
        {
            playable = true;

            greenScoreLabel.text = greenScore.ToString();
            redScoreLabel.text = redScore.ToString();
            if (currentTime < 30)
            {
                timerLabel.GetComponent<Animator>().enabled = true;
                if (!playTimeAttackOnce)
                {
                    playTimeAttackOnce = true;
                    StartCoroutine(TimeAttack());
                }
            }
            Timer();
        }
        else
        {
            if (GetComponent<MapGenerator>().isMenu) currentTime = timeLimit;
            playable = false;

            if (currentTime >= 0) return;

            timerLabel.GetComponent<Animator>().enabled = false;
            resultLabel.SetActive(true);

            FindObjectOfType<AudioManager>().Stop("bgm");
            if (!playSoundOnce)
            {
                playSoundOnce = true;
                FindObjectOfType<AudioManager>().Play("win");
            }

            if (greenScore > redScore)
            {
                winLabel.text = "GREEN WINS";
                winLabel.color = Color.green;
            }
            else if (redScore > greenScore)
            {
                winLabel.text = "RED WINS";
                winLabel.color = Color.red;
            }
            else winLabel.text = "DRAW!";

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // print("esc");
                LoadScene("MainMenu");
            }
        }

        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    GetComponent<MapGenerator>().SpawnItem(1);
        //}
    }

    private void Timer()
    {
        int min = Mathf.FloorToInt(currentTime / 60);
        int sec = Mathf.FloorToInt(currentTime % 60);
        string minMsg = "";
        string secMsg = "";

        if (min < 10) minMsg = "0" + min;
        else minMsg = min.ToString();
        if (sec < 10) secMsg = "0" + sec;
        else secMsg = sec.ToString();

        timerLabel.text = minMsg + ":" + secMsg;

        currentTime -= Time.deltaTime;
    }
    int timeAttackCounter = 30;
    private IEnumerator TimeAttack()
    {
        FindObjectOfType<AudioManager>().Play("time attack");
        yield return new WaitForSeconds(1);
        if (timeAttackCounter > 0)
        {
            timeAttackCounter--;
            StartCoroutine(TimeAttack());
        }
        else timeAttackCounter = 30;
    }

    private IEnumerator CoinBlink()
    {
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject coin in coins) coin.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(10);

        coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject coin in coins) coin.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(3);

        StartCoroutine(CoinBlink());
    }

    // Main Menu Stuff
    public void LoadScene(string name)
    {
        FindObjectOfType<AudioManager>().Play("button");
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }

    public void QuitGame()
    {
        FindObjectOfType<AudioManager>().Play("button");
        Application.Quit();
    }
}
