using System.Collections.Generic;
using UnityEngine;

namespace LevelDesigner
{
    /// <summary>
    /// Defines the ordered sequence of levels for the game.
    /// Create one asset at Assets/_Game/LevelData/LevelSequence.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelSequence", menuName = "Level Designer/Level Sequence")]
    public class LevelSequence : ScriptableObject
    {
        [Tooltip("Ordered list of levels. Index 0 = first level played.")]
        public List<LevelSettings> levels = new List<LevelSettings>();

        public int LevelCount => levels.Count;

        /// <summary>Returns null if index is out of range.</summary>
        public LevelSettings GetLevel(int index)
        {
            return IsValidIndex(index) ? levels[index] : null;
        }

        public bool IsValidIndex(int index) => index >= 0 && index < levels.Count;

        /// <summary>
        /// Returns the Unity scene name to load for a given level index.
        /// Currently all levels share the "level1" gameplay scene; add per-level
        /// scene names here when needed.
        /// </summary>
        public string GetSceneName(int index) => "level1";
    }
}
