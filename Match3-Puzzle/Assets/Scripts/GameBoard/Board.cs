using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameBoard;
using UnityEngine;
using Random = UnityEngine.Random;


public class Board : MonoBehaviour
{
    [Header(" Settings ")]
    [SerializeField] private float swapTime;
    public int width;
    public int height;
    public int borderSize;
    private Tile[,] _mAllTiles;

    [Header(" Elements ")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform gamePieceParent;
    public List<GamePieces> gamePiecePrefabs;
    private GamePieces[,] _mAllPieces;
    private Tile _clickedTile;
    private Tile _targetTile;
    
    
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
        HighlightMatches();
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

    public void PlaceGamePiece(GamePieces gamePiece,int x, int y)
    {
        if (!gamePiece)
        {
            Debug.LogWarning("No game piece selected");
            return;
        }
        
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if(IsWithinBounds(x, y))
            _mAllPieces[x, y] = gamePiece;
        gamePiece.SetCoordinates(x,y);
    }

    private bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    private void FillRandomGamePiece()
    {
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var randomGamePiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity,gamePieceParent);
                if (randomGamePiece)
                {
                    randomGamePiece.GetComponent<GamePieces>().Init(this);
                    PlaceGamePiece(randomGamePiece.GetComponent<GamePieces>(), i, j);
                }
                randomGamePiece.name = $"Piece{i},{j}";
            }
        }
    }

    public void ClickTile(Tile tile)
    {
        if (_clickedTile == null)
        {
            _clickedTile = tile;
            Debug.Log($"Clicked tile {tile}");
        }
    }

    public void DragToTile(Tile tile)
    {
        if(_clickedTile != null && IsNextTo(tile,_clickedTile))
            _targetTile = tile;
    }

    public void ReleaseToTile()
    {
        if (_clickedTile != null && _targetTile != null)
            SwitchTiles(_clickedTile,_targetTile);
        
        _clickedTile = null;
        _targetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        GamePieces clickedPiece = _mAllPieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePieces targetPiece = _mAllPieces[targetTile.xIndex, targetTile.yIndex];
        
        clickedPiece.MovePiece(_targetTile.xIndex, _targetTile.yIndex, swapTime);
        targetPiece.MovePiece(_clickedTile.xIndex, _clickedTile.yIndex, swapTime);
    }

    private bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
            return true;
        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
            return true;

        return false;
    }

    private List<GamePieces> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePieces> matches = new List<GamePieces>();
        GamePieces startPiece = null;
        
        if (IsWithinBounds(startX, startY))
            startPiece = _mAllPieces[startX, startY];

        if (startPiece)
            matches.Add(startPiece);
        else
            return null;

        var maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            var nextX = startX + (int)Mathf.Clamp(searchDirection.x,-1,1) * i;
            var nextY = startY + (int)Mathf.Clamp(searchDirection.y,-1,1) * i;

            if (!IsWithinBounds(nextX, nextY))
                break;
            
            GamePieces nextPiece = _mAllPieces[nextX, nextY];
            if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                matches.Add(nextPiece);
            else
                break;
        }

        if (matches.Count >= minLength)
            return matches;

        return null;
    }

    private List<GamePieces> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePieces> upwardMatches = FindMatches(startX, startY, new Vector2(0,1), 2);
        List<GamePieces> downwardMatches = FindMatches(startX, startY, new Vector2(0,-1), 2);
        
        upwardMatches ??= new List<GamePieces>();
        downwardMatches ??= new List<GamePieces>();

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    private List<GamePieces> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePieces> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePieces> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);
        
        rightMatches ??= new List<GamePieces>();
        leftMatches ??= new List<GamePieces>();
        
        var combinedMatches = rightMatches.Union(leftMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    private void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                SpriteRenderer spriteRenderer = _mAllTiles[i,j].GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b);

                List<GamePieces> horizontalMatches = FindHorizontalMatches(i, j, 3);
                List<GamePieces> verticalMatches = FindVerticalMatches(i, j, 3);
                
                verticalMatches ??= new List<GamePieces>();
                horizontalMatches ??= new List<GamePieces>();
                
                var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
                if (combinedMatches.Count > 0)
                {
                    foreach (var piece in combinedMatches)
                    {
                        spriteRenderer = _mAllTiles[piece.xIndex,piece.yIndex].GetComponent<SpriteRenderer>();
                        spriteRenderer.color = piece.GetComponent<SpriteRenderer>().color;
                    }
                }
            }
        }    
    }
}