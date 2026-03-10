using UnityEngine;

namespace LevelDesigner
{
    /// <summary>
    /// Abstract base for all level events. Create concrete types by subclassing
    /// and adding [CreateAssetMenu] to the subclass.
    /// </summary>
    public abstract class LevelEvent : ScriptableObject
    {
        [Tooltip("Human-readable label for this event (editor only)")]
        public string eventLabel = "Level Event";

        /// <summary>
        /// Called by LevelProgressionManager when this event fires.
        /// </summary>
        public abstract void Execute(LevelSettings context);
    }
}
