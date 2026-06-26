using System;
using System.Collections.Generic;
using HarmonyLib;
using Timberborn.ConstructionSites;
using Timberborn.RecoveredGoodSystem;
using Timberborn.InventorySystem;
using Timberborn.Navigation;
using Timberborn.PrioritySystem;
using UnityEngine;

namespace Calloatti.RubbleScavengers
{
  [HarmonyPatch(typeof(PrioritizedRecoveredGoodStackRegistry), "Load")]
  public static class Patch_PrioritizedRecoveredGoodStackRegistry_Load
  {
    public static PrioritizedRecoveredGoodStackRegistry RubbleRegistry;

    public static void Postfix(PrioritizedRecoveredGoodStackRegistry __instance)
    {
      RubbleRegistry = __instance;
      Debug.Log("[RubbleScavengers] Successfully hooked into the active map's Rubble Registry!");
    }
  }

  [HarmonyPatch(typeof(DistrictInventoryPicker), "ClosestInventoryWithStock", new Type[] { typeof(Vector3), typeof(string), typeof(Accessible) })]
  public static class Patch_DistrictInventoryPicker_ClosestInventoryWithStock
  {
    public static void Postfix(
        ref Inventory __result,
        Vector3 start,
        string goodId,
        Accessible accessibleReachableFromInventory)
    {
      if (Patch_PrioritizedRecoveredGoodStackRegistry_Load.RubbleRegistry == null) return;

      // OPTIMIZATION 1: Limit the search to a local radius (e.g., 30 blocks).
      float maxSearchRadius = 30f;

      // OPTIMIZATION 2: Use sqrMagnitude to avoid expensive square root calculations.
      float bestSqrDistance = maxSearchRadius * maxSearchRadius;

      if (__result != null)
      {
        Accessible storageAccessible = __result.GetEnabledComponent<Accessible>();
        if (storageAccessible != null)
        {
          bestSqrDistance = (start - storageAccessible.Transform.position).sqrMagnitude;
        }
      }

      Inventory bestRubbleInventory = null;

      foreach (Timberborn.PrioritySystem.Priority priority in Priorities.Descending)
      {
        var rubbleStacks = Patch_PrioritizedRecoveredGoodStackRegistry_Load.RubbleRegistry.GetRecoveredGoodStacks(priority);
        if (rubbleStacks == null) continue;

        bool foundInThisPriority = false;

        foreach (RecoveredGoodStack rubble in rubbleStacks)
        {
          if (rubble == null) continue;

          // OPTIMIZATION 7: The "Cheap Gate". 
          // Check distance FIRST using the cached Transform. 
          // Skips inventory & component lookups for 99% of the map's rubble.
          float sqrDistance = (start - rubble.Transform.position).sqrMagnitude;
          if (sqrDistance >= bestSqrDistance) continue;

          // Now do the slightly more expensive inventory checks
          if (rubble.Inventory == null || rubble.Inventory.IsEmpty) continue;
          if (rubble.Inventory.UnreservedAmountInStock(goodId) <= 0) continue;

          // Finally, do the component fetch and the most expensive pathfinding check
          Accessible rubbleAccessible = rubble.GetEnabledComponent<Accessible>();
          if (rubbleAccessible == null) continue;

          // OPTIMIZATION 4: Only perform the expensive pathing check if we already know it's closer.
          if (!accessibleReachableFromInventory.IsReachableByRoadToTerrain(rubbleAccessible)) continue;

          bestSqrDistance = sqrDistance;
          bestRubbleInventory = rubble.Inventory;
          foundInThisPriority = true;

          // OPTIMIZATION 5: Ultra-Close Early Exit. 
          // If we find valid rubble within ~2 blocks (sqrMagnitude of 4), stop searching.
          if (bestSqrDistance <= 4f) break;
        }

        // OPTIMIZATION 6: Priority Tier Short-Circuiting.
        // If we found ANY reachable rubble in this high-priority tier, skip lower tiers.
        if (foundInThisPriority) break;
      }

      if (bestRubbleInventory != null)
      {
        if (__result != bestRubbleInventory)
        {
          //Debug.Log($"[RubbleScavengers] Redirected beaver to scavenge {goodId} directly from rubble!");
        }
        __result = bestRubbleInventory;
      }
    }
  }
}