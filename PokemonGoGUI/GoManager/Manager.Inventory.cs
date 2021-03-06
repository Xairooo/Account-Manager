﻿using System.Collections.Generic;
using Google.Protobuf;
using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.GoManager.Models;
using PokemonGoGUI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        public void UpdateInventory()
        {
            LogCaller(new LoggerEventArgs("Updating inventory.", LoggerTypes.Debug));

            try
            {
                var inventoryItems = _client?.ClientSession?.Player?.Inventory?.InventoryItems;
                if( inventoryItems == null )
                    return;
                Items.Clear();
                Pokemon.Clear();
                Pokedex.Clear();
                PokemonCandy.Clear();
                Incubators.Clear();
                Eggs.Clear();
                foreach (var inventoryItem in inventoryItems) {
                    if (inventoryItem.InventoryItemData?.PlayerStats!=null){
                        Stats = inventoryItem.InventoryItemData.PlayerStats;
                    }
                    if (inventoryItem.InventoryItemData?.Item!=null){
                        Items.Add(inventoryItem.InventoryItemData.Item);
                    }
                    if (inventoryItem.InventoryItemData?.PokedexEntry!=null){
                        Pokedex.Add(inventoryItem.InventoryItemData.PokedexEntry);
                    }
                    if (inventoryItem.InventoryItemData?.Candy!=null){
                        PokemonCandy.Add(inventoryItem.InventoryItemData.Candy);
                    }
                    if (inventoryItem.InventoryItemData?.EggIncubators!=null){
                        foreach (var eggIncubator in inventoryItem.InventoryItemData.EggIncubators.EggIncubator)
                            if (eggIncubator!=null)
                                Incubators.Add(eggIncubator);
                        
                    }
                    if (inventoryItem.InventoryItemData?.PokemonData!=null){
                        if (inventoryItem.InventoryItemData.PokemonData.IsEgg)
                            Eggs.Add(inventoryItem.InventoryItemData.PokemonData);
                        else
                            Pokemon.Add(inventoryItem.InventoryItemData.PokemonData);
                    }
                    
                }


            }
            catch (Exception ex1)
            {
                AccountState = Enums.AccountState.TemporalBan;
                LogCaller(new LoggerEventArgs(String.Format("Failed updating inventory."), LoggerTypes.Debug, ex1));
                Stop();
            }
        }

        public async Task<MethodResult> RecycleFilteredItems()
        {
            if (!UserSettings.RecycleItems)
            {
                return new MethodResult
                {
                    Message = "Item deletion not enabled"
                };
            }


            foreach (ItemData item in Items)
            {
                InventoryItemSetting itemSetting = UserSettings.ItemSettings.FirstOrDefault(x => x.Id == item.ItemId);

                if (itemSetting == null)
                {
                    continue;
                }

                int toDelete = item.Count - itemSetting.MaxInventory;

                if (toDelete <= 0)
                {
                    continue;
                }

                await RecycleItem(itemSetting, toDelete);

                await Task.Delay(CalculateDelay(UserSettings.DelayBetweenPlayerActions, UserSettings.PlayerActionDelayRandom));
            }


            return new MethodResult
            {
                Message = "Success",
                Success = true
            };
        }

        public async Task<MethodResult> RecycleItem(ItemData item, int toDelete)
        {
            InventoryItemSetting itemSetting = UserSettings.ItemSettings.FirstOrDefault(x => x.Id == item.ItemId);

            return itemSetting == null ? new MethodResult() : await RecycleItem(itemSetting, toDelete);

        }

        public async Task<MethodResult> RecycleItem(InventoryItemSetting itemSetting, int toDelete)
        {
            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.RecycleInventoryItem,
                    RequestMessage = new RecycleInventoryItemMessage
                    {
                        Count = toDelete,
                        ItemId = itemSetting.Id
                    }.ToByteString()
                });

                RecycleInventoryItemResponse recycleInventoryItemResponse = null;

                recycleInventoryItemResponse = RecycleInventoryItemResponse.Parser.ParseFrom(response);
                LogCaller(new LoggerEventArgs(String.Format("Deleted {0} {1}. Remaining {2}", toDelete, itemSetting.FriendlyName, recycleInventoryItemResponse.NewCount), LoggerTypes.Recycle));

                return new MethodResult
                {
                    Success = true
                };

            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs(String.Format("Failed to recycle iventory item {0}", itemSetting.FriendlyName), LoggerTypes.Warning, ex));

                return new MethodResult();
            }
        }

        public double FilledInventoryStorage()
        {
            if (Items == null || PlayerData == null)
            {
                return 100;
            }

            return (double)Items.Sum(x => x.Count) / PlayerData.MaxItemStorage * 100;
        }
        public double FilledPokemonStorage()
        {
            if (Pokemon == null || PlayerData == null)
            {
                return 100;
            }

            return (double)(Pokemon.Count + Eggs.Count) / PlayerData.MaxPokemonStorage * 100;
        }
    }
}
