using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ant.Navigation;

public class TestGameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NavigationManager.Instance.BakeGrid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
