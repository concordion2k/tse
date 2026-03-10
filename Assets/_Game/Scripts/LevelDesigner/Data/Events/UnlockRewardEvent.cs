using UnityEngine;

namespace LevelDesigner
{
    /// <summary>
    /// Fires when a level is completed. Use to grant rewards, unlock content, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "UnlockRewardEvent", menuName = "Level Designer/Events/Unlock Reward")]
    public class UnlockRewardEvent : LevelEvent
    {
        [Tooltip("Identifier for the reward to grant (e.g. weapon ID, power-up key)")]
        public string rewardId;

        public override void Execute(LevelSettings context)
        {
            Debug.Log($"[LevelEvent] Unlock reward '{rewardId}' granted (level: {context?.levelName})");
            // Hook into reward/unlock system here when it exists
        }
    }
}
