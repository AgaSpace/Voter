#region Using

using TShockAPI;
using TerrariaServersAPI;

#endregion

namespace Voter.Commands
{
    public class Votes : Command
    {
        public static readonly Votes Instance = new Votes();
        private Votes() : base(Config!.Settings.VotesCommand, Execute, "votes")
        {
            HelpText = "Определяет количество ваших голосов.\nDetermines the number of your votes.";
        }
        
        private static void Execute(CommandArgs args)
        {
            if (!args.Player.IsLoggedIn)
            {
                args.Player.SendErrorMessage("Необходимо авторизоваться.");
                return;
            }

            string playerName = args.Parameters.ElementAtOrDefault(0) ?? args.Player.Account.Name;
            bool isMe = playerName == args.Player.Account.Name;

            // current, previous, flame, result
            (double, double, double, double) pair = votescount(playerName);

            string line;
            if (pair.Item4 == 0)
                line = $"{(isMe ? "Вы" : "Игрок")} еще ни разу не проголосовал{(isMe ? "и" : "")} за сервер{(isMe ? "!\r\nПерейдите на сайт http://terraz.ru/vote, введите своё имя в поле NickName, введите капчу и нажмите Vote." : ".")}";
            else if (pair.Item4 == 1)
                line = $"У {(isMe ? "вас" : "игрока")} одно очко.";
            else if (pair.Item4 % 10 == 1)
                line = $"У {(isMe ? "вас" : "игрока")} [c/00FF00:{pair.Item4}] очко. В конце месяца [c/FF0000:{pair.Item3}] сгорит.";
            else if (pair.Item4 < 5 || (pair.Item4 % 10 > 1 && pair.Item4 % 10 < 5))
                line = $"У {(isMe ? "вас" : "игрока")} [c/00FF00:{pair.Item4}] очка. В конце месяца [c/FF0000:{pair.Item3}] сгорит.";
            else
                line = $"У {(isMe ? "вас" : "игрока")} [c/00FF00:{pair.Item4}] очков. В конце месяца [c/FF0000:{pair.Item3}] сгорит.";
            args.Player.SendInfoMessage(line);
        }

        internal static (double, double, double, double) votescount(string playerName)
        {
            int Selector(VoterList.Voter voter) => int.Parse(voter.Votes);
            bool Predicate(VoterList.Voter voter) => voter.Nickname.ToLower() == playerName.ToLower();

            double current = Current.Voters.Where(Predicate).Sum(Selector),
                    previous = Previous.Voters.Where(Predicate).Sum(Selector) * Config!.Settings.ReductionCoefficient,
                    flame = Math.Ceiling(current * (1 - Config.Settings.ReductionCoefficient)) + previous;

            return (current, previous, flame, current + previous);
        }
    }
}
