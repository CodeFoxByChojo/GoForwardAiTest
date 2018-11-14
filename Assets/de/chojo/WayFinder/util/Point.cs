using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Numerics;

namespace de.chojo.WayFinder.util {
    public class Point {
        private double _down = 0;
        private double _left = 0;
        private double _none = 0;
        private double _right = 0;
        private double _up = 0;

        private Visits _visits = new Visits();

        public int SetValue(Directions direction, double value) {
            if(double.IsNaN(value)) {
                value = 0;
            }

            int visitAmount = 0;

            if(Math.Abs(value) > 0.000001) {
                visitAmount = 1;
            }

            switch(direction) {
                case Directions.Up:
                    _up = value;
                    return _visits.Up += visitAmount;
                case Directions.Down:
                    _down = value;
                    return _visits.Up += visitAmount;
                case Directions.Right:
                    _right = value;
                    return _visits.Right += visitAmount;
                case Directions.Left:
                    _left = value;
                    return _visits.Left += visitAmount;
                case Directions.None:
                    _none = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }

            return 0;
        }

        public double GetValue(Directions direction) {
            switch(direction) {
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
            if(Helper.IsLargestNumber(_up, _down, _right, _left))
                return Directions.Up;
            if(Helper.IsLargestNumber(_down, _up, _right, _left))
                return Directions.Down;
            if(Helper.IsLargestNumber(_left, _up, _down, _right))
                return Directions.Left;
            if(Helper.IsLargestNumber(_right, _up,_down, _left))
                return Directions.Right;
            return Directions.None;
        }

        public Directions GetLowestValueDirection() {
            if(Helper.IsSmallestNumber(_up, _down, _right, _left))
                return Directions.Up;
            if(Helper.IsSmallestNumber(_down, _up, _right, _left))
                return Directions.Down;
            if(Helper.IsSmallestNumber(_left, _up, _down, _right))
                return Directions.Left;
            if(Helper.IsSmallestNumber(_right, _up,_down, _left))
                return Directions.Right;
            return Directions.None;

        }

        public double GetValueSum() {
            return _up + _down + _left + _right;
        }

        public double GetBestValue() {
            List<double> temp = new List<double> {_up, _left, _down, _right, _none};
            return temp.Max();
        }

        public Visits Visits {
            get {return _visits;}
            set {_visits = value;}
        }
    }
}
