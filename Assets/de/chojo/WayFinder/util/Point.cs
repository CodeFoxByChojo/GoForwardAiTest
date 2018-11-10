using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace de.chojo.WayFinder.util {
    public class Point {
        private double _down = 0;
        private double _left = 0;
        private double _none = 0;
        private double _right = 0;
        private double _up = 0;

        private long _visits = 0;

        public void SetValue(Directions direction, double value) {
            _visits++;
            
            if (double.IsNaN(value)) {
                value = 0;
            }

            switch (direction) {
                case Directions.Up:
                    _up = value;
                    break;
                case Directions.Down:
                    _down = value;
                    break;
                case Directions.Right:
                    _right = value;
                    break;
                case Directions.Left:
                    _left = value;
                    break;
                case Directions.none:
                    _none = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }

        public double GetValue(Directions direction) {
            switch (direction) {
                case Directions.Up:
                    return _up;
                case Directions.Down:
                    return _down;
                case Directions.Right:
                    return _right;
                case Directions.Left:
                    return _left;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }

        public Directions GetBestDirection() {
            if (_up > _down && _up > _right && _up > _left)
                return Directions.Up;
            if (_down > _up && _down > _right && _down > _left)
                return Directions.Down;
            if (_left > _up && _left > _down && _left > _right)
                return Directions.Left;
            if (_right > _up && _right > _down && _right > _left)
                return Directions.Right;
            return Directions.none;
        }

        public double GetValueSum() {
            return _up + _down + _left + _right;
        }

        public double GetBestValue() {
            List<double> temp = new List<double> {_up, _left, _down, _right, _none};
            return temp.Max();
        }

        public long Visits {
            get { return _visits; }
            set { _visits = value; }
        }
    }
}