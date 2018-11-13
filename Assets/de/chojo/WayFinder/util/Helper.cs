using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace de.chojo.WayFinder.util {
    public static class Helper {
        /// <summary>
        /// Prints a Matrix with a title. No use anymore. Heatmap is better :P
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Title"></param>
        /// <returns></returns>
        public static string PrintMatrix(float[,] array, string Title) {
            var serializedArray = "";
            var longestNumber = 0;

            //Calculate longest Number
            for(var i = 0; i < array.GetLength(0); i++) {
                for(var j = 0; j < array.GetLength(1); j++) {
                    var number = array[i, j].ToString().Length;
                    if(number > longestNumber) longestNumber = number;
                }
            }

            if(array.GetLength(0) > longestNumber) longestNumber = array.GetLength(0);

            if(array.GetLength(1) > longestNumber) longestNumber = array.GetLength(1);

            if(Title.Length > longestNumber) longestNumber = Title.Length;

            //Write Array
            for(var i = 0; i < array.GetLength(0) + 1; i++) {
                for(var j = 0; j < array.GetLength(1) + 1; j++) {
                    if(i == 0 && j == 0) {
                        serializedArray = AddToStringAndFill(serializedArray, Title, longestNumber);
                        continue;
                    }

                    if(i == 0) {
                        serializedArray = AddToStringAndFill(serializedArray, (j - 1).ToString(), longestNumber);
                        continue;
                    }

                    if(j == 0) {
                        serializedArray = AddToStringAndFill(serializedArray, (i - 1).ToString(), longestNumber);
                        continue;
                    }

                    var number = array[i - 1, j - 1].ToString();
                    serializedArray = AddToStringAndFill(serializedArray, number, longestNumber);
                }

                serializedArray = string.Concat(serializedArray, "/n");
            }

            serializedArray = serializedArray.Replace("/n", Environment.NewLine);

            return serializedArray;
        }


        /// <summary>
        /// Fills a string with an amount of spaces
        /// </summary>
        /// <param name="OriginString"></param>
        /// <param name="StringToAdd"></param>
        /// <param name="FillTo"></param>
        /// <returns></returns>
        private static string AddToStringAndFill(string OriginString, string StringToAdd, int FillTo) {
            var fill = FillTo + 1 - StringToAdd.Length;

            for(var k = 0; k < fill; k++) StringToAdd = string.Concat(StringToAdd, " ");

            return string.Concat(OriginString, StringToAdd);
        }

        /// <summary>
        /// Calculates the Average of every number above 0
        /// </summary>
        /// <param name="List"></param>
        /// <returns></returns>
        public static double GetAverage(List<double> List) {
            double value = 0;
            double values = 0;
            foreach(var number in List) {
                if(number > 0) {
                    value += number;
                    values++;
                }
            }

            var result = value / values;
            if(double.IsNaN(result)) {
                result = 0;
            }

            return result;
        }

        public static BigInteger GetAverage(List<BigInteger> List) {
            BigInteger value = 0;
            BigInteger values = 0;
            foreach(BigInteger number in List) {
                if(number > 0) {
                    value += number;
                    values++;
                }
            }

            BigInteger result;
            if(value.IsZero || values.IsZero) {
                return BigInteger.Zero;
            }

            result = BigInteger.Divide(value, values);

            return result;
        }

        public static double GetMax(List<double> list) {
            if(list == null || list.Count == 0)
                return 0;
            double value = 0;
            foreach(var entry in list) {
                value += entry;
            }

            if(value == 0) {
                return 0;
            }
            return list.Max();
        }


        public static Color GetPercentAsColor(double value, double maxValue) {
            if(value == 0 || maxValue == 0) return GetPercentAsColor(0);
            return GetPercentAsColor((decimal)value / (decimal)maxValue);
        }

        public static Color GetPercentAsColor(double value, decimal maxValue) {
            if(maxValue == 0 || value == 0) return GetPercentAsColor(0);
            return GetPercentAsColor((decimal)value / maxValue);
        }

        public static Color GetPercentAsColor(decimal value, double maxValue) {
            if(value == 0 || maxValue == 0) return GetPercentAsColor(0);
            return GetPercentAsColor((decimal)value / (decimal)maxValue);
        }

        public static Color GetPercentAsColor(decimal value, decimal maxValue) {
            if(value == 0 || maxValue == 0) return GetPercentAsColor(0);
            return GetPercentAsColor(value / maxValue);
        }

        public static Color GetPercentAsColor(BigInteger value, BigInteger maxValue) {
            if(value.IsZero || maxValue.IsZero) return GetPercentAsColor(0);
            //Get the GCD
            var gcd = BigInteger.GreatestCommonDivisor(value, maxValue);
            //Divide with the gcd to get a small number
            BigInteger valueBig = BigInteger.Divide(value, gcd);
            BigInteger maxValueBig = BigInteger.Divide(maxValue, gcd);

            float valueAsSmallFloat = StringToFloat(valueBig.ToString(), 0);
            float maxValueAsSmallFloat = StringToFloat(maxValueBig.ToString(), 0);

            return GetPercentAsColor((decimal)(valueAsSmallFloat / maxValueAsSmallFloat));
        }

        public static float GetPercentfromBigInt(BigInteger value, BigInteger maxValue) {
            if(value.IsZero || maxValue.IsZero) return 0;
            //Get the GCD
            var gcd = BigInteger.GreatestCommonDivisor(value, maxValue);
            //Divide with the gcd to get a small number
            BigInteger valueBig = BigInteger.Divide(value, gcd);
            BigInteger maxValueBig = BigInteger.Divide(maxValue, gcd);

            float valueAsSmallFloat = StringToFloat(valueBig.ToString(), 0);
            float maxValueAsSmallFloat = StringToFloat(maxValueBig.ToString(), 0);

            return valueAsSmallFloat / maxValueAsSmallFloat;

        }


        private static Color GetPercentAsColor(Decimal percent) {
            float per;
            if(percent != null) {
                per = (float)percent;
            }
            else {
                per = 0;
            }

            var greyToBlue = new Gradient();
            var blueToCyan = new Gradient();
            var cyanToGreen = new Gradient();
            var greenToYellow = new Gradient();
            var yellowToRed = new Gradient();

            var colorKey0 = new GradientColorKey[2];
            colorKey0[0].color = Color.grey;
            colorKey0[0].time = 0.0f;
            colorKey0[1].color = Color.blue;
            colorKey0[1].time = 0.001f;
            
            var colorKey1 = new GradientColorKey[2];
            colorKey1[0].color = Color.blue;
            colorKey1[0].time = 0.0f;
            colorKey1[1].color = Color.cyan;
            colorKey1[1].time = 0.001f;

            var colorKey2 = new GradientColorKey[2];
            colorKey2[0].color = Color.cyan;
            colorKey2[0].time = 0.001f;
            colorKey2[1].color = Color.green;
            colorKey2[1].time = 0.33f;

            var colorKey3 = new GradientColorKey[2];
            colorKey3[0].color = Color.green;
            colorKey3[0].time = 0.33f;
            colorKey3[1].color = Color.yellow;
            colorKey3[1].time = 0.66f;

            var colorKey4 = new GradientColorKey[2];
            colorKey4[0].color = Color.yellow;
            colorKey4[0].time = 0.66f;
            colorKey4[1].color = Color.red;
            colorKey4[1].time = 1f;

            var alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1f;
            alphaKey[0].time = 1f;
            alphaKey[1].alpha = 1f;
            alphaKey[1].time = 1f;

            greyToBlue.SetKeys(colorKey0, alphaKey);
            blueToCyan.SetKeys(colorKey1, alphaKey);
            cyanToGreen.SetKeys(colorKey2, alphaKey);
            greenToYellow.SetKeys(colorKey3, alphaKey);
            yellowToRed.SetKeys(colorKey4, alphaKey);

            if(per <= 0.000001f) {
                return greyToBlue.Evaluate(per);
            }

            if ( per > 0.000001f && per <= 0.25f) {
                return blueToCyan.Evaluate(per);
            }

            if(per > 0.25f && per <= 0.50f) {
                return cyanToGreen.Evaluate(per);
            }

            if(per > 0.50f && per <= 0.75f) {
                return greenToYellow.Evaluate(per);
            }

            return yellowToRed.Evaluate(per);
        }

        public static List<QMatrixMemory> AddListToList(List<QMatrixMemory> firstList, List<QMatrixMemory> secondList) {
            var value = new List<QMatrixMemory>();
            foreach(var entry in firstList) {
                value.Add(entry);
            }

            if(secondList == null) return value;
            foreach(var entry in secondList) {
                value.Add(entry);
            }

            return value;
        }

        public static TextMeshProUGUI GetObjectWithTag(TextMeshProUGUI[] texts, string tag) {
            foreach(var entry in texts) {
                if(entry.tag.ToLower().Equals(tag.ToLower())) {
                    return entry;
                }
            }

            return null;
        }

        public static GameObject GetObjectWithTag(IEnumerable<GameObject> gameObjects, string tag) {
            foreach(var entry in gameObjects) {
                if(entry.tag.Equals(tag)) {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts a string to float. Rounds to digits.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="decimalPoints"></param>
        /// <returns>String as float if parse is valid. 0 if not.</returns>
        public static float StringToFloat(string text, int decimalPoints) {
            float result;

            text = text.Replace(",", ".");
            if(float.TryParse(text, out result)) {
                result = (float)Math.Round(result, decimalPoints);
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Converts a string to float. Rounds to two digits. 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>String as float if parse is valid. 0 if not.</returns>
        public static float StringToFloat(string text) {
            float result;

            text = text.Replace(",", ".");
            if(float.TryParse(text, out result)) {
                result = (float)Math.Round(result, 2);
                return result;
            }

            return 0;
        }

        /// <summary>
        /// Converts string to Integer.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>String as int if parse is valid. 0 if not.</returns>
        public static int StringToInt(string text, int remove) {
            int result;
            text = text.Replace("\u200B", "");
            int.TryParse(text, out result);
            return result;
        }

        public static string RemoveEnd(string text, int remove) {
            string value = "";
            for(int i = 0; i < text.Length - remove; i++) {
                value = string.Concat(value, text[i]);
                Debug.Log(text[i]);
            }

            return value;
        }

        public static float ClampFloat(float value, float min, float max, out bool changed) {
            changed = true;
            if(value > max) return max;
            if(value < min) return min;
            changed = false;
            return value;
        }

        public static int ClampInt(int value, int min, int max, out bool changed) {
            changed = true;
            if(value > max) return max;
            if(value < min) return min;
            changed = false;
            return value;
        }

        public static float ClampFloat(float value, float min, float max) {
            if(value > max) return max;
            if(value < min) return min;
            return value;
        }

        public static int ClampInt(int value, int min, int max) {
            if(value > max) return max;
            if(value < min) return min;
            return value;
        }

        public static Vector2Int GetNewCoordVector2(Vector2Int pos, Directions? direction) {
            switch(direction) {
                case Directions.Up:
                    return new Vector2Int(pos.x, pos.y + 1);
                case Directions.Down:
                    return new Vector2Int(pos.x, pos.y - 1);
                case Directions.Right:
                    return new Vector2Int(pos.x + 1, pos.y);
                case Directions.Left:
                    return new Vector2Int(pos.x - 1, pos.y);
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }

        public static Vector3Int GetNewCoordVector3(Vector3Int pos, Directions direction) {
            switch(direction) {
                case Directions.Up:
                    return new Vector3Int(pos.x, pos.y + 1, pos.z);
                case Directions.Down:
                    return new Vector3Int(pos.x, pos.y - 1, pos.z);
                case Directions.Right:
                    return new Vector3Int(pos.x + 1, pos.y, pos.z);
                case Directions.Left:
                    return new Vector3Int(pos.x - 1, pos.y, pos.z);
                default:
                    throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }
    }
}





public enum Directions {
    Up,
    Down,
    Right,
    Left,
    None
}

public enum HeatMapType {
    BestWay,
    Visits
}

public static class Data {
    private static readonly string _uiUpdateTag = "UiUpdateText";

    public static string UiUpdateTag {
        get {return _uiUpdateTag;}
    }
}
