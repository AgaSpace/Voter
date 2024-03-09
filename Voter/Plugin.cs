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
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
                CommandManager.Dispose();

                _timer = null;
                Config = null;
            }
            base.Dispose(disposing);
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
    }
}
