using System;
using System.Collections;
using System.Collections.Generic;
using GameBoard;
using UnityEngine;
using Random = UnityEngine.Random;


public class Board : MonoBehaviour
{
    [Header(" Settings ")]
    public int width;
    public int height;
    public int borderSize;
    private Tile[,] _mAllTiles;

    [Header(" Elements ")]
    [SerializeField] private GameObject tilePrefab;
    
    public List<GamePieces> gamePiecePrefabs;
    private GamePieces[,] _mAllPieces;
    
    
    public static Board instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _mAllTiles = new Tile[width, height];
        _mAllPieces = new GamePieces[width, height];
        
        SetupTiles();
        FillRandomGamePiece();
    }

    private void SetupTiles()
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity);
                tile.name = $"Tile{i},{j}";
                tile.transform.parent = transform;
                
                _mAllTiles [i,j] = tile.GetComponent<Tile>();
                _mAllTiles[i,j].Init(i,j,this);
            }
        }
    }

    private GameObject GetRandomGamePiece()
    {
        var randomIndex = Random.Range(0, gamePiecePrefabs.Count);
        
        if(!gamePiecePrefabs[randomIndex])
            Debug.LogWarning("No random game piece selected");
        
        return gamePiecePrefabs[randomIndex].gameObject;
    }

    private void PlaceGamePiece(GamePieces gamePiece,int x, int y)
    {
        if (!gamePiece)
        {
            Debug.LogWarning("No game piece selected");
            return;
        }
        
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        gamePiece.SetCoordinates(x,y);
    }

    private void FillRandomGamePiece()
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var randomGamePiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);
                if (randomGamePiece)
                    PlaceGamePiece(randomGamePiece.GetComponent<GamePieces>(), i, j);
            }
        }
    }
    
}