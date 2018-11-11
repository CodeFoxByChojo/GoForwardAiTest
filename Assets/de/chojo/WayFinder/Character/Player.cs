using System;
using System.Collections.Generic;
using de.chojo.WayFinder.Manager;
using de.chojo.WayFinder.util;
using UnityEngine;
using Random = UnityEngine.Random;

namespace de.chojo.WayFinder.Character {
    public class Player : MonoBehaviour {
        private readonly CharacterPosition _characterPosition = new CharacterPosition();
        private float _actionCounterCounter;


        private readonly float _alpha = 0.1f;
        private Field _field;
        private readonly float _gamma = 0.9f;

        private CollectiveBrain brain;

        public QMatrixMemory CurrentQMatrix { get; private set; }


        private void Start() {
            _field = Field.GetInstance();
            _field.RegisterPlayer(this);

            brain = _field.Brain;

            SetNewStartPoint();

            _actionCounterCounter = 1 / _field.ActionsPerSecond;
            CurrentQMatrix = brain.FindQMatrix(_field.Goal);

            if (WayBlocked(Directions.Up) && WayBlocked(Directions.Down) && WayBlocked(Directions.Left) &&
                WayBlocked(Directions.Right)) {
                _field.RemovePlayer(this);
            }
        }

        private void Update() {
            _actionCounterCounter -= Time.deltaTime;
            if (_actionCounterCounter < 0) {
                Move();
                _actionCounterCounter = 1 / _field.ActionsPerSecond;
                CheckForGoal();
            }
        }

        /// <summary>
        /// Generates a random start point on the field.
        /// </summary>
        private void SetNewStartPoint() {
            var a = new Vector3Int(Random.Range(0, _field.Dimensions.x), Random.Range(0, _field.Dimensions.y),
                -1);
            _characterPosition.CurrentPos = a;
            gameObject.transform.position = a;
        }


        /// <summary>
        /// Checks, if the player found the goal
        /// </summary>
        private void CheckForGoal() {
            if (!_field.CheckForGoal(_characterPosition.CurrentPos)) return;
            GoalFound();
        }


        /// <summary>
        /// Unsign the Player from the Field
        /// </summary>
        private void GoalFound() {
            _field.RemovePlayer(this);
        }

        /// <summary>
        /// The move routine
        /// </summary>
        private void Move() {
            var dir = ChooseNextStep();

            CalculateActionValue(dir);

            GoToNextField(dir);
        }

        /// <summary>
        /// Generates a Random direction, which is not blocked, if learning is enabled.
        /// If learning ist disabled it tries to use the current data to figure out the best way.
        /// </summary>
        /// <returns></returns>
        private Directions ChooseNextStep() {
            Directions direction;

            if (_field.Learning) {
                direction = (Directions) Random.Range(0, 4);

                while (WayBlocked(direction)) direction = (Directions) Random.Range(0, 4);
            }
            else {
                direction = CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x,
                        _characterPosition.CurrentPos.y]
                    .GetBestDirection();
                if (direction == Directions.none) {
                    direction = (Directions) Random.Range(0, 4);

                    while (WayBlocked(direction)) direction = (Directions) Random.Range(0, 4);
                }
            }

            return direction;
        }

        /// <summary>
        /// Checks if the way in the desired direction is blocked.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private bool WayBlocked(Directions direction) {
            if (_field.IsBlocked(
                Helper.GetNewCoordVector2(
                    new Vector2Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y), direction))) {
                return true;
            }

            switch (direction) {
                case Directions.Up:
                    return _characterPosition.CurrentPos.y + 1 > _field.Dimensions.y - 1;
                case Directions.Down:
                    return _characterPosition.CurrentPos.y - 1 < 0;
                case Directions.Right:
                    return _characterPosition.CurrentPos.x + 1 > _field.Dimensions.x - 1;
                case Directions.Left:
                    return _characterPosition.CurrentPos.x - 1 < 0;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }

        /// <summary>
        /// Calculates the Value of the next action
        /// </summary>
        /// <param name="direction"></param>
        private void CalculateActionValue(Directions direction) {
            var value =
                (1 - _alpha) * CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x,
                        _characterPosition.CurrentPos.y]
                    .GetValue(direction) + _alpha * GetActionValue(direction);

            if (double.IsNaN(value)) {
                value = 0;
                Debug.Log("Calculation Failed! NaN!");
            }

            CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y]
                .SetValue(direction, value);
        }

        /// <summary>
        /// moves the Character to the next field
        /// </summary>
        /// <param name="dir"></param>
        private void GoToNextField(Directions dir) {
            _characterPosition.CurrentPos = GetNewCoord(dir);
            gameObject.transform.position = _characterPosition.CurrentPos;
        }

        /// <summary>
        /// Generates the Coords of a point in a specific direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Vector3Int GetNewCoord(Directions direction) {
            switch (direction) {
                case Directions.Up:
                    return new Vector3Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y + 1, -1);
                case Directions.Down:
                    return new Vector3Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y - 1, -1);
                case Directions.Right:
                    return new Vector3Int(_characterPosition.CurrentPos.x + 1, _characterPosition.CurrentPos.y, -1);
                case Directions.Left:
                    return new Vector3Int(_characterPosition.CurrentPos.x - 1, _characterPosition.CurrentPos.y, -1);
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }

        /// <summary>
        /// Returns the value of the best direction of the field in the desired position
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private double GetBestActionValue(Directions direction) {
            var values = new List<float>();

            Point tempPoint;

            switch (direction) {
                case Directions.Up:
                    //if(_characterPosition.CurrentPos.y + 1 > _field.Dimensions.y) break;
                    tempPoint = CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x,
                        _characterPosition.CurrentPos.y + 1];
                    break;
                case Directions.Down:
                    tempPoint = CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x,
                        _characterPosition.CurrentPos.y - 1];
                    break;
                case Directions.Right:
                    tempPoint = CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x + 1,
                        _characterPosition.CurrentPos.y];
                    break;
                case Directions.Left:
                    tempPoint = CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x - 1,
                        _characterPosition.CurrentPos.y];
                    break;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }

            return tempPoint.GetBestValue();
        }

        /// <summary>
        /// Gets the value of the action in de desired direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private double GetActionValue(Directions direction) {
            return _field.RMatrix.QMatrix[_characterPosition.CurrentPos.x,
                           _characterPosition.CurrentPos.y]
                       .GetValue(direction) + _gamma * GetBestActionValue(direction);
        }

        /// <summary>
        /// Saves the Data and Destrys the Player
        /// </summary>
        public void SaveAndDestroy() {
            brain.CollectData(CurrentQMatrix);
            Destroy(gameObject);
        }
    }
}