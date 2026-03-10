using UnityEngine;

namespace LevelDesigner
{
    /// <summary>
    /// A gameplay event that fires mid-level (e.g. boss spawn, cutscene, dialogue).
    /// </summary>
    [CreateAssetMenu(fileName = "GameplayTriggerEvent", menuName = "Level Designer/Events/Gameplay Trigger")]
    public class GameplayTriggerEvent : LevelEvent
    {
        [Tooltip("Key identifying what should happen (e.g. 'boss_spawn', 'cutscene_intro')")]
        public string triggerKey;

        public override void Execute(LevelSettings context)
        {
            Debug.Log($"[LevelEvent] Gameplay trigger '{triggerKey}' fired (level: {context?.levelName})");
            // Hook into gameplay event system here when it exists
        }
    }
}
