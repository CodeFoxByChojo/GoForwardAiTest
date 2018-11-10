using System.Collections.Generic;
using de.chojo.WayFinder.Character;
using de.chojo.WayFinder.util;
using UnityEngine;

namespace de.chojo.WayFinder.Manager {
    public class HeatMap : MonoBehaviour {
        private Field _field;
        private GameObject[,] _heatMap;

        private bool _mergeInProgress = false;
        private QMatrixMemory _mergedMemory;
        private List<QMatrixMemory> _memorysToMerge;
        private int _mergeIndex;
        private int _mergeXIndex;
        private int _mergeYIndex;
        private int mergesPerFrame;

        private bool _drawInProgress = false;
        private int _drawXIndex;
        private int _drawYIndex;
        private int _drawsPerFrame;


        private void Start() {
            _field = Field.GetInstance();
            _mergeIndex = 0;
            _mergeInProgress = true;
        }

        private void Update() {
            if (!_drawInProgress && !_mergeInProgress) {
                MergeQMatrixDataAsync(true);
                _mergeInProgress = true;
            }

            if (_mergeInProgress) {
                MergeQMatrixDataAsync(false);
            }
        }

        private void MergeQMatrixDataAsync(bool init) {
            if (init) {
                List<QMatrixMemory> temp = new List<QMatrixMemory>();
                List<Player> PlayerList = _field.Players;

                foreach (var entry in PlayerList) {
                    temp.Add(entry.CurrentQMatrix);
                }

                _memorysToMerge = Helper.AddListToList(temp, _field.Brain.CollectedMemories);
            }

            int mergeIndexGoal = _mergeIndex + mergesPerFrame;
            int currentMergeIndex = _mergeIndex;

            for (var i = _mergeXIndex; i < _field.Dimensions.x; i++) {
                for (var j = _mergeYIndex; j < _field.Dimensions.y; j++) {
                    var up = new List<double>();
                    var down = new List<double>();
                    var right = new List<double>();
                    var left = new List<double>();
                    long visits = 0;

                    foreach (var memory in _memorysToMerge) {
                        up.Add(memory.QMatrix[i, j].GetValue(Directions.Up));
                        down.Add(memory.QMatrix[i, j].GetValue(Directions.Down));
                        right.Add(memory.QMatrix[i, j].GetValue(Directions.Right));
                        left.Add(memory.QMatrix[i, j].GetValue(Directions.Left));
                        visits += memory.QMatrix[i, j].Visits;
                    }

                    _mergedMemory.QMatrix[i, j].SetValue(Directions.Up, Helper.GetAverage(up));
                    _mergedMemory.QMatrix[i, j].SetValue(Directions.Down, Helper.GetAverage(down));
                    _mergedMemory.QMatrix[i, j].SetValue(Directions.Right, Helper.GetAverage(right));
                    _mergedMemory.QMatrix[i, j].SetValue(Directions.Left, Helper.GetAverage(left));
                    _mergedMemory.QMatrix[i, j].Visits = visits;

                    _mergeYIndex = j;
                    
                    currentMergeIndex++;
                    if (currentMergeIndex > mergeIndexGoal) {
                        return;
                    }
                }

                _mergeXIndex = i;
            }

            _mergeInProgress = false;

        }

        /// <summary>
        /// generate the heatmap at start
        /// </summary>
        public void GenerateHeatMap(int x, int y) {
            _heatMap = new GameObject[x, y];
            for (var i = 0; i < x; i++) {
                for (var j = 0; j < y; j++) {
                    var obj = Instantiate(_field.FieldFrame);
                    obj.transform.SetParent(gameObject.transform);
                    obj.transform.position = new Vector3(i, j, 1);
                    obj.transform.GetComponent<Renderer>().material.color = new Color(1f, 0.92f, 0.99f);
                    _heatMap[i, j] = obj;
                }
            }
        }

        private QMatrixMemory MergeQMatrixData(List<Player> PlayerList) {
            List<QMatrixMemory> temp = new List<QMatrixMemory>();
            foreach (var entry in PlayerList) {
                temp.Add(entry.CurrentQMatrix);
            }

            var mergedMemory = Helper.AddListToList(temp, _field.Brain.CollectedMemories);
            var data = new QMatrixMemory(_field.Goal);
            for (var i = 0; i < _field.Dimensions.x; i++) {
                for (var j = 0; j < _field.Dimensions.y; j++) {
                    var up = new List<double>();
                    var down = new List<double>();
                    var right = new List<double>();
                    var left = new List<double>();
                    long visits = 0;

                    foreach (var memory in mergedMemory) {
                        up.Add(memory.QMatrix[i, j].GetValue(Directions.Up));
                        down.Add(memory.QMatrix[i, j].GetValue(Directions.Down));
                        right.Add(memory.QMatrix[i, j].GetValue(Directions.Right));
                        left.Add(memory.QMatrix[i, j].GetValue(Directions.Left));
                        visits += memory.QMatrix[i, j].Visits;
                    }

                    data.QMatrix[i, j].SetValue(Directions.Up, Helper.GetAverage(up));
                    data.QMatrix[i, j].SetValue(Directions.Down, Helper.GetAverage(down));
                    data.QMatrix[i, j].SetValue(Directions.Right, Helper.GetAverage(right));
                    data.QMatrix[i, j].SetValue(Directions.Left, Helper.GetAverage(left));
                    data.QMatrix[i, j].Visits = visits;
                }
            }

            return data;
        }


        /// <summary>
        /// Redraw the heatmap
        /// </summary>
        private void DrawHeatMap() {
            var matrix = MergeQMatrixData(_field.Players);
            double highestValue = 0;

            for (var i = 0; i < matrix.QMatrix.GetLength(0); i++) {
                for (var j = 0; j < matrix.QMatrix.GetLength(1); j++) {
                    double fieldValue = 0;

                    if (_field.HeatMapType == HeatMapType.BestWay) {
                        fieldValue = matrix.QMatrix[i, j].GetBestValue();
                    }

                    if (_field.HeatMapType == HeatMapType.Visits) {
                        fieldValue = matrix.QMatrix[i, j].Visits;
                    }

                    if (fieldValue > highestValue) highestValue = fieldValue;
                }
            }


            for (var i = 0; i < matrix.QMatrix.GetLength(0); i++) {
                for (var j = 0; j < matrix.QMatrix.GetLength(1); j++) {
                    if (_field.HeatMapType == HeatMapType.BestWay) {
                        _heatMap[i, j].GetComponent<Renderer>().material.color =
                            Helper.GetPercentAsColor(matrix.QMatrix[i, j].GetBestValue() / highestValue);
                    }

                    if (_field.HeatMapType == HeatMapType.Visits) {
                        _heatMap[i, j].GetComponent<Renderer>().material.color =
                            Helper.GetPercentAsColor(matrix.QMatrix[i, j].Visits / highestValue);
                    }
                }
            }
        }
    }
}