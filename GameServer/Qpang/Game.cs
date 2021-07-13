﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Qserver.GameServer.Network;
using Qserver.GameServer.Network.Managers;
using Qserver.Util;

namespace Qserver.GameServer.Qpang
{
    public enum GameState
    {
        LOGIN = 1,
        WAITROOM,
        GAME,
        LOADING,
        TUTORIAL,
        UI_SQUARE_STATE
    }

    public enum GameUIState
    {
        NONE_STATE = 0,
        LOGO_STATE,
        LOGIN_STATE,
        WAITROOM_STATE,
        LOAD_STATE,
        GAME_STATE,
        MATCH_STATE,
        STRUGGLE_STATE = 7,
        TUTORIAL_STATE = 9,
        PRACTICE_STATE = 8,
        SQUARE_STATE = 11,
        PVE_STATE = 10
    }

    public class Game
    {
        public static Game Instance;

        private RoomServer _roomServer;
        private BanManager _banManager;
        private ChannelManager _channelManager;
        private ShopManager _shopManager;
        private SquareManager _squareManager;
        private CacheManager _cacheManager;
        private ChatManager _chatManager;
        private RoomManager _roomManager;
        private WeaponManager _weaponManager;
        private SpawnManager _spawnManager;
        private SkillManager _skillManager;
        private AchievementManager _achievementManager;
        private LevelManager _levelManger;
        private CraneManager _craneManager;

        private Leaderboard _leaderboard;

        private LobbyServer _lobbyServer; // lobby?
        private SquareServer _squareServer;

        private object _lock;

        private Dictionary<uint, Player> _players;
        private Dictionary<string, Player> _platersByName;

        public BanManager BanManager
        {
            get { return this._banManager; }
        }
        public ChannelManager ChannelManager
        {
            get { return this._channelManager; }
        }
        public ShopManager ShopManager
        {
            get { return this._shopManager; }
        }
        public SquareManager SquareManager
        {
            get { return this._squareManager; }
        }
        public CacheManager CacheManager
        {
            get { return this._cacheManager; }
        }
        public ChatManager ChatManager
        {
            get { return this._chatManager; }
        }
        public RoomManager RoomManager
        {
            get { return this._roomManager; }
        }
        public WeaponManager WeaponManager
        {
            get { return this._weaponManager; }
        }
        public SpawnManager SpawnManager
        {
            get { return this._spawnManager; }
        }
        public SkillManager SkillManager
        {
            get { return this._skillManager; }
        }
        public AchievementManager AchievementManager
        {
            get { return this._achievementManager; }
        }
        public LevelManager LevelManager
        {
            get { return this._levelManger; }
        }
        public CraneManager CraneManger
        {
            get { return this._craneManager; }
        }
        public Leaderboard Leaderboard
        {
            get { return this._leaderboard; }
        }

        public Game()
        {
            this._lock = new object();

            this._players = new Dictionary<uint, Player>();

            this._lobbyServer = new LobbyServer();
            this._lobbyServer.Server.Start();
            this._lobbyServer.Server.StartConnectionThreads();

            this._squareServer = new SquareServer();
            this._squareServer.Server.Start();
            this._squareServer.Server.StartConnectionThreads();

            this._channelManager = new ChannelManager();
            this._shopManager = new ShopManager();
            this._squareManager = new SquareManager();
            this._chatManager = new ChatManager();
            this._weaponManager = new WeaponManager();
            this._spawnManager = new SpawnManager();
            this._skillManager = new SkillManager();
            this._levelManger = new LevelManager();
            this._craneManager = new CraneManager();
            this._leaderboard = new Leaderboard();
            this._roomServer = new RoomServer();

            Instance = this;
        }

        public void tick()
        {
            while(true)
            {
                try
                {

                }catch(Exception e)
                {
                    Log.Message(LogType.ERROR, "Fatal error: " + e.ToString());
                }
                Thread.Sleep(1);
            }
        }
        public void RemoveClient(Player player)
        {
            if (player == null)
                return;

            lock (this._lock)
            {
                _players.Remove(player.PlayerId);
            }
        }

        public Player CreatePlayer(ConnServer conn, uint playerId)
        {
            Player player = new Player(playerId);

            player.LobbyConnection = conn;
            conn.Player = player;

            lock(this._lock)
            {
                if(this._players.ContainsKey(player.PlayerId))
                {
                    this._players[player.PlayerId].SendLobby(LobbyManager.Instance.DuplicateLogin());
                    this._players[player.PlayerId].Close();
                }
                
                // cache manger

                this._players[player.PlayerId] = player;
                // this._playersName
            }

            return player;
        }

        public Player GetPlayer(uint playerId)
        {
            lock(this._lock)
            {
                return this._players[playerId];
            }
        }
        public Player GetPlayer(string name)
        {
            lock (this._lock)
            {
                //foreach(var p in this._players)
                //    if(p.Value.)
            }
            return null;
        }

        // TODO: event handles to kick player on connection close

    }
}
