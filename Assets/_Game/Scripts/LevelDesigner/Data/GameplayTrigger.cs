using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelDesigner
{
    public enum TriggerType
    {
        PathDistance,
        // KillCount,   // reserved for future use
        // TimeElapsed, // reserved for future use
    }

    /// <summary>
    /// Defines a condition that, when met during gameplay, fires a list of LevelEvents.
    /// Stored inline in LevelSettings — not a ScriptableObject.
    /// </summary>
    [Serializable]
    public class GameplayTrigger
    {
        [Tooltip("Label shown in the inspector")]
        public string label = "Trigger";

        public TriggerType triggerType = TriggerType.PathDistance;

        [Tooltip("Distance along the path at which this trigger fires")]
        [Min(0f)]
        public float pathDistance;

        /// <summary>
        /// Whether this trigger has fired this play session. Not persisted — resets each time the level loads.
        /// </summary>
        [NonSerialized]
        public bool hasTriggered;

        [Tooltip("Events to execute when this trigger fires")]
        public List<LevelEvent> events = new List<LevelEvent>();
    }
}
