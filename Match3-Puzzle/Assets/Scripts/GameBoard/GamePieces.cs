using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace GameBoard
{
    public class GamePieces : MonoBehaviour
    {
        public int xIndex;
        public int yIndex;

        private Board _board;
        private bool _isMoving;

        public MatchValue matchValue;
        public enum MatchValue
        {
            Yellow,
            Blue,
            Magenta,
            Indigo,
            Green,
            Teal,
            Red,
            Cyan,
            Wild
        }
        
        public void Init(Board board)
        {
            _board = board;
        }
        
        public void SetCoordinates(int x, int y)
        {
            xIndex = x;
            yIndex = y;
        }
        
        public void MovePiece(int destX, int destY, float timeToMove)
        {
            Vector3 destination = new Vector3(destX, destY, 0f);
            
            if (!_isMoving)
            {
                _isMoving = true;
                transform.DOMove(destination, timeToMove)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        _isMoving = false;
                        if (_board)
                            _board.PlaceGamePiece(this, destX, destY);
                    });
            }
        }
        
    } 
}