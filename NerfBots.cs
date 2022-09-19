using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust.Cui;
using Rust;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CompanionServer;
using Facepunch;
using ProtoBuf;

namespace Oxide.Plugins
{
  [Info("Nerf Bots", "SegFault", "0.0.1")]
  [Description("Apply a customizable nerf on rust+ bots game events monitoring")]
  class NerfBots : CovalencePlugin
  {
    #region Localization
    protected override void LoadDefaultMessages()
    {
      lang.RegisterMessages(new Dictionary<string, string> 
      {
        ["EnableFeature"] = "Added {0} map markers on rust companion.",
        ["DisableFeature"] = "Removed {0} map markers on rust companion."
      }, this);
    }
    #endregion

    #region Configs
    private Configuration config;
    public class Configuration
    {
      [JsonProperty("Enable debug logs")]
      public bool Debug = false;

      [JsonProperty("Remove every map markers from rust+ (except vending machines)")]
      public bool HideAll = false;

      [JsonProperty("Remove map markers from hackable crates (Nerf oilrigs, cargo crates and CH47 dropped crate)")]
      public bool HideHackables = false;

      [JsonProperty("Remove map marker from explosions (Heli crash, Bradley)")]
      public bool HideExplosions = false;

      [JsonProperty("Remove map marker from cargo ship")]
      public bool HideCargoship = false;

      [JsonProperty("Remove map marker from helicopter")]
      public bool HideHelicopter = false;

      [JsonProperty("Remove map marker from CH47 (Nerf oilrigs and CH47 dropped crate)")]
      public bool HideCH47 = false;

      public string ToJson() => JsonConvert.SerializeObject(this);
      public Dictionary<string, object> ToDictionary() => JsonConvert.DeserializeObject<Dictionary<string, object>>(ToJson());
    }

    protected override void LoadDefaultConfig() => config = new Configuration();

    protected override void LoadConfig()
    {
      base.LoadConfig();
      try
      {
        config = Config.ReadObject<Configuration>();
        if (config == null)
          throw new JsonException();

        if (!config.ToDictionary().Keys.SequenceEqual(Config.ToDictionary(x => x.Key, x => x.Value).Keys))
        {
          LogWarning("Configuration appears to be outdated; updating and saving");
          SaveConfig();
        }
      }
      catch
      {
        LogWarning($"Configuration file {Name}.json is invalid; using defaults");
        LoadDefaultConfig();
      }
    }

    protected override void SaveConfig()
    {
      Config.WriteObject(config, true);
    }
    #endregion
    #region Functions
    private string formatConfig(bool option) {
      return option ? "removed" : "displayed";
    }

    private void checkMarker(MapMarker marker) 
    {
      if ((config.HideAll && marker.appType != AppMarkerType.VendingMachine)
      || (config.HideHackables && marker.appType == AppMarkerType.Crate) 
      || (config.HideExplosions && marker.appType == AppMarkerType.Explosion) 
      || (config.HideCargoship && marker.appType == AppMarkerType.CargoShip) 
      || (config.HideHelicopter && marker.appType == AppMarkerType.PatrolHelicopter)
      || (config.HideCH47 && marker.appType == AppMarkerType.CH47))
      {
        marker.appType = AppMarkerType.Undefined;
        if (config.Debug)
          Puts($"Neutralized map marker {marker.appType} ({marker.ShortPrefabName})");
      }
    }

    private void applyRules()
    {
      foreach (MapMarker marker in MapMarker.serverMapMarkers)
        if (marker.appType != AppMarkerType.Undefined && marker.appType != AppMarkerType.VendingMachine)
        {
          if (config.Debug)
            Puts($"Found visible map marker {marker.appType} ({marker.ShortPrefabName})");
          checkMarker(marker);
        }
    }
    #endregion
    #region Hooks
    private void Loaded()
    {
      applyRules();
    }
    void OnEntitySpawned(MapMarker marker)
    {
      if (marker == null)
        return;
      checkMarker(marker);
    }
    #endregion

    #region Commands

    [Command("nerfbots"), Permission("nerfbots.configuration")]
    private void CommandNerfBots(IPlayer player, string command, string[] args)
    {
      if (args.Length != 2)
      {
        if (config.HideAll)
          player.Reply("[Settings] All markers disabled");
        else 
          player.Reply($"[Settings] crates: {formatConfig(config.HideHackables)} | cargo: {formatConfig(config.HideCargoship)} | explosions: {formatConfig(config.HideExplosions)} | helicopter: {formatConfig(config.HideHelicopter)} | ch47: {formatConfig(config.HideCH47)}");
        player.Reply("Usage: /nerfbots <all|crates|cargo|heli|ch47|explosions> <display|remove>");
        return;
      }
      string feature = args[0];
      bool state = args[1].ToLower() == "remove";

      switch (feature)
      {
        case "all":
          config.HideAll = state;
          break;
        case "crates":
          config.HideHackables = state;
          break;
        case "cargo":
          config.HideCargoship = state;
          break;
        case "heli":
          config.HideHelicopter = state;
          break;
        case "explosions":
          config.HideExplosions = state;
          break;
        case "ch47":
          config.HideCH47 = state;
          break;
        default:
          player.Reply("Usage: /nerfbots <all|crates|cargo|heli|explosions> <enable|disable>");
          return;
      }

      SaveConfig();
      applyRules();
      string message = lang.GetMessage(state ? "DisableFeature" : "EnableFeature", this, player.Id);
      player.Reply(string.Format(message, feature));
    }
    #endregion
  }
}