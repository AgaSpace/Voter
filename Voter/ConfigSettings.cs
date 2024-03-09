namespace Voter
{
    public class ConfigSettings
    {
        public string Key = "xxx";
        public string? Url = null;

        public bool Reward = false;
        public string[] RewardCommands = new string[0];
        public double ReductionCoefficient = 0.67f;

        public int VoteCheckIntervalInSeconds = 60 * 5; // 5 min

        public Dictionary<string, byte> VoterGroupList = new Dictionary<string, byte>();

        public string VotesCommand = "voter";
        public string VoterGroupUpdateIgnore = "voter.ignore";
        public string OtherVotesSubCommand = "voter.other";
        public string RewardCommand = "voter.reward";

    }
}
