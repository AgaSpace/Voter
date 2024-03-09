#region Using

using TShockAPI;
using TerrariaServersAPI;

#endregion

namespace Voter.Commands
{
    public class Reward : Command
    {
        public static readonly Reward Instance = new Reward();

        private Reward() : base(Config!.Settings.RewardCommand, Execute, "reward", "votereward", "votesreward", "claimreward")
        {
            HelpText = "Выдает ежедневную награду за голосование.\nGives out a daily reward for voting.";
        }

        private static async void Execute(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("Необходимо авторизоваться!");
                return;
            }
            if (!Config!.Settings.Reward)
            {
                args.Player.SendWarningMessage("Награда на сервере отключена!");
                return;
            }

            try
            {
                switch (await APIRequest.CheckUserHasVoted(Config!.Settings.Key, args.Player.Account.Name, Config!.Settings.Url))
                {
                    case CheckUserHasVoted.NotFound:
                        args.Player.SendWarningMessage("Вы не голосовали сегодня!");
                        break;
                    case CheckUserHasVoted.HasVoted:
                        try
                        {
                            if (await APIRequest.SetVoteAsClaimed(Config!.Settings.Key, args.Player.Account.Name, Config!.Settings.Url) == SetVoteAsClaimed.Claimed)
                            {
                                HookManager.OnReward(args.Player);
                                foreach (string command in Config!.Settings.RewardCommands)
                                    TShockAPI.Commands.HandleCommand(args.Player, command.Replace("%player%", $"tsi:{args.Player.Index}"));
                                args.Player.SendSuccessMessage("Спасибо за ваш голос! Мы действительно это ценим!");
                            }
                            else
                            {
                                args.Player.SendWarningMessage("Вы либо не голосовали сегодня, либо уже получали награду.");
                            }
                        }
                        catch (Exception ex)
                        {
                            args.Player.SendErrorMessage("Не удалось получить награду, обратитесь к разработчикам.");
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                        break;
                    case CheckUserHasVoted.HasVotedAndClaimed:
                        args.Player.SendWarningMessage("Вы уже получили свою награду на сегодня!");
                        break;
                }
            }
            catch (Exception ex)
            {
                args.Player.SendErrorMessage("Не удалось проверить ваш голос!");
                TShock.Log.ConsoleError(ex.ToString());
            }
        }
    }
}
