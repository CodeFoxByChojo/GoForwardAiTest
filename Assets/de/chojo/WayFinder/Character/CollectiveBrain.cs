using System.Collections.Generic;
using de.chojo.WayFinder.util;
using UnityEngine;

namespace de.chojo.WayFinder.Character {
    public class CollectiveBrain {
        private List<QMatrixMemory> _collectedMemories;


        private readonly List<QMatrixMemory> _memory = new List<QMatrixMemory>();


        /// <summary>
        /// Returns a matrix for a specific goal, if the goal is known
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        public QMatrixMemory FindQMatrix(Vector2Int goal) {
            foreach (var matrix in _memory)
                if (matrix.Equals(goal.x, goal.y))
                    return matrix;
            return new QMatrixMemory(goal);
        }

        /// <summary>
        /// Saves the Data.
        /// </summary>
        /// <param name="newMatrix"></param>
        private void SafeQMatrix(QMatrixMemory newMatrix) {
            foreach (var matrix in _memory)
                if (matrix.Equals(newMatrix.X, newMatrix.Y)) {
                    matrix.QMatrix = newMatrix.QMatrix;
                    return;
                }

            _memory.Add(newMatrix);
        }

        /// <summary>
        /// Collect data from player.
        /// </summary>
        /// <param name="qMatrixMemory"></param>
        public void CollectData(QMatrixMemory qMatrixMemory) {
            if (_collectedMemories == null) _collectedMemories = new List<QMatrixMemory>();
            _collectedMemories.Add(qMatrixMemory);
        }


        /// <summary>
        /// Merge the collected data and safe them.
        /// </summary>
        public void MergeAndSaveQMatrixData() {
            if(_collectedMemories == null) return;
            Debug.Log("Merged " + _collectedMemories.Count + " collected Memories!");
            var goal = new Vector2Int(_collectedMemories[1].X, _collectedMemories[0].Y);
            var data = new QMatrixMemory(goal);
            for (var i = 0; i < _collectedMemories[0].QMatrix.GetLength(0); i++) {
                for (var j = 0; j < _collectedMemories[0].QMatrix.GetLength(1); j++) {
                    var up = new List<double>();
                    var down = new List<double>();
                    var right = new List<double>();
                    var left = new List<double>();

                    foreach (var player in _collectedMemories) {
                        up.Add(player.QMatrix[i, j].GetValue(Directions.Up));
                        down.Add(player.QMatrix[i, j].GetValue(Directions.Down));
                        right.Add(player.QMatrix[i, j].GetValue(Directions.Right));
                        left.Add(player.QMatrix[i, j].GetValue(Directions.Left));
                    }

                    data.QMatrix[i, j].SetValue(Directions.Up, Helper.GetAverage(up));
                    data.QMatrix[i, j].SetValue(Directions.Down, Helper.GetAverage(down));
                    data.QMatrix[i, j].SetValue(Directions.Right, Helper.GetAverage(right));
                    data.QMatrix[i, j].SetValue(Directions.Left, Helper.GetAverage(left));
                }
            }

            SafeQMatrix(data);
            _collectedMemories.Clear();
        }
    }
}