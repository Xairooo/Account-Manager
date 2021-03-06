﻿using Newtonsoft.Json;
using POGOLib.Official.Util.Device;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using PokemonGoGUI.Enums;
using PokemonGoGUI.Extensions;
using PokemonGoGUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace PokemonGoGUI
{
    public class Settings
    {
        public List<string> HashKeys { get; set; }
        public bool UseOnlyOneKey { get; set; }
        public string AuthAPIKey { get; set; }
        public Uri HashHost { get; set; }
        public string HashEndpoint { get; set; }
        public cLocale PlayerLocale  { get; set; } = new cLocale();
        public cLocation Location { get; set; } = new cLocation();

        public double DisableCatchDelay { get; set; }
        public bool SpinGyms { get; set; }
        public string GroupName { get; set; }
        public string AccountName { get; set; }
        public AuthType AuthType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool MimicWalking { get; set; }
        public int WalkingSpeed { get; set; }
        public bool EncounterWhileWalking { get; set; }
        public int MaxTravelDistance { get; set; }
        public bool UseLuckyEgg { get; set; }
        public bool ClaimLevelUpRewards { get; set; }
        public int MinPokemonBeforeEvolve { get; set; }
        public bool RecycleItems { get; set; }
        public bool TransferPokemon { get; set; }
        public bool EvolvePokemon { get; set; }
        public bool CatchPokemon { get; set; }
        public bool IncubateEggs { get; set; }
        public int MaxLevel { get; set; }
        public bool SPF { get; set; }
        
        public double SearchFortBelowPercent { get; set; }
        public double ForceEvolveAbovePercent { get; set; }
        public bool StopOnAPIUpdate { get; set; }

        public int MaxLogs { get; set; }
        public double RunForHours{ get; set; }

        //Humanization
        public bool EnableHumanization { get; set; }
        public int InsideReticuleChance { get; set; }

        public int DelayBetweenPlayerActions { get; set; }
        public int PlayerActionDelayRandom { get; set; }

        public int DelayBetweenLocationUpdates { get; set; }
        public int LocationupdateDelayRandom { get; set; }

        public int GeneralDelay { get; set; }
        public int GeneralDelayRandom { get; set; }

        public double WalkingSpeedOffset { get; set; }
        //End Humanization
        
        public cDeviceInfo DeviceInfo { get; set; } = new cDeviceInfo();

        public string ProxyIP { get; set; }
        public int ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        public bool AutoRotateProxies { get; set; }
        public bool AutoRemoveOnStop { get; set; }

        public bool StopOnIPBan { get; set; }
        public int MaxFailBeforeReset { get; set; }

        public bool UseBerries {
            get;
            set;
        }

        public bool OnlyUnlimitedIncubator {
            get;
            set;
        }

        public bool TransferSlashPokemons {
            get;
            set;
        }

        public bool ShufflePokestops {
            get;
            set;
        }

        public bool GetArBonus {
            get;
            set;
        }

        public decimal ARBonusProximity {
            get;
            set;
        }

        public decimal ARBonusAwareness {
            get;
            set;
        }

        public bool CompleteTutorial {
            get;
            set;
        }

        public bool TransferAtOnce {
            get;
            set;
        }

        public bool ShowDebugLogs {
            get;
            set;
        }
        public bool DownloadResources {
            get;
            set;
        }
        public AccountState StopAtMinAccountState { get; set; }

        public ProxyEx Proxy {
            get {
                return new ProxyEx {
                    Address = ProxyIP,
                    Port = ProxyPort,
                    Username = ProxyUsername,
                    Password = ProxyPassword
                };
            }
        }

        public List<InventoryItemSetting> ItemSettings { get; set; }
        public List<CatchSetting> PokemonSettings { get; set; }
        public List<TransferSetting> TransferSettings { get; set; }

        [JsonConstructor]
        public Settings(bool jsonConstructor = true)
        {
            LoadDefaults();
        }

        public Settings()
        {
            //Defaults
            LoadDefaults();

            ItemSettings = new List<InventoryItemSetting>();
            CatchSettings = new List<CatchSetting>();

            RandomizeDevice();
            LoadInventorySettings();
            LoadCatchSettings();
            LoadEvolveSettings();
            LoadTransferSettings();
        }

        public void LoadDefaults()
        {
            GroupName = "Default";
            AuthType = AuthType.Ptc;
            Location = new cLocation(40.764665, -73.973184, 10);
            MimicWalking = true;
            CatchPokemon = true;
            WalkingSpeed = 7;
            MaxTravelDistance = 1000;
            EncounterWhileWalking = false;
            EnableHumanization = false;
            InsideReticuleChance = 100;
            MinPokemonBeforeEvolve = 0;
            StopAtMinAccountState = AccountState.SoftBan;
            DelayBetweenPlayerActions = 500;
            DelayBetweenLocationUpdates = 1000;
            GeneralDelay = 800;
            MaxLogs = 400;
            MaxFailBeforeReset = 3;
            StopOnIPBan = true;
            SearchFortBelowPercent = 1000;
            ForceEvolveAbovePercent = 1000;
            StopOnAPIUpdate = true;
            SpinGyms = true;
            HashHost = new Uri("https://pokehash.buddyauth.com/");
            HashEndpoint = "api/v157_5/hash";
            AuthAPIKey = "XXXXXXXXXXXXXXXXXXXX";
            PlayerLocale.Country = "US";
            PlayerLocale.Language = "en";
            PlayerLocale.Timezone = "America/New_York";
            PlayerLocale.POSIX = "en-us";
            DisableCatchDelay = 8;
            DownloadResources = true;
        }


        public void LoadCatchSettings()
        {
            CatchSettings = new List<CatchSetting>();

            foreach (PokemonId pokemon in Enum.GetValues(typeof(PokemonId))) {
                if (pokemon == PokemonId.Missingno) {
                    continue;
                }

                var cSettings = new CatchSetting {
                    Id = pokemon
                };

                CatchSettings.Add(cSettings);
            }
        }

        public void LoadInventorySettings()
        {
            ItemSettings = new List<InventoryItemSetting>();

            foreach (ItemId item in Enum.GetValues(typeof(ItemId))) {
                if (item == ItemId.ItemUnknown) {
                    continue;
                }

                var itemSetting = new InventoryItemSetting {
                    Id = item
                };

                ItemSettings.Add(itemSetting);
            }
        }

        public void LoadEvolveSettings()
        {
            EvolveSettings = new List<EvolveSetting>();

            foreach (PokemonId pokemon in Enum.GetValues(typeof(PokemonId))) {
                if (pokemon == PokemonId.Missingno) {
                    continue;
                }

                var setting = new EvolveSetting {
                    Id = pokemon
                };

                EvolveSettings.Add(setting);
            }
        }

        public void LoadTransferSettings()
        {
            TransferSettings = new List<TransferSetting>();

            foreach (PokemonId pokemon in Enum.GetValues(typeof(PokemonId))) {
                if (pokemon == PokemonId.Missingno) {
                    continue;
                }

                var setting = new TransferSetting {
                    Id = pokemon,
                    Transfer = true
                };

                TransferSettings.Add(setting);
            }
        }

        public void RandomizeDeviceId()
        {
            var device = DeviceInfoUtil.GetRandomDevice();
            DeviceInfo.DeviceId = device.DeviceInfo.DeviceId;
        }
        
        public void RandomizeDevice()
        {
            var device = DeviceInfoUtil.GetRandomDevice();
            DeviceInfo.DeviceId = device.DeviceInfo.DeviceId;
            DeviceInfo.DeviceBrand = device.DeviceInfo.DeviceBrand;
            DeviceInfo.DeviceModel = device.DeviceInfo.DeviceModel;
            DeviceInfo.DeviceModelBoot = device.DeviceInfo.DeviceModelBoot;
            DeviceInfo.HardwareManufacturer = device.DeviceInfo.HardwareManufacturer;
            DeviceInfo.HardwareModel = device.DeviceInfo.HardwareModel;
            DeviceInfo.FirmwareBrand = device.DeviceInfo.FirmwareBrand;
            DeviceInfo.FirmwareType = device.DeviceInfo.FirmwareType;
        }

        private byte RandomByte()
        {
            using (var randomizationProvider = new RNGCryptoServiceProvider()) {
                var randomBytes = new byte[1];
                randomizationProvider.GetBytes(randomBytes);
                return randomBytes.Single();
            }
        }
        public class cLocation
        {
            public cLocation()
            {
            
            }
            public cLocation(double lat, double lon, int alt)
            {
                Latitude = lat;
                Longitude = lon;
                Altitude = alt;
            }

            public double Latitude{ get; set; }
            public double Longitude{ get; set; }
            public double Altitude{ get; set; }
        }
        public class cDeviceInfo
        {
            public string DeviceId{ get; set; }
            public string DeviceBrand{ get; set; }
            public string DeviceModel{ get; set; }
            public string DeviceModelBoot{ get; set; }
            public string HardwareManufacturer{ get; set; }
            public string HardwareModel{ get; set; }
            public string FirmwareBrand{ get; set; }
            public string FirmwareType{ get; set; }
        }
        public class cLocale
        {
            public string Country{ get; set; }
            public string Language{ get; set; }
            public string Timezone{ get; set; }
            public string POSIX{ get; set; }
        }
        //Obsoleted. For retrocompatibility. Remove after of several new versions. (currently 2.21.1.25)
        public double DefaultLatitude{ get; set; }
        public double DefaultLongitude{ get; set; }
        public double DefaultAltitude{ get; set; }
        public string DeviceId{ get; set; }
        public string DeviceBrand{ get; set; }
        public string DeviceModel{ get; set; }
        public string DeviceModelBoot{ get; set; }
        public string HardwareManufacturer{ get; set; }
        public string HardwareModel{ get; set; }
        public string FirmwareBrand{ get; set; }
        public string FirmwareType{ get; set; }
        public string Country { get; set; }
        public string Language { get; set; }
        public string TimeZone { get; set; }
        public string POSIX { get; set; }
        public List<CatchSetting> CatchSettings { get; set; }
        public List<EvolveSetting> EvolveSettings { get; set; }
        
        
    }
}
