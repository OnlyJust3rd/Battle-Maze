using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    //  69 x 36
    // NODE 23 x 12
    // -35 / 34
    // -18 / 18
    public Tilemap maze, mazeWall;
    public Tile wall, yellow;
    public GameObject[] lights;
    public GameObject coin;
    public GameObject[] items;
    public Transform itemsParent;
    public Text timerLabel;

    public bool isMenu;
    public Text[] titleGame;

    private void Start()
    {
        GenerateWall();
        StartCoroutine(GenerateMaze());
        // StartCoroutine(GamerMode());
    }

    private void Update()
    {
        //GameObject[] coin = GameObject.FindGameObjectsWithTag("Coin");

        //if (coin.Length <= 0 && !GameManager.instance.isGeneratingMap)
        //{
        //    foreach (Transform item in itemsParent)
        //    {
        //        print(item.name);
        //        item.GetComponent<ItemScript>().shouldIDestroyThis = true;
        //        Destroy(item.gameObject);
        //    }
        //    GenerateWall();
        //    StartCoroutine(GenerateMaze());
        //}
        GamerMode();
    }

    public Color[] rainbow;
    private int gamemodeColor;
    private float t = 1;
    private float colorTimer;

    private void GamerMode()
    {
        if (gamemodeColor > 6) gamemodeColor = 0;

        Color currentColor = rainbow[gamemodeColor];
        Color nextColor;

        if (gamemodeColor == rainbow.Length - 1) nextColor = rainbow[0];
        else nextColor = rainbow[gamemodeColor + 1];

        maze.color = Color.Lerp(currentColor, nextColor, Mathf.PingPong(Time.time, 1));
        timerLabel.color = Color.Lerp(currentColor, nextColor, Mathf.PingPong(Time.time, 1));
        if (isMenu)
        {
            foreach (var title in titleGame)
            {
                title.color = Color.Lerp(currentColor, nextColor, Mathf.PingPong(Time.time, 1));
            }

        }

        if (colorTimer > t)
        {
            colorTimer = 0;
            gamemodeColor++;
        }
        else colorTimer += Time.deltaTime;
    }

    private void GenerateWall()
    {
        // fill every node with wall
        for (int j = 0; j < 36; j++)
        {
            for (int i = 0; i < 70; i++)
            {
                if (i % 3 != 0 && j % 3 != 0) continue;

                Vector3Int posi = new Vector3Int(i - 35, j - 18, 0);
                maze.SetTile(posi, wall);
            }
        }
    }

    private Vector2Int currentNode;

    private IEnumerator GenerateMaze()
    {
        maze.GetComponent<TilemapRenderer>().maskInteraction = SpriteMaskInteraction.None;
        foreach (GameObject light in lights)
        {
            light.SetActive(false);
        }

        GameManager.instance.isGeneratingMap = true;
        currentNode = Vector2Int.zero;
        bool[,] visitNode = new bool[23, 12];
        Vector2Int desireNode = currentNode;

        List<Vector2Int> path = new List<Vector2Int>();
        int backTracking = 1;
        bool isBackTracking = false;

        int testLimiter = 10000;

        while (testLimiter > 0)
        {
            visitNode[currentNode.x, currentNode.y] = true;

            // check which way is avaliable
            List<int> waysAvaliable = new List<int> { 0, 1, 2, 3 };

            print(currentNode);

            // check up
            if (currentNode.y <= 0) waysAvaliable.Remove(0);
            else if (visitNode[currentNode.x, currentNode.y - 1]) waysAvaliable.Remove(0);
            // check right
            if (currentNode.x >= 22) waysAvaliable.Remove(1);
            else if (visitNode[currentNode.x + 1, currentNode.y]) waysAvaliable.Remove(1);
            // check down
            if (currentNode.y >= 11) waysAvaliable.Remove(2);
            else if (visitNode[currentNode.x, currentNode.y + 1]) waysAvaliable.Remove(2);
            // check left
            if (currentNode.x <= 0) waysAvaliable.Remove(3);
            else if (visitNode[currentNode.x - 1, currentNode.y]) waysAvaliable.Remove(3);

            if (waysAvaliable.Count <= 0)
            {
                // mazeWall.SetTile(NodeToTilemap(currentNode), yellow);

                print("======= dead end =======");
                if (path.Count - backTracking <= 0) break;

                isBackTracking = true;
                backTracking++;
                currentNode = path[path.Count - backTracking];
                continue;
            }

            if (isBackTracking)
            {
                isBackTracking = false;
                backTracking = 1;
                // path.Clear();
            }

            int whichWayToGo = waysAvaliable[Random.Range(0, waysAvaliable.Count)];

            if (whichWayToGo == 0) desireNode = currentNode - Vector2Int.up;
            if (whichWayToGo == 1) desireNode = currentNode + Vector2Int.right;
            if (whichWayToGo == 2) desireNode = currentNode + Vector2Int.up;
            if (whichWayToGo == 3) desireNode = currentNode - Vector2Int.right;

            // break wall
            BreakWall(currentNode, whichWayToGo);
            maze.SetTile(NodeToTilemap(currentNode), null);

            path.Add(currentNode);
            currentNode = desireNode;

            FindObjectOfType<AudioManager>().Play("generate wall");

            testLimiter--;
            yield return new WaitForSeconds(.000001f);
        }

        // spawn fruit
        int spawnToken = 100;
        int totalSpawn = 0;
        List<Vector2Int> nodeTaken = new List<Vector2Int>();

        while(spawnToken > 0)
        {
            int x = Random.Range(0, 23);
            int y = Random.Range(0, 12);

            if (x == 0 && y == 0) continue;
            if (x == 22 && y == 0) continue;

            Vector2Int node = new Vector2Int(x, y);
            Vector3 desirePos = maze.CellToWorld(NodeToTilemap(node));
            desirePos += Vector3.right;
            GameObject newFruit = Instantiate(items[0], desirePos, Quaternion.identity);

            newFruit.transform.SetParent(itemsParent);
            yield return new WaitForSeconds(.0000000000001f);

            nodeTaken.Add(node);

            if (totalSpawn >= 2) break;
            else totalSpawn++;

            FindObjectOfType<AudioManager>().Play("generate coin");

            spawnToken--;
        }

        AstarPath.active.Scan();

        // spawn item
        for (int j = 0; j < 12; j++)
        {
            for (int i = 0; i < 23; i++)
            {
                bool isThisNodeTaken = false;
                foreach (Vector2Int badNode in nodeTaken) if (badNode.x == i && badNode.y == j) isThisNodeTaken = true;

                if (isThisNodeTaken) continue;

                Vector2Int node = new Vector2Int(i, j);
                Vector3 desirePos = maze.CellToWorld(NodeToTilemap(node));
                desirePos += Vector3.right;
                GameObject new_item = Instantiate(coin, desirePos, Quaternion.identity);

                FindObjectOfType<AudioManager>().Play("generate coin");

                new_item.transform.SetParent(itemsParent);

                yield return new WaitForSeconds(.0000000000001f);
            }
        }

        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject coin in coins) coin.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        GameObject[] fruits = GameObject.FindGameObjectsWithTag("Item/Fruit");
        foreach (GameObject fruit in fruits)
        {
            fruit.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            fruit.transform.GetChild(0).GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        foreach (GameObject light in lights) light.SetActive(true);
        maze.GetComponent<TilemapRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

        foreach (PlayerController player in GameManager.instance.players)
        {
            StartCoroutine(player.SpawnEffect());
            // player.isControlledByBot = true;
        }

        Invoke("DoneGenerating", 2.5f);
        print("MAP GENERATION DONE");
    }

    private void DoneGenerating()
    {
        GameManager.instance.isGeneratingMap = false;
    }

    public void SpawnItem(int n)
    {
        int spawnToken = 100;
        int totalSpawn = 0;

        while (spawnToken > 0)
        {
            spawnToken--;

            int x = Random.Range(0, 23);
            int y = Random.Range(0, 12);

            Vector2Int node = new Vector2Int(x, y);
            Vector3 desirePos = maze.CellToWorld(NodeToTilemap(node));
            desirePos += Vector3.right;

            Collider2D[] itemsInNode = Physics2D.OverlapCircleAll(desirePos, .5f);
            if(itemsInNode.Length > 0)
            {
                foreach (Collider2D item in itemsInNode)
                {
                    if(item.CompareTag("Item/Fruit") || item.CompareTag("Coin")) Destroy(item.gameObject);
                }
            }

            int randItem = Random.Range(0, items.Length);
            GameObject newFruit = Instantiate(items[randItem], desirePos, Quaternion.identity);
            newFruit.transform.SetParent(itemsParent);

            newFruit.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            newFruit.transform.GetChild(0).GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            if (totalSpawn >= n-1) break;
            else totalSpawn++;
        }
    }

    private void BreakWall(Vector2Int node, int dir)
    {
        Vector3Int originTile = NodeToTilemap(node);

        Vector3Int[] wallToBreak = new Vector3Int[2];

        switch (dir)
        {
            case 0:
                wallToBreak[0] = new Vector3Int(originTile.x, originTile.y + 1, 0);
                wallToBreak[1] = new Vector3Int(originTile.x + 1, originTile.y + 1, 0);
                break;
            case 1:
                wallToBreak[0] = new Vector3Int(originTile.x + 2, originTile.y, 0);
                wallToBreak[1] = new Vector3Int(originTile.x + 2, originTile.y - 1, 0);
                break;
            case 2:
                wallToBreak[0] = new Vector3Int(originTile.x, originTile.y - 2, 0);
                wallToBreak[1] = new Vector3Int(originTile.x + 1, originTile.y - 2, 0);
                break;
            case 3:
                wallToBreak[0] = new Vector3Int(originTile.x - 1, originTile.y, 0);
                wallToBreak[1] = new Vector3Int(originTile.x - 1, originTile.y - 1, 0);
                break;
        }

        foreach(Vector3Int theWall in wallToBreak)
        {
            maze.SetTile(theWall, null);
        }
    }

    private Vector3Int NodeToTilemap(Vector2Int node)
    {
        int x = node.x * 3 - 34;
        int y = node.y * -3 + 17;

        return new Vector3Int(x, y, 0);
    }
}
