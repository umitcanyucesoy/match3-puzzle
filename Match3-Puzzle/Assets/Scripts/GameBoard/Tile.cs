using System;
using UnityEngine;

namespace GameBoard
{
    public class Tile : MonoBehaviour
    {
        [Header("Tile Properties")] 
        public int xIndex;
        public int yIndex;

        private Board _board;

        public void Init(int x, int y, Board board)
        {
            _board = board;
            xIndex = x;
            yIndex = y;
        }

        private void OnMouseDown()
        {
            if(_board)
                _board.ClickTile(this);
        }

        private void OnMouseEnter()
        {
            if(_board)
                _board.DragToTile(this);
        }

        private void OnMouseUp()
        {
            if(_board)
                _board.ReleaseToTile();
        }
    }
}