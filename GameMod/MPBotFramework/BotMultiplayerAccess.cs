using Overload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GameMod.MPBotFramework
{
    public class BotMultiplayerAccess
    {
        enum MultiplayerState { NONE, INIT_REQUEST_PLAYER_ID, SETUP_GAME, START_GAME, IN_GAME }

        private static MultiplayerState CurrentMultiplayerState = MultiplayerState.NONE;
        private static MultiplayerState NextMultiplayerState = MultiplayerState.NONE;


        private static void RequestNextMultiplayerState(MultiplayerState nextState)
        {
            CurrentMultiplayerState = nextState;
        }


        // use delegate to handle when done?
        public static void HostAndEnterGame()
        {
            RequestNextMultiplayerState(MultiplayerState.INIT_REQUEST_PLAYER_ID);
        }

        private static void RequestPlayerId()
        {
            NetworkMatch.SetPlayerId("00000000-0000-0000-0000-000000000000");
            Action<string, string> callback = HandleGetPlayerId;
            NetworkMatch.GetMyPlayerId(PilotManager.PilotName, callback);
        }

        private static void HandleGetPlayerId(string error, string player_id)
        {
            if (error != null)
            {
                NetworkMatch.SetPlayerId("00000000-0000-0000-0000-000000000000");
            }
            else
            {
                NetworkMatch.SetPlayerId(player_id);
                RequestNextMultiplayerState(MultiplayerState.SETUP_GAME);
            }
        }

        private static void SetupGame()
        {
            MenuManager.m_mp_lan_match = true;
            MenuManager.m_mp_private_match = true;
            //UIManager.DestroyAll(false);
            //MenuManager.ChangeMenuState(MenuState.MP_LOCAL_MATCH, false);

            MenuManager.m_updating_pm_settings = false;

            //MenuManager.ChangeMenuState(MenuState.MP_PRE_MATCH_MENU, false);

            MPInternet.Enabled = true;
            MPInternet.MenuPassword = "167.71.41.143"; // Frankfurt 1
            MPInternet.MenuIPAddress = "167.71.41.143";
            MPInternet.ServerAddress = IPAddress.Parse("167.71.41.143");

            InternetMatchAccessor.Enabled = true;
            InternetMatchAccessor.ServerAddress = MPInternet.ServerAddress;

            NetworkMatch.SetNetworkGameClientMode(NetworkMatch.NetworkGameClientMode.LocalLAN);

            MenuManager.m_mp_status = Loc.LS("CREATING INTERNET MATCH");

            PrivateMatchDataMessage pmd = BotMultiplayerAccess.GetPrivateMatchDataMessage();
            NetworkMatch.StartPrivateLobby(pmd);
            UIManager.DestroyAll(false);

            // TODO: likely have to wait for completing the switch to the lobby

            RequestNextMultiplayerState(MultiplayerState.START_GAME);
        }

        private static void StartGame()
        {
            MenuManager.m_mp_ready_to_start = true;
            //MenuManager.UIPulse(1f);
            //MenuManager.PlayCycleSound(1f, 1f);
            NetworkMatch.SetLobbyStartNowVote(MenuManager.m_mp_ready_to_start);
            MenuManager.m_mp_ready_vote_timer = 1.25f;

            RequestNextMultiplayerState(MultiplayerState.IN_GAME);
        }

        public static void Update() 
        {
            if (NextMultiplayerState != CurrentMultiplayerState)
            {
                TransitionToNextMultiplayerState();
            }
        }

        public static void TransitionToNextMultiplayerState() 
        {
            CurrentMultiplayerState = NextMultiplayerState;

            switch (NextMultiplayerState)
            {
                case MultiplayerState.INIT_REQUEST_PLAYER_ID:
                    RequestPlayerId();
                    break;
                case MultiplayerState.SETUP_GAME:
                    SetupGame();
                    break;
                case MultiplayerState.START_GAME:
                    StartGame();
                    break;
            }
        }


        public static PrivateMatchDataMessage GetPrivateMatchDataMessage()
        {
            PrivateMatchDataMessage privateMatchDataMessage = new PrivateMatchDataMessage();
            privateMatchDataMessage.m_name = "spark"; // PilotManager.PilotName;
                                                      //privateMatchDataMessage.m_password = "127.0.0.1"; //MenuManager.mms_match_password;
            privateMatchDataMessage.m_password = "167.71.41.143"; /*Frankfurt 1*/ //MenuManager.mms_match_password;
            privateMatchDataMessage.m_match_mode = MatchMode.ANARCHY; //MenuManager.mms_mode;
            privateMatchDataMessage.m_num_players_to_start = 1;
            privateMatchDataMessage.m_max_players_for_match = 1; //MenuManager.mms_max_players;
            privateMatchDataMessage.m_addon_level_name_hash = "QUICK1.MP:9667D24A"; //string.Empty;

            privateMatchDataMessage.m_level_num = -1;

            /*
if (GameManager.MultiplayerMission.IsLevelAnAddon(MenuManager.mms_level_num))
{
if (isOnlineMatch)
{
    privateMatchDataMessage.m_level_num = 0;
}
else
{
    privateMatchDataMessage.m_level_num = -1;
    privateMatchDataMessage.m_addon_level_name_hash = GameManager.MultiplayerMission.GetAddOnLevelIdStringHash(MenuManager.mms_level_num);
}
}
else
{
privateMatchDataMessage.m_level_num = MenuManager.mms_level_num;
}
*/
            privateMatchDataMessage.m_time_limit = MatchTimeLimit.NONE;//MenuManager.mms_time_limit;
            privateMatchDataMessage.m_score_limit = 0; //MenuManager.mms_score_limit;
            privateMatchDataMessage.m_respawn_time_seconds = 2; //MenuManager.mms_respawn_time;
            privateMatchDataMessage.m_respawn_shield_time_seconds = 2; //MenuManager.mms_respawn_invuln;
            privateMatchDataMessage.m_friendlyfire = false; //(MenuManager.mms_friendly_fire > 0);
            privateMatchDataMessage.m_show_enemy_names = MatchShowEnemyNames.NORMAL; //MenuManager.mms_show_names;
            privateMatchDataMessage.m_turn_speed_limit = 2; //MenuManager.mms_turn_speed_limit;
            privateMatchDataMessage.m_powerup_spawn = 2; // MenuManager.mms_powerup_spawn;
            privateMatchDataMessage.m_powerup_initial = 2; // MenuManager.mms_powerup_initial;
            privateMatchDataMessage.m_powerup_big_spawn = 1; //MenuManager.mms_powerup_big_spawn;
            privateMatchDataMessage.m_powerup_filter_bitmask = 0xffff; //RUtility.BoolArrayToBitmask(MenuManager.mms_powerup_filter);
            privateMatchDataMessage.m_force_loadout = 0;  // MenuManager.mms_force_loadout;
            privateMatchDataMessage.m_force_m1 = MissileType.FALCON;  //MenuManager.mms_force_m1;
            privateMatchDataMessage.m_force_m2 = MissileType.NUM; // MenuManager.mms_force_m2;
            privateMatchDataMessage.m_force_modifier1 = 4; //MenuManager.mms_force_modifier1;
            privateMatchDataMessage.m_force_modifier2 = 4; //MenuManager.mms_force_modifier2;
            privateMatchDataMessage.m_force_w1 = WeaponType.IMPULSE; //MenuManager.mms_force_w1;
            privateMatchDataMessage.m_force_w2 = WeaponType.NUM; //MenuManager.mms_force_w2;

            return privateMatchDataMessage;
        }

    }
}
