using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public void PickItem()
    {
        GameManager.instance.GetComponent<MapGenerator>().SpawnItem(1);
        Destroy(gameObject);
    }
}
