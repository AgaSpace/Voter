#region Using

global using static Voter.VoterPlugin;

using System.Timers;

using Terraria;
using TerrariaApi.Server;

using TShockAPI;
using TShockAPI.Hooks;
using TShockAPI.Configuration;

using TerrariaServersAPI;

using Voter.Commands;
using TShockAPI.DB;

#endregion

namespace Voter
{
    [ApiVersion(2, 1)]
    public class VoterPlugin : TerrariaPlugin
    {
        #region Data

        public override string Author => "Zoom L1";
        public override string Name => "Voter";
        public override Version Version => new Version(1, 0);
        public VoterPlugin(Main game) : base(game) { }

        public static VoterList Current, Previous;
        public static ConfigFile<ConfigSettings>? Config;
        private static System.Timers.Timer? _timer;

        #endregion
        #region Initialize

        public override void Initialize()
        {
            OnReload();
            GeneralHooks.ReloadEvent += OnReload;
            CommandManager.Initialize();

            _timer = new System.Timers.Timer(Config!.Settings.VoteCheckIntervalInSeconds * 1000)
            {
                AutoReset = true,
                Enabled = true
            };
            _timer.Elapsed += OnTimerElapsed;
            OnTimerElapsed(null, null!);

            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
                CommandManager.Dispose();

                PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;

                _timer = null;
                Config = null;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region OnPlayerPostLogin

        void OnPlayerPostLogin(PlayerPostLoginEventArgs args)
        {
            Group? group = SelectGroup(args.Player);
            if (group != null)
                args.Player.Group = SelectGroup(args.Player);
        }

        #endregion
        #region OnTimerElapsed

        async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                VoterList current = Current, 
                          previous = Previous;

                Current = await APIRequest.GetVoterListAsync(Config!.Settings.Key, VoterListMonth.Current, url: Config!.Settings.Url);
                Previous = await APIRequest.GetVoterListAsync(Config!.Settings.Key, VoterListMonth.Previous, url: Config!.Settings.Url);

                HookManager.OnTimerElapsed(current, previous, Current, Previous);

                TShock.Players.Where(i => i?.Active == true).ForEach(i =>
                {
                    Group? group = SelectGroup(i);
                    if (group != null && i.Group != group)
                    {
                        i.Group = SelectGroup(i);
                        i.SendInfoMessage("Ваша группа обновлена, ведь произошёл переподсчёт голосов.");
                    }
                });
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError($"[{Name}] OnTimerElapsed // {ex}");
            }
        }

        #endregion
        #region OnReload

        void OnReload(ReloadEventArgs? args = null)
        {
            string path = Path.Combine(TShock.SavePath, $"{Name}.json");
            Config = new ConfigFile<ConfigSettings>();
            Config.Read(path, out bool write);
            if (write)
                Config.Write(path);

            args?.Player.SendInfoMessage($"[{Name}] конфиг перезагружен. Если вы изменяли разрешения, то вы должны перезапустить сервер.");
        }

        #endregion

        #region 

        public static Group? SelectGroup(TSPlayer player) => SelectGroup(player.Account?.Name ?? player.Name, player.Group);
        public static Group? SelectGroup(UserAccount account) => SelectGroup(account, TShock.Groups.GetGroupByName(account.Group));
        public static Group? SelectGroup(UserAccount account, Group group) => SelectGroup(account.Name, group);
        public static Group? SelectGroup(string playerName, Group? group)
        {
            (double, double, double, double) pair = votescount(playerName);
            if (group?.HasPermission(Config!.Settings.VoterGroupUpdateIgnore) != false) // null or true
            {
                string voterGroup = Config!.Settings.VoterGroupList
                    .Where(i => pair.Item4 >= i.Value)
                    .OrderBy(i => i.Value)
                    .LastOrDefault().Key;
                return TShock.Groups.GetGroupByName(voterGroup);
            }
            return null;
        }

        #endregion

        #region votescount

        /// <summary>
        /// votes count
        /// </summary>
        /// <param name="playerName">player name. case insensitive</param>
        /// <returns>current, previous, flame, result</returns>
        internal static (double, double, double, double) votescount(string playerName)
        {
            int Selector(VoterList.Voter voter) => int.Parse(voter.Votes);
            bool Predicate(VoterList.Voter voter) => voter.Nickname.ToLower() == playerName.ToLower();

            double current = Current.Voters.Where(Predicate).Sum(Selector),
                    previous = Previous.Voters.Where(Predicate).Sum(Selector) * Config!.Settings.ReductionCoefficient,
                    flame = Math.Ceiling(current * (1 - Config.Settings.ReductionCoefficient)) + previous;

            return (current, previous, flame, current + previous);
        }

        #endregion
    }
}
