using System.Numerics;
using System.Runtime.Remoting.Messaging;

namespace de.chojo.WayFinder.util {
    public struct Visit {
        public int Up;
        public int Down;
        public int Left;
        public int Right;

    }

    public class Visits {
        public int Right {get; set;}
        public int Up {get; set;}
        public int Down {get; set;}
        public int Left {get; set;}

        public void SetValues(Visit visit) {
            Up = visit.Up;
            Down = visit.Down;
            Left = visit.Left;
            Right = visit.Right;
        }

        public void AddValues(Visit visit) {
            Up += visit.Up;
            Down += visit.Down;
            Left += visit.Left;
            Right += visit.Right;
        }

        public Visit GetValues() {
            var visits = new Visit {Up = Up, Down = Down, Left = Left, Right = Right};
            return visits;
        }

        public int GetValueSum() {
            return Up + +Down + Left + Right;
        }

        public Directions GetDirectionWithLowestVistis() {
            if(Helper.IsSmallestNumber(Up, Down, Left, Right))
                return Directions.Up;
            if(Helper.IsLargestNumber(Down, Up, Left, Right))
                return Directions.Down;
            if(Helper.IsSmallestNumber(Left, Right, Down,Up))
                return Directions.Left;
            if(Helper.IsSmallestNumber(Right, Left, Up, Down))
                return Directions.Right;
            return Directions.None;
        }
        
        public Directions GetDirectionWithLowestVistis(out int visits) {
            visits = 0;
            if(Helper.IsSmallestNumber(Up, Down, Left, Right)) {
                visits = Up;
                return Directions.Up;
            }
            if(Helper.IsLargestNumber(Down, Up, Left, Right)) {
                visits = Down;
                return Directions.Down;
            }
            if(Helper.IsSmallestNumber(Left, Right, Down, Up)) {
                visits = Left;
                return Directions.Left;
            }
            if(Helper.IsSmallestNumber(Right, Left, Up, Down)) {
                visits = Right;
                return Directions.Right;
            }
            return Directions.None;
        }



    }


}
