using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace GY.DamageTime
{
    public class ResetCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "Reset";
        public string Help => "";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string>{"gy.reset"};
        
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, DamageTime.Instance.Translate("command_invalid"), Color.red);
                return;
            }

            var target = UnturnedPlayer.FromName(command[0]);

            if (target == null)
            {
                UnturnedChat.Say(caller, DamageTime.Instance.Translate("player_not_found"), Color.red);
                return;
            }
            
            var data = PlayerSavedata.readData(target.SteamPlayer().playerID, "/Player/Time.dat");
            data.writeSingle("TotalPlayingTime", 0);
            
            UnturnedChat.Say(caller, DamageTime.Instance.Translate("time_reset", target.DisplayName), Color.cyan);
        }
    }
}