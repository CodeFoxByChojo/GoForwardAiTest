using System.Collections.Generic;
using System.Numerics;
using de.chojo.WayFinder.Character;
using de.chojo.WayFinder.Menu;
using de.chojo.WayFinder.util;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace de.chojo.WayFinder.Manager {
    public class HeatMap : MonoBehaviour {
        private Field _field;
        private GameObject[,] _heatMap;
        private GameControlls _gameControlls;

        private HeatMapType _heatMapType;

        private bool _mergeInProgress;
        private bool _mergeDone = false;
        private QMatrixMemory _mergedMemory;
        private List<QMatrixMemory> _memorysToMerge;
        private int _mergeIndex;
        private int _mergeXIndex;
        private int _mergeYIndex;
        private int _mergePointIndex;
        private int _mergesPerFrame = 20000;

        private bool _drawInProgress = false;
        private bool _findHighestValue = false;
        private BigInteger _highestVisitValue;
        private double _highestWayValue;
        private int _drawXIndex;
        private int _drawYIndex;
        private int _drawsPerFrame = 100;
        private int _valueChecksPerFrame;

        List<double> up = new List<double>();
        List<double> down = new List<double>();
        List<double> right = new List<double>();
        List<double> left = new List<double>();
        List<BigInteger> visits = new List<BigInteger>();


        private void Start() {
            _field = Field.GetInstance();
            _gameControlls = GameControlls.GetInstance();
            _mergeIndex = 0;
            _mergeInProgress = false;
        }

        private void Update() {
            if (_mergeInProgress && Time.deltaTime > 0.016f) {
                _mergesPerFrame -= 20;
            }
            else if (_mergeInProgress && Time.deltaTime < 0.016f) {
                _mergesPerFrame += 20;
            }

            if (_mergeDone && Time.deltaTime > 0.016f) {
                _drawsPerFrame -= 10;
            }
            else if (_mergeDone && Time.deltaTime < 0.016f) {
                _drawsPerFrame += 10;
            }

            if (!_mergeInProgress && !_mergeDone) {
                MergeQMatrixDataAsync();
            }

            if (_mergeInProgress && !_mergeDone) {
                MergeQMatrixDataAsync();
            }

            if (_mergeDone) {
                DrawHeatMapAsync();
            }
        }

        private void CalculateMergeAmount() {
            if (_field.AisFoundGoal + _field.AisOnField == 0) {
                _drawsPerFrame = _mergesPerFrame =
                    Helper.ClampInt(0, 2, 100);
                return;
            }

            _drawsPerFrame = _mergesPerFrame =
                Helper.ClampInt(200 / (_field.AisFoundGoal + _field.AisOnField), 2, 200);
        }

        private void MergeQMatrixDataAsync() {
            if (!_mergeInProgress) {
                List<QMatrixMemory> tempPlayer = new List<QMatrixMemory>();
                _mergedMemory = new QMatrixMemory(_field.Goal);

                _heatMapType = _field.HeatMapType;

                if (_field.Players == null) return;

                foreach (var entry in _field.Players) {
                    tempPlayer.Add(entry.CurrentQMatrix);
                }

                if (_field.Brain.CollectedMemories == null) {
                    _memorysToMerge = new List<QMatrixMemory>(tempPlayer);
                }
                else {
                    _memorysToMerge = Helper.AddListToList(tempPlayer,
                        new List<QMatrixMemory>(_field.Brain.CollectedMemories));
                }

                _mergeInProgress = true;
                _gameControlls.Log("Async matrix merge in progress. Trying to merge " + _memorysToMerge.Count +
                                   " records");
                return;
            }

            int mergeIndex = 0;
            for (var i = _mergeXIndex; i < _field.Dimensions.x; i++) {
                for (var j = _mergeYIndex; j < _field.Dimensions.y; j++) {
                    for (int k = _mergePointIndex; k < _memorysToMerge.Count; k++) {
                        up.Add(_memorysToMerge[k].QMatrix[i, j].GetValue(Directions.Up));
                        down.Add(_memorysToMerge[k].QMatrix[i, j].GetValue(Directions.Down));
                        right.Add(_memorysToMerge[k].QMatrix[i, j].GetValue(Directions.Right));
                        left.Add(_memorysToMerge[k].QMatrix[i, j].GetValue(Directions.Left));
                        visits.Add(_memorysToMerge[k].QMatrix[i, j].Visits);
                        mergeIndex++;
                        if (mergeIndex > _mergesPerFrame) {
                            _mergePointIndex++;
                            return;
                        }
                    }

                    _mergePointIndex = 0;

                    _mergedMemory.QMatrix[i, j].SetValue(Directions.Up, Helper.GetAverage(up));
                    _mergedMemory.QMatrix[i, j].SetValue(Directions.Down, Helper.GetAverage(down));
                    _mergedMemory.QMatrix[i, j].SetValue(Directions.Right, Helper.GetAverage(right));
                    _mergedMemory.QMatrix[i, j].SetValue(Directions.Left, Helper.GetAverage(left));
                    _mergedMemory.QMatrix[i, j].Visits = Helper.GetAverage(visits);

                    up = new List<double>();
                    down = new List<double>();
                    right = new List<double>();
                    left = new List<double>();
                    visits = new List<BigInteger>();

                    _mergeYIndex = j;

                    mergeIndex++;
                    if (mergeIndex > _mergesPerFrame) {
                        if (j + 1 >= _field.Dimensions.y) {
                            _mergeXIndex++;
                            _mergeYIndex = 0;
                            return;
                        }

                        _mergeYIndex++;
                        return;
                    }
                }

                _mergeYIndex = 0;

                _mergeXIndex = i;
            }

            _gameControlls.Log("Merge Done. Trying to find Highest Value.");
            _mergeXIndex = _mergeYIndex = 0;
            _mergeInProgress = false;
            _mergeDone = true;
        }

        /// <summary>
        /// generate the heatmap at start
        /// </summary>
        public void GenerateHeatMap(int x, int y, GameObject field) {
            _heatMap = new GameObject[x, y];
            for (var i = 0; i < x; i++) {
                for (var j = 0; j < y; j++) {
                    var obj = Instantiate(field);
                    obj.transform.SetParent(gameObject.transform);
                    obj.transform.position = new Vector3(i, j, 1);
                    obj.transform.GetComponent<Renderer>().material.color = new Color(1f, 0.92f, 0.99f);
                    _heatMap[i, j] = obj;
                }
            }
        }

        private QMatrixMemory MergeQMatrixDataSync(List<Player> PlayerList) {
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
                    var visits = new List<BigInteger>();

                    foreach (var memory in mergedMemory) {
                        up.Add(memory.QMatrix[i, j].GetValue(Directions.Up));
                        down.Add(memory.QMatrix[i, j].GetValue(Directions.Down));
                        right.Add(memory.QMatrix[i, j].GetValue(Directions.Right));
                        left.Add(memory.QMatrix[i, j].GetValue(Directions.Left));
                        visits.Add(memory.QMatrix[i, j].Visits);
                    }

                    data.QMatrix[i, j].SetValue(Directions.Up, Helper.GetAverage(up));
                    data.QMatrix[i, j].SetValue(Directions.Down, Helper.GetAverage(down));
                    data.QMatrix[i, j].SetValue(Directions.Right, Helper.GetAverage(right));
                    data.QMatrix[i, j].SetValue(Directions.Left, Helper.GetAverage(left));
                    data.QMatrix[i, j].Visits = Helper.GetAverage(visits);
                }
            }

            return data;
        }


        /// <summary>
        /// Redraw the heatmap
        /// </summary>
        private void DrawHeatMapSync() {
            var matrix = _mergedMemory;
            BigInteger highestVisitValue = 0;
            double highestWayValue = 0;

            for (var i = 0; i < matrix.QMatrix.GetLength(0); i++) {
                for (var j = 0; j < matrix.QMatrix.GetLength(1); j++) {
                    if (_heatMapType == HeatMapType.BestWay) {
                        var wayValue = matrix.QMatrix[i, j].GetBestValue();
                        if (wayValue > highestWayValue) highestWayValue = wayValue;
                    }

                    if (_heatMapType == HeatMapType.Visits) {
                        var visitValue = matrix.QMatrix[i, j].Visits;
                        if (visitValue > highestVisitValue) highestVisitValue = visitValue;
                    }
                }
            }
        }

        private void DrawHeatMapAsync() {
            int drawIndex = 0;

            if (!_findHighestValue) {
                for (var i = _drawXIndex; i < _mergedMemory.QMatrix.GetLength(0); i++) {
                    for (var j = _drawYIndex; j < _mergedMemory.QMatrix.GetLength(1); j++) {
                        if (_heatMapType == HeatMapType.BestWay) {
                            var wayValue = _mergedMemory.QMatrix[i, j].GetBestValue();
                            if (wayValue > _highestWayValue) _highestWayValue = wayValue;
                        }

                        if (_heatMapType == HeatMapType.Visits) {
                            var visitValue = _mergedMemory.QMatrix[i, j].Visits;
                            if (visitValue > _highestVisitValue) _highestVisitValue = visitValue;
                        }


                        _drawYIndex = j;
                        drawIndex++;
                        if (drawIndex > _mergesPerFrame) {
                            if (j + 1 >= _mergedMemory.QMatrix.GetLength(1)) {
                                _drawXIndex++;
                                _drawYIndex = 0;
                                return;
                            }

                            _drawYIndex++;
                            return;
                        }
                    }

                    _drawYIndex = 0;

                    _drawXIndex = i;
                }


                _gameControlls.Log("Found Highest Value (" + _highestVisitValue + "). Starting draw of Heat Map");
                _drawXIndex = _drawYIndex = 0;
                _findHighestValue = true;
                return;
            }


            for (var i = _drawXIndex; i < _mergedMemory.QMatrix.GetLength(0); i++) {
                for (var j = _drawYIndex; j < _mergedMemory.QMatrix.GetLength(1); j++) {
                    if (_heatMapType == HeatMapType.BestWay) {
                        _heatMap[i, j].GetComponent<Renderer>().material.color =
                            Helper.GetPercentAsColor(_mergedMemory.QMatrix[i, j].GetBestValue(), _highestWayValue);
                    }

                    if (_heatMapType == HeatMapType.Visits) {
                        _heatMap[i, j].GetComponent<Renderer>().material.color =
                            Helper.GetPercentAsColor(_mergedMemory.QMatrix[i, j].Visits, _highestVisitValue);
                    }

                    _drawYIndex = j;
                    drawIndex++;
                    if (drawIndex > _drawsPerFrame) {
                        if (j + 1 >= _mergedMemory.QMatrix.GetLength(1)) {
                            _drawXIndex++;
                            _drawYIndex = 0;
                            return;
                        }

                        _drawYIndex++;
                        return;
                    }
                }

                _drawYIndex = 0;

                _drawXIndex = i;
            }

            _drawXIndex = _drawYIndex = 0;
            _mergeDone = _findHighestValue = false;
            _gameControlls.Log("Async Heat Map draw done. Starting new Data Merge. Merging " + _mergesPerFrame +
                               " records per frame");
        }
    }
}