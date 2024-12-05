using System;
using System.Collections;
using System.Collections.Generic;
using GameBoard;
using UnityEngine;


public class Board : MonoBehaviour
{
    [Header(" Settings ")]
    public int width;
    public int height;
    public int borderSize;
    private Tile[,] _mAllTiles;

    [Header(" Elements ")]
    [SerializeField] private GameObject tilePrefab;

    public static Board instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _mAllTiles = new Tile[width, height];
        SetupTiles();
    }

    private void SetupTiles()
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity);
                tile.name = $"Tile{i},{j}";
                _mAllTiles [i,j] = tile.GetComponent<Tile>();
                tile.transform.parent = transform;
            }
        }
    }
}