using System;
using System.Collections.Generic;
using System.Timers;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Core.Utils;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace GY.DamageTime
{
    public class DamageTime : RocketPlugin<Config>
    {
        public static DamageTime Instance;
        private static readonly Dictionary<Player, Timer> UsersData = new Dictionary<Player, Timer>();
        private Config _cfg;

        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"need_to_play", "Вы не можете наносить урон еще {0}."},
            {"dest_play", "Вы наиграли необходимое кл-во времени, блокировка урона отключена."},
            {"command_invalid", "Команда введена неверно, используйте /reset [nick | steam]."},
            {"player_not_found", "Игрок не найден!"},
            {"time_reset", "Вы сбросили игровое время игрока {0}."}
        };

        protected override void Load()
        {
            Instance = this;
            _cfg = Configuration.Instance;
            DamageTool.onPlayerAllowedToDamagePlayer += OnDamageAllow;
            U.Events.OnPlayerConnected += EventOnPlayerConnected;
            U.Events.OnPlayerDisconnected += EventsOnPlayerDisconnected;
        }

        private static void OnDamageAllow(Player killer, Player victim, ref bool allow)
        {
            if (UsersData.ContainsKey(killer) || UsersData.ContainsKey(victim))
            {
                allow = false;
            }
        }

        private static void EventsOnPlayerDisconnected(UnturnedPlayer player)
        {
            UsersData[player.Player].Dispose();
            UsersData.Remove(player.Player);
        }

        private void EventOnPlayerConnected(UnturnedPlayer player)
        {
            var sPlayer = player.SteamPlayer().playerID;
            var data = PlayerSavedata.readData(sPlayer, "/Player/Time.dat");
            var totalPlayed = data.readSingle("TotalPlayingTime");

            if (totalPlayed >= _cfg.SecToDamage || player.HasPermission(_cfg.BypassPermission)) return;
            
            var seconds = _cfg.SecToDamage - totalPlayed;
            
            var timer = new Timer
            {
                Interval = seconds * 1000,
                AutoReset = false
            };

            timer.Elapsed += (sender, args) =>
            {
                (sender as Timer)?.Dispose();
                UsersData.Remove(player.Player);
                
                TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(player, Translate("dest_play"), Color.cyan));
            };
            
            UsersData.Add(player.Player, timer);
            UnturnedChat.Say(player, Translate("need_to_play", TimeSpan.FromSeconds(seconds).ToString("h'ч.' m'м.' s'с.'")), Color.red);
        }

        protected override void Unload()
        {
            DamageTool.onPlayerAllowedToDamagePlayer -= OnDamageAllow;
            U.Events.OnPlayerConnected -= EventOnPlayerConnected;
            U.Events.OnPlayerDisconnected -= EventsOnPlayerDisconnected;
        }
    }
}