using System;
using System.Collections.Generic;
using System.Linq;
using de.chojo.WayFinder.Character;
using de.chojo.WayFinder.Menu;
using de.chojo.WayFinder.util;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace de.chojo.WayFinder.Manager {
    public class Field : MonoBehaviour {
        private float _roundEndTimer;

        private HeatMap _heatMap;

        private bool[,] _blocked;

        private int _obstaclesPerUnit = 5;


        [Header("Game Setup")] [SerializeField]
        private Vector2Int _dimensions = new Vector2Int(11, 11);


        [SerializeField] [Range(1, 10)] private float _heatMapRefresh = 5;
        [SerializeField] [Range(1, 250)] private int _aIsPerRound = 10;

        [SerializeField] private Vector2Int _goalInput = new Vector2Int(5, 5);

        [SerializeField] private float _roundLength = 30;

        private HeatMapType _heatMapType = HeatMapType.BestWay;

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

        [SerializeField] private GameObject _fieldFrame;

        private GameControlls _gameControlls;

        private int curiosity = 0;

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
            LoadPlayerPrefs();

            _gameControlls = GameControlls.GetInstance();

            Brain = new CollectiveBrain();

            _roundEndTimer = _roundLength;

            Camera.main.transform.position =
                new Vector3(_dimensions.x / 2, _dimensions.y / 2, Camera.main.transform.position.z);

            Application.targetFrameRate = 200;
            QualitySettings.vSyncCount = 0;

            _heatMap = new GameObject().AddComponent<HeatMap>();
            _heatMap.GenerateHeatMap(_dimensions.x, _dimensions.y, _fieldFrame);

            GenerateNewGoal(true, false);

            GenerateObstacles((int) ((_dimensions.x * _dimensions.y) * 0.15f));
        }

        private void GenerateObstacles(int amount) {
            var obj = new GameObject {name = "Obstacles"};
            _blocked = new bool[_dimensions.x, _dimensions.y];

            int obstacleValue = (int) (_dimensions.x * _dimensions.y * 0.3f / _obstaclesPerUnit);

            for (int i = 0; i < obstacleValue; i++) {
                Debug.Log("Generate Obstacle Line");
                Vector3Int start =
                    new Vector3Int(Random.Range(1, _dimensions.y - 1), Random.Range(1, _dimensions.y - 1), -2);
                while (start.x == Goal.x && start.y == Goal.y) {
                    start = new Vector3Int(Random.Range(1, _dimensions.x - 1), Random.Range(1, _dimensions.y-1), -2);
                }

                Vector3Int lastCoord = start;

                Directions dir = (Directions) Random.Range(0, 3);

                GenerateObstacleAt(start, obj);

                for (int j = 0; j < _obstaclesPerUnit; j++) {
                    Debug.Log("Generate Obstacle");

                    lastCoord = Helper.GetNewCoordVector3(lastCoord, dir);
                    if (lastCoord.x == Goal.x && lastCoord.y == Goal.y) continue;
                    if (lastCoord.x > _dimensions.x - 1 || lastCoord.x < 0 || lastCoord.y > _dimensions.y - 1 ||
                        lastCoord.y < 0) return;
                    if (_blocked[lastCoord.x, lastCoord.y]) continue;
                    GenerateObstacleAt(lastCoord, obj);
                }
            }
        }

        private void GenerateObstacleAt(Vector3Int pos, GameObject parent) {
            var field = Instantiate(_fieldFrame);
            field.name = "Obstacle";
            field.GetComponent<Renderer>().material.color = Color.black;
            field.transform.SetParent(parent.transform);
            field.transform.position = pos;
            _blocked[pos.x, pos.y] = true;
        }


        private void Update() {
            UpdateMonitoring();

            _currentRoundDuration += Time.deltaTime;
            _roundEndTimer -= Time.deltaTime;

            if (_roundEndTimer < 0) {
                ForceNewRound();
                _roundEndTimer = _roundLength;
            }
        }


        //Visualisierung
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
        [
            ContextMenu("EndRound")]
        public void ForceNewRound() {
            if (_players != null) {
                foreach (var player in _players) {
                    player.SaveAndDestroy();
                }

                _players.Clear();
            }

            StartNewRound();
        }

        /// <summary>
        /// Starts the new round routine
        /// </summary>
        private void StartNewRound() {
            _currentGeneration++;
            _currentRoundDuration = 0;

            Brain.MergeAndSaveQMatrixData();
            for (int i = 0;
                i < _aIsPerRound;
                i++) {
                Instantiate(_aiObject, new Vector3(-1, -1, -1), new Quaternion());
            }

            _gameControlls.Log("Start new Round");
        }

        /// <summary>
        /// Generates a new Goal. Currently not used
        /// </summary>
        public void GenerateNewGoal(bool load, bool newGoal) {
            if (load) {
                Goal = _goalInput;
            }
            else if (newGoal) {
                Goal = new Vector2Int(Random.Range(1, _dimensions.x - 1),
                    Random.Range(1, _dimensions.y - 1));
                while (_blocked[Goal.x, Goal.y]) {
                    Goal = new Vector2Int(Random.Range(1, _dimensions.x - 1),
                        Random.Range(1, _dimensions.y - 1));
                }
            }

            RMatrix = GenerateRMatrix(Goal);

            _goal.transform.position = new Vector3(Goal.x, Goal.y, 0);

            _gameControlls.Log("New goal is: " + Goal.x + "|" + Goal.y);

            ForceNewRound();
        }

        private QMatrixMemory GenerateRMatrix(Vector2Int goal) {
            QMatrixMemory memory = new QMatrixMemory(goal);

            memory.QMatrix[goal.x - 1, goal.y].SetValue(Directions.Right, 1);
            memory.QMatrix[goal.x + 1, goal.y].SetValue(Directions.Left, 1);
            memory.QMatrix[goal.x, goal.y - 1].SetValue(Directions.Up, 1);
            memory.QMatrix[goal.x, goal.y + 1].SetValue(Directions.Down, 1);
            memory.QMatrix[goal.x, goal.y].SetValue(Directions.None, 1);

            return memory;
        }

        private void UpdateMonitoring() {
            if (_players != null) {
                _aisOnField = _players.Count;
            }

            if (Brain.CollectedMemories == null) return;
            _aisFoundGoal = Brain.CollectedMemories.Count;
        }

        public void LoadPlayerPrefs() {
            _aIsPerRound = PlayerPrefsHandler.GetAiAmount();
            _dimensions.x = PlayerPrefsHandler.GetDimensionX();
            _dimensions.y = PlayerPrefsHandler.GetDimensionY();
            _goalInput = new Vector2Int {x = PlayerPrefsHandler.GetGoalX(), y = PlayerPrefsHandler.GetGoalY()};
            _roundLength = PlayerPrefsHandler.GetRoundDuration();
            _actionsPerSecond = PlayerPrefsHandler.GetAiActionsPerSecond();
        }

        public bool IsBlocked(Vector2Int pos) {
            if (Helper.IsIndexOutOfArray(_blocked, pos.x, pos.y)) {
                return true;
            }

            return _blocked[pos.x, pos.y];
        }

        public Vector2Int Dimensions {
            get { return _dimensions; }
        }

        public QMatrixMemory RMatrix { get; private set; }

        public Vector2Int Goal { get; private set; }

        public CollectiveBrain Brain { get; private set; }

        public bool Learning {
            get { return _learning; }
            set { _learning = value; }
        }

        public float ActionsPerSecond {
            get { return _actionsPerSecond; }
            set { _actionsPerSecond = value; }
        }

        public float RoundLength {
            get { return _roundLength; }
            set { _roundLength = value; }
        }

        public int AisOnField {
            get { return _aisOnField; }
        }

        public int AisFoundGoal {
            get { return _aisFoundGoal; }
        }

        public int CurrentGeneration {
            get { return _currentGeneration; }
        }

        public float CurrentRoundDuration {
            get { return _currentRoundDuration; }
        }

        public int AIsPerRound {
            get { return _aIsPerRound; }
            set { _aIsPerRound = value; }
        }

        public HeatMapType HeatMapType {
            get { return _heatMapType; }
            set { _heatMapType = value; }
        }

        public GameObject FieldFrame {
            get { return _fieldFrame; }
        }

        public List<Player> Players {
            get { return _players; }
        }

        public int Curiosity {
            get { return curiosity; }
            set { curiosity = value; }
        }
    }
}