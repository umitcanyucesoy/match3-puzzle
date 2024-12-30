using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameBoard;
using Unity.Mathematics;
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
    private bool _isPlayerInputEnabled = true;
    private bool _isClearAndRefillRunning = false;
    private bool _isSwitching = false;
    
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
        FillBoardRandomGamePiece();
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

    private void FillBoardRandomGamePiece(int falseYOffset = 0, float moveTime = .1f)
    {
        int maxIterations = 100;
        int iterations = 0;
        
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                if (_mAllPieces[i, j] == null)
                {
                    GamePieces gamePiece = FillBoardGamePieceAt(i, j ,falseYOffset, moveTime);

                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                        gamePiece = FillBoardGamePieceAt(i, j, falseYOffset, moveTime);
                        iterations++;

                        if (iterations >= maxIterations)
                        {
                            iterations = 0;
                            break;
                        }
                    }
                }
            }
        }
    }

    private GamePieces FillBoardGamePieceAt(int i, int j, int falseYOffset = 0, float moveTime = 0.1f)
    {
        var randomGamePiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity,gamePieceParent);
        if (randomGamePiece)
        {
            randomGamePiece.GetComponent<GamePieces>().Init(this);
            PlaceGamePiece(randomGamePiece.GetComponent<GamePieces>(), i, j);
            randomGamePiece.name = $"Piece{i},{j}";

            if (falseYOffset != 0)
            {
                randomGamePiece.transform.position = new Vector3(i, j + falseYOffset, 0);
                randomGamePiece.GetComponent<GamePieces>().MovePiece(i,j,moveTime);
            }
            
            return randomGamePiece.GetComponent<GamePieces>();
        }

        return null;
    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePieces> leftMatches = FindMatches(x,y,new Vector2(-1,0),minLength);
        List<GamePieces> rightMatches = FindMatches(x,y,new Vector2(0,-1),minLength);
        
        leftMatches ??= new List<GamePieces>();
        rightMatches ??= new List<GamePieces>();
        
        return (leftMatches.Count > 0 || rightMatches.Count > 0);
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

    public async void ReleaseToTile()
    {
        if (_clickedTile != null && 
            _targetTile != null && 
            !_isSwitching &&
            _isPlayerInputEnabled && 
            !_isClearAndRefillRunning)
        {
            await SwitchTiles(_clickedTile, _targetTile);
        }

        _clickedTile = null;
        _targetTile = null;
    }

    private async UniTask SwitchTiles(Tile clickedTile, Tile targetTile) 
    { 
        if (_isSwitching || !_isPlayerInputEnabled || _isClearAndRefillRunning) return;
        _isPlayerInputEnabled = false; 
        _isSwitching = true;
        
        GamePieces clickedPiece = _mAllPieces[clickedTile.xIndex, clickedTile.yIndex]; 
        GamePieces targetPiece  = _mAllPieces[targetTile.xIndex, targetTile.yIndex];
        
        _mAllPieces[clickedTile.xIndex, clickedTile.yIndex] = targetPiece;
        _mAllPieces[targetTile.xIndex, targetTile.yIndex]   = clickedPiece;
        
        clickedPiece.MovePiece(targetTile.xIndex, targetTile.yIndex, swapTime); 
        targetPiece.MovePiece(clickedTile.xIndex, clickedTile.yIndex, swapTime);
        
        await UniTask.Delay(TimeSpan.FromSeconds(swapTime));
    
        List<GamePieces> allMatches = FindAllMatchesOnBoard();

        if (allMatches.Count == 0) 
        { 
            _mAllPieces[clickedTile.xIndex, clickedTile.yIndex] = clickedPiece;
            _mAllPieces[targetTile.xIndex, targetTile.yIndex]   = targetPiece;

            clickedPiece.MovePiece(clickedTile.xIndex, clickedTile.yIndex, swapTime); 
            targetPiece.MovePiece(targetTile.xIndex, targetTile.yIndex, swapTime);

            await UniTask.Delay(TimeSpan.FromSeconds(swapTime));
        }
        else 
        { 
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f)); 
            ClearAndRefillBoard(allMatches).Forget();
        }
        
        _isSwitching = false;
        _isPlayerInputEnabled = true; 
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

            if (nextPiece == null)
            {
                break;
            }
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                    matches.Add(nextPiece);
                else
                    break;
            }
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

    private List<GamePieces> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePieces> horizontalMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePieces> verticalMatches = FindVerticalMatches(x, y, minLength);
        
        verticalMatches ??= new List<GamePieces>();
        horizontalMatches ??= new List<GamePieces>();
        
        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }
    
    private List<GamePieces> FindMatchesAt(List<GamePieces> gamePieces, int minLength = 3)
    {
        List<GamePieces> matches = new List<GamePieces>();

        foreach (GamePieces piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }

        return matches;
    }
    
    private List<GamePieces> FindAllMatchesOnBoard(int minLength = 3)
    {
        List<GamePieces> allMatches = new List<GamePieces>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_mAllPieces[x, y] != null)
                {
                    var matchesAtCell = FindMatchesAt(x, y, minLength);
                    if (matchesAtCell != null && matchesAtCell.Count > 0)
                    {
                        allMatches = allMatches.Union(matchesAtCell).ToList();
                    }
                }
            }
        }

        return allMatches;
    }

    private void HighlightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = _mAllTiles[x,y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b);
    }

    private void HighlightTileOn(int x, int y, Color color)
    {
        SpriteRenderer spriteRenderer = _mAllTiles[x,y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
    }

    private void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x,y);
        
        var combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (var piece in combinedMatches)
                HighlightTileOn(piece.xIndex,piece.yIndex,piece.GetComponent<SpriteRenderer>().color);
        }
    }
    
    private void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i,j);
            }
        }    
    }

    private void ClearPieceAt(int x, int y)
    {
        GamePieces pieceToClear = _mAllPieces[x,y];

        if (pieceToClear != null)
        {
            pieceToClear.transform.DOKill();
            _mAllPieces[x,y] = null;
            Destroy(pieceToClear.gameObject);
        }
    }
    
    private void ClearPieceAt(List<GamePieces> gamePieces)
    {
        foreach (var piece in gamePieces)
        {
            if (piece != null)
                ClearPieceAt(piece.xIndex,piece.yIndex);
        }
    }

    private void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i,j);
            }
        }
    }

    private List<GamePieces> CollapseColumn(int column, float collapseTime = .1f)
    {
        List<GamePieces> movingPieces = new List<GamePieces>();

        for (int i = 0; i < height - 1; i++)
        {
            if (_mAllPieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (_mAllPieces[column, j] != null)
                    {
                        _mAllPieces[column,j].MovePiece(column,i, collapseTime * (j - i));
                        _mAllPieces[column,i] = _mAllPieces[column,j];
                        _mAllPieces[column,i].SetCoordinates(column,i);

                        if (!movingPieces.Contains(_mAllPieces[column,i]))
                            movingPieces.Add(_mAllPieces[column,i]);
                        
                        _mAllPieces[column,j] = null;
                        break;
                    }
                }
            }
        }
        
        return movingPieces;
    }

    private UniTask<List<GamePieces>> CollapseColumn(List<GamePieces> gamePieces)
    {
        List<GamePieces> movingPieces = new List<GamePieces>();
        List<int> columnsToCollapse = GetColumns(gamePieces);

        foreach (var column in columnsToCollapse)
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        
        return new UniTask<List<GamePieces>>(movingPieces);
    }

    private List<int> GetColumns(List<GamePieces> gamePieces)
    {
        List<int> columns = new List<int>();

        foreach (var piece in gamePieces)
        {
            if (!columns.Contains(piece.xIndex))
                columns.Add(piece.xIndex);
        }
        
        return columns;
    }
    
    private async UniTask ClearAndRefillBoard(List<GamePieces> gamePieces)
    {
        if (_isClearAndRefillRunning) return;
        
        _isClearAndRefillRunning = true;
        _isPlayerInputEnabled = false;
        
        await ClearAndCollapseBoard(gamePieces);
        await UniTask.Yield();

        FillBoardRandomGamePiece(10,.2f);
        await UniTask.Yield();

        _isPlayerInputEnabled = true;
        _isClearAndRefillRunning = false;
    }

    private async UniTask ClearAndCollapseBoard(List<GamePieces> gamePieces)
    {
        List<GamePieces> movingPieces = new List<GamePieces>();
        List<GamePieces> matches = new List<GamePieces>();
        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        
        bool isFinished = false;
        while (!isFinished)
        {
            ClearPieceAt(gamePieces);
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            
            movingPieces = await CollapseColumn(gamePieces);
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            
            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
                await ClearAndCollapseBoard(matches);
        }

        await UniTask.Yield();
    }
}