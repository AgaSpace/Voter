#region Using

using TShockAPI;

#endregion

namespace Voter.Commands
{
    static class CommandManager
    {
        public static readonly IReadOnlyCollection<Command> Commands = new Command[]
        {
            Votes.Instance, Reward.Instance
        };

        public static void Initialize()
        {
            TShockAPI.Commands.ChatCommands.AddRange(Commands);
        }
        public static void Dispose()
        {
            Commands.ForEach(c => TShockAPI.Commands.ChatCommands.Remove(c));
        }
    }
}
