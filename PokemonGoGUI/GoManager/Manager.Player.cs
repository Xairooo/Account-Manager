﻿using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonGoGUI.GoManager.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGoGUI.Extensions;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using Google.Protobuf;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        
        public async Task<MethodResult> UpdateDetails()
        {
            //TODO: review what we need do here.
            UpdateInventory();// <- should not be needed

            LogCaller(new LoggerEventArgs("Updating details", LoggerTypes.Debug));

            await Task.Delay(CalculateDelay(UserSettings.GeneralDelay, UserSettings.GeneralDelayRandom));


            return new MethodResult
            {
                Success = true
            };
        }


        public async Task<MethodResult> ExportStats()
        {
            MethodResult result = await UpdateDetails();

            //Prevent API throttling
            await Task.Delay(500);

            if (!result.Success)
            {
                return result;
            }

            //Possible some objects were empty.
            var builder = new StringBuilder();
            builder.AppendLine("=== Trainer Stats ===");

            if (Stats != null && PlayerData != null)
            {
                builder.AppendLine(String.Format("Group: {0}", UserSettings.GroupName));
                builder.AppendLine(String.Format("Username: {0}", UserSettings.Username));
                builder.AppendLine(String.Format("Password: {0}", UserSettings.Password));
                builder.AppendLine(String.Format("Level: {0}", Stats.Level));
                builder.AppendLine(String.Format("Current Trainer Name: {0}", PlayerData.Username));
                builder.AppendLine(String.Format("Team: {0}", PlayerData.Team));
                builder.AppendLine(String.Format("Stardust: {0:N0}", TotalStardust));
                builder.AppendLine(String.Format("Unique Pokedex Entries: {0}", Stats.UniquePokedexEntries));
            }
            else
            {
                builder.AppendLine("Failed to grab stats");
            }

            builder.AppendLine();

            builder.AppendLine("=== Pokemon ===");

            if (Pokemon != null)
            {
                foreach (PokemonData pokemon in Pokemon.OrderByDescending(x => x.Cp))
                {
                    string candy = "Unknown";

                    MethodResult<PokemonSettings> pSettings = GetPokemonSetting(pokemon.PokemonId);

                    if (pSettings.Success)
                    {
                        Candy pCandy = PokemonCandy.FirstOrDefault(x => x.FamilyId == pSettings.Data.FamilyId);

                        if (pCandy != null)
                        {
                            candy = pCandy.Candy_.ToString("N0");
                        }
                    }

                    double perfectResult = CalculateIVPerfection(pokemon);
                    string iv = "Unknown";

                    iv = Math.Round(perfectResult, 2).ToString() + "%";

                    builder.AppendLine(String.Format("Pokemon: {0,-10} CP: {1, -5} IV: {2,-7} Primary: {3, -14} Secondary: {4, -14} Candy: {5}", pokemon.PokemonId, pokemon.Cp, iv, pokemon.Move1.ToString().Replace("Fast", ""), pokemon.Move2, candy));
                }
            }

            //Remove the hardcoded directory later
            try
            {
                string directoryName = "AccountStats";

                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                string fileName = UserSettings.Username.Split('@').First();

                string filePath = Path.Combine(directoryName, fileName) + ".txt";

                File.WriteAllText(filePath, builder.ToString());

                LogCaller(new LoggerEventArgs(String.Format("Finished exporting stats to file {0}", filePath), LoggerTypes.Info));

                return new MethodResult
                {
                    Message = "Success",
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to export stats due to exception", LoggerTypes.Warning, ex));

                return new MethodResult();
            }
        }

        public async Task<MethodResult> ClaimLevelUpRewards(int level)
        {
            if (!UserSettings.ClaimLevelUpRewards || level < 2)
            {
                return new MethodResult();
            }

            try
            {
                if (!_client.LoggedIn)
                {
                    MethodResult result = await AcLogin();

                    if (!result.Success)
                    {
                        return result;
                    }
                }

                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.LevelUpRewards,
                    RequestMessage = new LevelUpRewardsMessage
                    {
                        Level = level
                    }.ToByteString()
                });

                LevelUpRewardsResponse levelUpRewardsResponse = null;

                levelUpRewardsResponse = LevelUpRewardsResponse.Parser.ParseFrom(response);
                string rewards = StringUtil.GetSummedFriendlyNameOfItemAwardList(levelUpRewardsResponse.ItemsAwarded);
                LogCaller(new LoggerEventArgs(String.Format("Grabbed rewards for level {0}. Rewards: {1}", level, rewards), LoggerTypes.Info));

                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to get level up rewards", LoggerTypes.Exception, ex));
                return new MethodResult();
            }
        }

        public async Task<MethodResult<GetBuddyWalkedResponse>> GetBuddyWalked()
        {
            GetBuddyWalkedResponse getBuddyWalkedResponse = null;

            try
            {
                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.GetBuddyWalked,
                    RequestMessage = new GetBuddyWalkedMessage
                    {

                    }.ToByteString()
                });

                getBuddyWalkedResponse = GetBuddyWalkedResponse.Parser.ParseFrom(response);
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("GetBuddyWalkedResponse is empty", LoggerTypes.Exception, ex));

                return new MethodResult<GetBuddyWalkedResponse>
                {
                    Data = getBuddyWalkedResponse
                };
            }

            return new MethodResult<GetBuddyWalkedResponse>
            {
                Data = getBuddyWalkedResponse,
                Success = true
            };
        }
        public async Task<MethodResult> GetPlayer(bool nobuddy =true, bool noinbox =true)
        {

            try
            {
                if (!_client.LoggedIn)
                {
                    MethodResult result = await AcLogin();

                    if (!result.Success)
                    {
                        return result;
                    }
                }

                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request {
                    RequestType = RequestType.GetPlayer,
                    RequestMessage = new GetPlayerMessage {
                        PlayerLocale = new GetPlayerMessage.Types.PlayerLocale {
                            Country = UserSettings.PlayerLocale.Country,
                            Language = UserSettings.PlayerLocale.Language,
                            Timezone = UserSettings.PlayerLocale.Timezone
                        }
                    }.ToByteString()
                }, true, nobuddy, noinbox);


                var parsedResponse = GetPlayerResponse.Parser.ParseFrom(response);


                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to get level up rewards", LoggerTypes.Exception, ex));
                return new MethodResult();
            }
        }
        public async Task<MethodResult> GetPlayerProfile()
        {

            try
            {
                if (!_client.LoggedIn)
                {
                    MethodResult result = await AcLogin();

                    if (!result.Success)
                    {
                        return result;
                    }
                }

                var response = await _client.ClientSession.RpcClient.SendRemoteProcedureCallAsync(new Request {
                    RequestType = RequestType.GetPlayerProfile,
                    RequestMessage = new GetPlayerProfileMessage {
                    }.ToByteString()
                }, true, false, true);


                var parsedResponse = GetPlayerProfileResponse.Parser.ParseFrom(response);


                return new MethodResult
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                LogCaller(new LoggerEventArgs("Failed to get level up rewards", LoggerTypes.Exception, ex));
                return new MethodResult();
            }
        }         
    }
}
