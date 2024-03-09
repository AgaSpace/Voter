#region Using

using TerrariaServersAPI;
using TShockAPI;

#endregion

namespace Voter
{
    public static class HookManager
    {
        public static event Action<TSPlayer>? Reward;
        public static void OnReward(TSPlayer player) => Reward?.Invoke(player);

        /// <summary>
        /// First Item - old current, Second Item - old previous,
        /// Third Item - current, Fourth Item - previous.
        /// </summary>
        public static event Action<VoterList, VoterList, VoterList, VoterList>? TimerElapsed;
        public static void OnTimerElapsed(VoterList oldCurrent, VoterList oldPrevious, VoterList current, VoterList previous) =>
            TimerElapsed?.Invoke(oldCurrent, oldPrevious, current, previous);
    }
}
