using HarmonyLib;
using Timberborn.ModManagerScene;
using Timberborn.RecoveredGoodSystem;
using UnityEngine;

namespace Calloatti.RubbleScavengers
{
  public class ModStarter : IModStarter
  {
    public void StartMod(IModEnvironment modEnvironment)
    {
      new Harmony("Calloatti.RubbleScavengers").PatchAll();
      Debug.Log("[RubbleScavengers] Patches applied successfully.");
    }
  }

}