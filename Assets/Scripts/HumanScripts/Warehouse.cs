using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warehouse : MonoBehaviour
{
    public GameResourcesList resourcesList;
    private void Awake()
    {
        GameObject.FindGameObjectWithTag("Human").GetComponent<HumanScript>().warehouseIsActive = true;
    }
}
