using de.chojo.WayFinder.Manager;
using UnityEngine;

namespace de.chojo.WayFinder.util {
    public class QMatrixMemory {
        

        public Point[,] QMatrix { get; set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public bool Equals(int x, int y) {
            return X == x && Y == y;
        }
        
        public QMatrixMemory(Vector2Int goal) {
            QMatrix = new Point[Field.GetInstance().Dimensions.x, Field.GetInstance().Dimensions.y];
            X = goal.x;
            Y = goal.y;

            for (var i = 0; i < QMatrix.GetLength(0); i++) {
                for (var j = 0; j < QMatrix.GetLength(1); j++) QMatrix[i, j] = new Point();
            }
        }
    }
}