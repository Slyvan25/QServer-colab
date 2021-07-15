﻿using System;
using System.Collections.Generic;
using System.Text;
using Qserver.GameServer.Network.Managers;

namespace Qserver.GameServer.Qpang
{
    public class InventoryManager
    {
        private Player _player;
        private object _lock;

        private Dictionary<ulong, InventoryCard> _cards;
        private Dictionary<ulong, InventoryCard> _gifts;

        public InventoryManager(Player player)
        {
            this._player = player;
            this._lock = new object();

            this._cards = new Dictionary<ulong, InventoryCard>();
            this._gifts = new Dictionary<ulong, InventoryCard>();
            // TODO: Database

            //this._gifts.Add(5, new InventoryCard()
            //{
            //    Id = 5,
            //    BoostLevel = 0,
            //    IsActive = false,
            //    IsGiftable = true,
            //    IsOpened = false,
            //    ItemId = 1095368711,
            //    Period = 375,
            //    PeriodeType = 1,
            //    PlayerOwnedId = this._player.PlayerId,
            //    Type = 1
            //});

            //this._cards.Add(6, new InventoryCard()
            //{
            //    Id = 6,
            //    BoostLevel = 0,
            //    IsActive = false,
            //    IsGiftable = false,
            //    IsOpened = true,
            //    ItemId = 1095368711,
            //    Period = 2,
            //    PeriodeType = 2,
            //    PlayerOwnedId = this._player.PlayerId,
            //    Type = 0
            //});

            this._cards.Add(0, new InventoryCard()
            {
                Id = 0,
                BoostLevel = 0,
                IsActive = false,
                IsGiftable = true,
                IsOpened = true,
                ItemId = 1095368711,
                Period = 1,
                PeriodeType = 254,
                PlayerOwnedId = this._player.PlayerId,
                Type = 87
            });

            //this._cards.Add(3, new InventoryCard()
            //{
            //    Id = 3,
            //    BoostLevel = 0,
            //    IsActive = true,
            //    IsGiftable = false,
            //    IsOpened = true,
            //    ItemId = 1124230656,
            //    Period = 50,
            //    PeriodeType = 1,
            //    PlayerOwnedId = this._player.PlayerId,
            //    Type = 1
            //});

            //this._cards.Add(4, new InventoryCard()
            //{
            //    Id = 4,
            //    BoostLevel = 0,
            //    IsActive = true,
            //    IsGiftable = false,
            //    IsOpened = true,
            //    ItemId = 1124230400,
            //    Period = 375,
            //    PeriodeType = 0,
            //    PlayerOwnedId = this._player.PlayerId,
            //    Type = 0
            //});
        }

        public List<InventoryCard> List()
        {
            lock(this._lock)
            {
                List<InventoryCard> cards = new List<InventoryCard>();
                foreach(var c in this._cards)
                {
                    cards.Add(c.Value);
                }
                return cards;
            }
        }

        public List<InventoryCard> ListGifts()
        {
            lock (this._lock)
            {
                List<InventoryCard> cards = new List<InventoryCard>();
                foreach (var c in this._gifts)
                {
                    cards.Add(c.Value);
                }
                return cards;
            }
        }

        public InventoryCard Get(ulong cardId)
        {
            lock(this._lock)
                if (this._cards.ContainsKey(cardId))
                    return this._cards[cardId];
            return new InventoryCard();
        }

        public bool HasCard(ulong cardId)
        {
            lock (this._lock)
                return this._cards.ContainsKey(cardId);
        }

        public void DeleteCard(ulong cardId)
        {
            if (!HasCard(cardId))
                return;

            lock(this._lock)
            {
                if (this._player == null)
                    return;

                bool isEquiped = this._player.EquipmentManager.HasEquipped(cardId);
                if (isEquiped)
                    return;

                this._cards.Remove(cardId);

                // TODO: remove from database!

                this._player.SendLobby(LobbyManager.Instance.RemoveCard(cardId));
            }

            return;
        }

        public void SetCardActive(ulong cardId, bool isActive)
        {
            lock(this._lock)
            {
                if (!this._cards.ContainsKey(cardId))
                    return;

                var card = this._cards[cardId];
                var duplicate = this._player.EquipmentManager.HasFunctionCard(card.ItemId);

                if(isActive && !duplicate)
                {
                    card.TimeCreated = DateTime.UtcNow;
                    card.IsActive = true;
                    this._player.EquipmentManager.AddFunctionCard(cardId);
                    this._player.SendLobby(LobbyManager.Instance.EnabledFunctionCard(card));
                }else if(!isActive && duplicate)
                {
                    card.IsActive = true;
                    this._player.EquipmentManager.AddFunctionCard(cardId);
                    this._player.SendLobby(LobbyManager.Instance.EnabledFunctionCard(card));
                }

                this._cards[card.Id] = card;
            }
            return;
        }

        public bool AddCard(InventoryCard card)
        {
            if (!HasSpace())
                return false;

            lock (this._lock)
                this._cards[card.Id] = card;

            return true;
        }

        public void StoreCard(InventoryCard card)
        {

        }

        public void UseCard(uint cardId, uint playtime)
        {

        }

        public bool IsExpired(ulong cardId)
        {
            return false;
        }

        public bool HasSpace()
        {
            return this._cards.Count + this._gifts.Count < 200;
        }

        public void GiftCard(InventoryCard card, Player player)
        {
            lock(this._lock)
            {
                if (this._player == null)
                    return;

                if (!this._cards.ContainsKey(card.Id))
                    return;

                this._cards.Remove(card.Id);
                card.TimeCreated = DateTime.UtcNow;
                player.InventoryManager.ReceiveGift(card, this._player.Name);

                // TODO: UPDATE player_items SET player_id = ?, opened = 0, time = ? WHERE id = ?

                this._player.SendLobby(LobbyManager.Instance.GiftCardSuccess(card.Id));
                this._player.EquipmentManager.Save();
            }
        }

        public void ReceiveGift(InventoryCard card, string sender)
        {
            lock(this._lock)
            {
                if (this._player == null)
                    return;

                card.IsOpened = false;
                card.PlayerOwnedId = this._player.PlayerId;
                this._gifts[card.Id] = card;

                this._player.SendLobby(LobbyManager.Instance.ReceiveGift(card, sender));
            }
        }

        public void OpenGift(ulong cardId)
        {
            lock(this._lock)
            {
                if (this._player == null)
                    return;

                if (!this._gifts.ContainsKey(cardId))
                    return;

                var card = this._gifts[cardId];
                card.IsOpened = true;
                this._cards[card.Id] = card;

                this._gifts.Remove(cardId);

                // TODO: UPDATE player_items SET opened = 1 WHERE id = ?

                this._player.SendLobby(LobbyManager.Instance.OpenGiftSuccess(this._player, card));
            }
        }

        public bool HasGiftSpace()
        {
            return this._gifts.Count < 5;
        }
        public void Close()
        {
            // TODO
            lock(this._lock)
            {
                // TODO: UPDATE player_items SET active = ? WHERE id = ?"
            }
        }
    }
}
