using UnityEngine;

namespace de.chojo.WayFinder.Character {
    public class CharacterPosition {
        private Directions _lastDirection;

        public Vector3Int CurrentPos { get; set; }
    }
}