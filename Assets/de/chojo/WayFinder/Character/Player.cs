using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using de.chojo.WayFinder.Manager;
using de.chojo.WayFinder.util;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace de.chojo.WayFinder.Character {
    public class Player : MonoBehaviour {
        private readonly CharacterPosition _characterPosition = new CharacterPosition();
        private float _actionCounterCounter;


        private readonly float _alpha = 0.1f;
        private Field _field;
        private readonly float _gamma = 0.9f;

        private int _maxVisitValue;
        private double _maxWayValue;

        private CollectiveBrain brain;

        public QMatrixMemory CurrentQMatrix {get; private set;}


        private void Start() {
            _field = Field.GetInstance();
            _field.RegisterPlayer(this);

            brain = _field.Brain;

            SetNewStartPoint();

            _actionCounterCounter = 1 / _field.ActionsPerSecond;
            CurrentQMatrix = brain.FindQMatrix(_field.Goal, out _maxVisitValue, out _maxWayValue);

            if(WayBlocked(Directions.Up) && WayBlocked(Directions.Down) && WayBlocked(Directions.Left) &&
               WayBlocked(Directions.Right)) {
                _field.RemovePlayer(this);
            }
        }

        private void Update() {
            _actionCounterCounter -= Time.deltaTime;
            if(_actionCounterCounter < 0) {
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
            if(!_field.CheckForGoal(_characterPosition.CurrentPos)) return;
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
            Directions direction = GetDirectionByCuriosity();

            if(direction != Directions.None) {
                return direction;
            }


            //Just get a Random Direction
            if(_field.Learning) {
                direction = (Directions)Random.Range(0, 4);

                while(WayBlocked(direction)) direction = (Directions)Random.Range(0, 4);
            }

            //Find With Curiosity
            else {
                direction = CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x,
                                                   _characterPosition.CurrentPos.y]
                    .GetBestDirection();
                if(direction == Directions.None) {
                    direction = (Directions)Random.Range(0, 4);

                    while(WayBlocked(direction)) direction = (Directions)Random.Range(0, 4);
                }
            }

            return direction;
        }

        private Directions GetDirectionByCuriosity() {
            bool _fieldHasValue = CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y].GetBestValue() > 0;

            if(_field.Curiosity == 0) {
                return Directions.None;
            }

            Point current = CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y];

            Point up = null;
            Vector2Int upVector = Helper.GetNewCoordVector2(
                                                            new Vector2Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y), Directions.Up);
            if(!Helper.IsIndexOutOfArray(CurrentQMatrix.QMatrix, upVector.x, upVector.y)) {
                if(!WayBlocked(Directions.Up))
                    up = CurrentQMatrix.QMatrix[upVector.x, upVector.y];
            }

            Point down = null;
            Vector2Int downVector = Helper.GetNewCoordVector2(
                                                              new Vector2Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y),
                                                              Directions.Down);
            if(!Helper.IsIndexOutOfArray(CurrentQMatrix.QMatrix, downVector.x, downVector.y)) {
                if(!WayBlocked(Directions.Down))
                    down = CurrentQMatrix.QMatrix[downVector.x, downVector.y];
            }

            Point left = null;
            Vector2Int leftVector = Helper.GetNewCoordVector2(
                                                              new Vector2Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y),
                                                              Directions.Left);
            if(!Helper.IsIndexOutOfArray(CurrentQMatrix.QMatrix, leftVector.x, leftVector.y)) {
                if(!WayBlocked(Directions.Left))
                    left = CurrentQMatrix.QMatrix[leftVector.x, leftVector.y];
            }

            Point right = null;
            Vector2Int rightVector = Helper.GetNewCoordVector2(
                                                               new Vector2Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y),
                                                               Directions.Right);
            if(!Helper.IsIndexOutOfArray(CurrentQMatrix.QMatrix, rightVector.x, rightVector.y)) {
                if(!WayBlocked(Directions.Right))
                    right = CurrentQMatrix.QMatrix[rightVector.x, rightVector.y];
            }

            double upPercent = 1;
            double downPercent = 1;
            double leftPercent = 1;
            double rightPercent = 1;

            if(up != null) {
                upPercent = Helper.SaveDivide(up.Visits.Up, _maxVisitValue);
            }

            if(down != null) {
                downPercent = Helper.SaveDivide(down.Visits.Down, _maxVisitValue);
            }

            if(left != null) {
                leftPercent = Helper.SaveDivide(left.Visits.Left, _maxVisitValue);
            }

            if(right != null) {
                rightPercent = Helper.SaveDivide(right.Visits.Right, _maxVisitValue);
            }

            if(_fieldHasValue) {
                //go to field with lowest value.
                if(up != null)
                    if(!WayBlocked(Directions.Up))
                        if(Math.Abs(up.GetBestValue()) < 0.0000000000001)
                            return Directions.Up;

                if(down != null)
                    if(!WayBlocked(Directions.Down))
                        if(Math.Abs(down.GetBestValue()) < 0.0000000000001)
                            return Directions.Down;

                if(left != null)
                    if(!WayBlocked(Directions.Left))
                        if(Math.Abs(left.GetBestValue()) < 0.0000000000001)
                            return Directions.Left;

                if(right != null)
                    if(!WayBlocked(Directions.Right))
                        if(Math.Abs(right.GetBestValue()) < 0.0000000000001)
                            return Directions.Right;

                //Go to field with lowest visit rate.
                int visits;
                Directions dir = current.Visits.GetDirectionWithLowestVistis(out visits);
                if(visits / _maxVisitValue < (_field.Curiosity / 100) * _maxVisitValue)
                    if(dir != Directions.None)
                        return dir;
            }

            if(!_fieldHasValue) {
                if(Random.Range(1, 100) <= _field.Curiosity) {
                    //TODO: Go to field with highest Value. If not found return random.
                    double upValue = 0;
                    double downValue = 0;
                    double leftValue = 0;
                    double rightValue = 0;

                    if(up != null)
                        upValue = up.GetBestValue();
                    if(down != null)
                        downValue = down.GetBestValue();
                    if(left != null)
                        leftValue = left.GetBestValue();
                    if(right != null)
                        rightValue = right.GetBestValue();

                    if(!WayBlocked(Directions.Up))
                        if(upValue != 0)
                            if(Helper.IsLargestNumber(upValue, downValue, leftValue, rightValue))
                                return Directions.Up;

                    if(!WayBlocked(Directions.Down))
                        if(downValue != 0)
                            if(Helper.IsLargestNumber(downValue, upValue, leftValue, rightValue))
                                return Directions.Down;

                    if(!WayBlocked(Directions.Left))
                        if(leftValue != 0)
                            if(Helper.IsLargestNumber(leftValue, rightValue, downValue, upValue))
                                return Directions.Left;

                    if(!WayBlocked(Directions.Right))
                        if(rightValue != 0)
                            if(Helper.IsLargestNumber(rightValue, leftValue, upValue, downValue))
                                return Directions.Right;
                }
            }


//            if(!WayBlocked(Directions.Up)) {
//                if(Helper.IsSmallestNumber(upPercent, downPercent, rightPercent, leftPercent)) {
//                    if(upPercent < (_field.Curiosity / 100)) {
//                        return Directions.Up;
//                    }
//                }
//            }
//
//            if(!WayBlocked(Directions.Down)) {
//                if(Helper.IsSmallestNumber(downPercent, upPercent, rightPercent, leftPercent)) {
//                    if(downPercent < (_field.Curiosity / 100)) {
//                        return Directions.Down;
//                    }
//                }
//            }
//
//            if(!WayBlocked(Directions.Left)) {
//                if(Helper.IsSmallestNumber(leftPercent, upPercent, rightPercent, downPercent)) {
//                    if(leftPercent < (_field.Curiosity / 100)) {
//                        return Directions.Left;
//                    }
//                }
//            }
//
//            if(!WayBlocked(Directions.Right)) {
//                if(Helper.IsSmallestNumber(rightPercent, upPercent, leftPercent, downPercent)) {
//                    if(rightPercent < (_field.Curiosity / 100)) {
//                        return Directions.Right;
//                    }
//                }
//            }

            return Directions.None;
        }

        /// <summary>
        /// Checks if the way in the desired direction is blocked.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private bool WayBlocked(Directions direction) {
            if(_field.IsBlocked(
                                Helper.GetNewCoordVector2(
                                                          new Vector2Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y), direction))) {
                return true;
            }

            switch(direction) {
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

            if(double.IsNaN(value)) {
                value = 0;
                Debug.Log("Calculation Failed! NaN!");
            }

            int visits = CurrentQMatrix.QMatrix[_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y]
                .SetValue(direction, value);
            if(visits > _maxVisitValue) {
                _maxVisitValue = visits;
            }

            if(value > _maxWayValue) {
                _maxWayValue = value;
            }
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
            switch(direction) {
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

            switch(direction) {
                case Directions.Up:
                    //if(_characterPosition.CurrentPos.y + 1 > _field.Dimensions.y) break;
                    var posUp = new Vector2Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y + 1);
                    if(Helper.IsIndexOutOfArray(CurrentQMatrix.QMatrix, posUp.x, posUp.y)) return 0;
                    tempPoint = CurrentQMatrix.QMatrix[posUp.x, posUp.y];
                    break;
                case Directions.Down:
                    var posDown = new Vector2Int(_characterPosition.CurrentPos.x, _characterPosition.CurrentPos.y - 1);
                    if(Helper.IsIndexOutOfArray(CurrentQMatrix.QMatrix, posDown.x, posDown.y)) return 0;
                    tempPoint = CurrentQMatrix.QMatrix[posDown.x, posDown.y];
                    break;
                case Directions.Right:
                    var posRight = new Vector2Int(_characterPosition.CurrentPos.x + 1, _characterPosition.CurrentPos.y);
                    if(Helper.IsIndexOutOfArray(CurrentQMatrix.QMatrix, posRight.x, posRight.y)) return 0;
                    tempPoint = CurrentQMatrix.QMatrix[posRight.x, posRight.y];
                    break;
                case Directions.Left:
                    var posLeft = new Vector2Int(_characterPosition.CurrentPos.x - 1, _characterPosition.CurrentPos.y);
                    if(Helper.IsIndexOutOfArray(CurrentQMatrix.QMatrix, posLeft.x, posLeft.y)) return 0;
                    tempPoint = CurrentQMatrix.QMatrix[posLeft.x, posLeft.y];
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
