using System;
using System.Collections.Generic;
using System.Linq;
using de.chojo.WayFinder.Character;
using de.chojo.WayFinder.util;
using UnityEngine;
using UnityEngine.Serialization;

namespace de.chojo.WayFinder.Manager {
    public class Field : MonoBehaviour {
        private float _heatMapRefreshTimer;
        private float _roundEndTimer;

        private GameObject[,] _heatMap;


        [Header("Game Setup")] [SerializeField]
        private Vector2Int _dimensions = new Vector2Int(11, 11);

        [SerializeField] [Range(1, 10)] private float _heatMapRefresh = 5;
        [SerializeField] [Range(1, 250)] private int _aIsPerRound = 10;

        [SerializeField] private Vector2Int _goalInput = new Vector2Int(5, 5);

        [SerializeField] private float _roundLength = 30;

        [Header("AI Controlls")] [SerializeField] [Range(1, 100)]
        private float _actionsPerSecond;

        [SerializeField] private bool _learning = true;

        [Header("Monitoring! Do not touch!")] [SerializeField]
        private int _aisOnField;

        [SerializeField] private int _aisFoundGoal;
        [SerializeField] private int _currentGeneration = 0;
        [SerializeField] private float _currentRoundDuration;
        private List<Player> _players;

        [Header("Assign Section")] [SerializeField] [Tooltip("Goal Object in Scene")]
        private GameObject _goal;

        [SerializeField] [Tooltip("Player Prefab")]
        private GameObject _aiObject;

        public static Field GetInstance() {
            return FindObjectOfType<Field>();
        }

        /// <summary>
        /// Registration of the known player.
        /// </summary>
        /// <param name="player"></param>
        public void RegisterPlayer(Player player) {
            if (_players == null) _players = new List<Player>();
            _players.Add(player);
        }

        /// <summary>
        /// Check of the player is on the goal
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool CheckForGoal(Vector3 position) {
            return Math.Abs(position.x - Goal.x) < 0.1 && Math.Abs(position.y - Goal.y) < 0.1;
        }


        private void Start() {
            Brain = new CollectiveBrain();

            _roundEndTimer = _roundLength;

            _heatMapRefreshTimer = _heatMapRefresh;

            Camera.main.transform.position =
                new Vector3(_dimensions.x / 2, _dimensions.y / 2, Camera.main.transform.position.z);

            Application.targetFrameRate = 200;
            QualitySettings.vSyncCount = 0;

            GenerateHeatMap();

            StartNewRound();
        }


        private void Update() {
            _heatMapRefreshTimer -= Time.deltaTime;
            _roundEndTimer -= Time.deltaTime;

            if (_roundEndTimer < 0) {
                ForceNewRound();
                _roundEndTimer = _roundLength;
            }

            if (_heatMapRefreshTimer > 0) return;
            _heatMapRefreshTimer = _heatMapRefresh;

            DrawHeatmap();
        }


        //Visualisierung
        /// <summary>
        /// Merge the current matrix data for the heat map
        /// </summary>
        /// <param name="PlayerList"></param>
        /// <returns></returns>
        private QMatrixMemory MergeQMatrixData(List<Player> PlayerList) {
            List<QMatrixMemory> temp = new List<QMatrixMemory>();
            foreach (var entry in PlayerList) {
                temp.Add(entry.CurrentQMatrix);
            }

            var mergedMemory = Helper.AddListToList(temp, Brain.CollectedMemories);
            var data = new QMatrixMemory(Goal);
            for (var i = 0; i < _dimensions.x; i++) {
                for (var j = 0; j < _dimensions.y; j++) {
                    var up = new List<double>();
                    var down = new List<double>();
                    var right = new List<double>();
                    var left = new List<double>();

                    foreach (var memory in mergedMemory) {
                        up.Add(memory.QMatrix[i, j].GetValue(Directions.Up));
                        down.Add(memory.QMatrix[i, j].GetValue(Directions.Down));
                        right.Add(memory.QMatrix[i, j].GetValue(Directions.Right));
                        left.Add(memory.QMatrix[i, j].GetValue(Directions.Left));
                    }

                    data.QMatrix[i, j].SetValue(Directions.Up, Helper.GetAverage(up));
                    data.QMatrix[i, j].SetValue(Directions.Down, Helper.GetAverage(down));
                    data.QMatrix[i, j].SetValue(Directions.Right, Helper.GetAverage(right));
                    data.QMatrix[i, j].SetValue(Directions.Left, Helper.GetAverage(left));
                }
            }

            return data;
        }

        /// <summary>
        /// generate the heatmap at start
        /// </summary>
        private void GenerateHeatMap() {
            _heatMap = new GameObject[_dimensions.x, _dimensions.y];
            for (var i = 0; i < _dimensions.x; i++) {
                for (var j = 0; j < _dimensions.y; j++) {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    obj.transform.SetParent(gameObject.transform);
                    obj.transform.position = new Vector3(i, j, 1);
                    obj.transform.GetComponent<Renderer>().material.color = new Color(1f, 0.92f, 0.99f);
                    _heatMap[i, j] = obj;
                }
            }
        }


        /// <summary>
        /// Redraw the heatmap
        /// </summary>
        private void DrawHeatmap() {
            var matrix = MergeQMatrixData(_players);
            double highestValue = 0;

            for (var i = 0; i < matrix.QMatrix.GetLength(0); i++) {
                for (var j = 0; j < matrix.QMatrix.GetLength(1); j++) {
                    var fieldValue = matrix.QMatrix[i, j].GetBestValue();

                    if (fieldValue > highestValue) highestValue = fieldValue;
                }
            }

            for (var i = 0; i < matrix.QMatrix.GetLength(0); i++) {
                for (var j = 0; j < matrix.QMatrix.GetLength(1); j++)
                    _heatMap[i, j].GetComponent<Renderer>().material.color =
                        Helper.GetPercentAsColor(matrix.QMatrix[i, j].GetBestValue() / highestValue);
            }
        }


        //AI Knowledge Merging
        /// <summary>
        /// Removes the player from the list, save and destroy.
        /// </summary>
        /// <param name="player"></param>
        public void RemovePlayer(Player player) {
            for (var i = 0; i < _players.Count; i++)
                if (_players[i].GetInstanceID() == player.GetInstanceID()) {
                    player.SaveAndDestroy();
                    _players.RemoveAt(i);
                    break;
                }

            if (_players.Count == 0) {
                StartNewRound();
            }
        }


        //New Round
        /// <summary>
        /// Triggers a new round with saving the data of ais, which are not in the goal.
        /// </summary>
        [ContextMenu("EndRound")]
        private void ForceNewRound() {
            foreach (var player in _players) {
                player.SaveAndDestroy();
            }

            _players.Clear();
            StartNewRound();
        }

        /// <summary>
        /// Starts the new round routine
        /// </summary>
        private void StartNewRound() {
            Brain.MergeAndSaveQMatrixData();
            GenerateNewGoal();
            for (int i = 0;
                i < _aIsPerRound;
                i++) {
                Instantiate(_aiObject, new Vector3(-1, -1, -1), new Quaternion());
            }

            Debug.Log("New round started with " + _aIsPerRound + " Testsubjects!");
        }

        /// <summary>
        /// Generates a new Goal. Currently not used
        /// </summary>
        public void GenerateNewGoal() {
            //_goal = new Vector2Int(_dimensions.x,dimensions.y(_dimensions.x, _dimensions.y));
            Goal = _goalInput;
            RMatrix = new QMatrixMemory(Goal);
            RMatrix.QMatrix[Goal.x - 1, Goal.y].SetValue(Directions.Right, 1);
            RMatrix.QMatrix[Goal.x + 1, Goal.y].SetValue(Directions.Left, 1);
            RMatrix.QMatrix[Goal.x, Goal.y - 1].SetValue(Directions.Up, 1);
            RMatrix.QMatrix[Goal.x, Goal.y + 1].SetValue(Directions.Down, 1);
            RMatrix.QMatrix[Goal.x, Goal.y].SetValue(Directions.none, 1);
            _goal.transform.position = new Vector3(Goal.x, Goal.y, 0);
        }

        public Vector2Int Dimensions {
            get { return _dimensions; }
        }

        public QMatrixMemory RMatrix { get; private set; }

        public Vector2Int Goal { get; private set; }

        public CollectiveBrain Brain { get; private set; }

        public bool Learning {
            get { return _learning; }
        }

        public float ActionsPerSecond {
            get { return 1 / _actionsPerSecond; }
        }
    }
}