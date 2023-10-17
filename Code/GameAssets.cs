using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance;

    public GameObject Point;
    public GameObject WallMaker;
    public GameObject Stamp;

    public GameObject Line;


    private void Awake()
    {
        Instance = this;
    }
}
