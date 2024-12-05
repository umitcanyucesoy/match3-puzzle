using UnityEngine;

namespace GameBoard
{
    public class GamePieces : MonoBehaviour
    {
        public int xIndex;
        public int yIndex;


        public void SetCoordinates(int x, int y)
        {
            xIndex = x;
            yIndex = y;
        }
    }
}