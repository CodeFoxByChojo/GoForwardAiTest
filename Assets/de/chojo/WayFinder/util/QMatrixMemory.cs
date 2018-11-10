using de.chojo.WayFinder.Manager;
using UnityEngine;

namespace de.chojo.WayFinder.util {
    public class QMatrixMemory {
        public Point[,] QMatrix { get; set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        private int _records = 0;
        private int _generations;

        public bool Equals(int x, int y) {
            return X == x && Y == y;
        }

        public QMatrixMemory(Vector2Int goal) {
            Generations = 1;
            QMatrix = new Point[Field.GetInstance().Dimensions.x, Field.GetInstance().Dimensions.y];
            X = goal.x;
            Y = goal.y;

            for (var i = 0; i < QMatrix.GetLength(0); i++) {
                for (var j = 0; j < QMatrix.GetLength(1); j++) QMatrix[i, j] = new Point();
            }
        }

        public void RecordsCountAdd(int count) {
            _records += count;
        }

        public int Generations {
            get { return _generations; }
            set { _generations = value; }
        }

        public int Records {
            get { return _records; }
        }
    }
}