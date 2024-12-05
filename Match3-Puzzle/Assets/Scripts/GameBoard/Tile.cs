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
    }
}