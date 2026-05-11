using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Timberborn.BehaviorSystem;
using Timberborn.Carrying;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Yielding;
using UnityEngine;

namespace KKDoubleResources;

internal static class ResourceMultiplier
{
    private const int Multiplier = 10;

    private static readonly HashSet<string> BoostedGoods = new(StringComparer.Ordinal)
    {
        "Log",
        "Berries",
        "Dandelion",
        "CoffeeBean",
        "Chestnut",
        "MangroveFruit",
        "PineResin",
        "MapleSyrup",
        "ScrapMetal"
    };

    public static bool CanBoost(string goodId)
    {
        return BoostedGoods.Contains(goodId);
    }

    public static GoodAmount Multiply(GoodAmount goods)
    {
        return new GoodAmount(goods.GoodId, goods.Amount * Multiplier);
    }
}

internal static class PendingYieldDeliveries
{
    private static readonly Dictionary<GoodCarrier, GoodAmount> BoostedByCarrier = new();

    public static void Set(GoodCarrier carrier, GoodAmount boostedGoods)
    {
        BoostedByCarrier[carrier] = boostedGoods;
    }

    public static bool TryGet(GoodCarrier carrier, out GoodAmount boostedGoods)
    {
        return BoostedByCarrier.TryGetValue(carrier, out boostedGoods);
    }

    public static void Clear(GoodCarrier carrier)
    {
        BoostedByCarrier.Remove(carrier);
    }
}

[HarmonyPatch(typeof(YielderRemover), nameof(YielderRemover.CompleteReservation))]
internal static class YieldDeliveryMarkerPatch
{
    private static readonly FieldInfo GoodCarrierField =
        AccessTools.Field(typeof(YielderRemover), "_goodCarrier");

    private static readonly FieldInfo ReservedYieldField =
        AccessTools.Field(typeof(YielderRemover), "_reservedYield");

    private static bool _loggedMarker;

    [HarmonyPostfix]
    public static void Postfix(YielderRemover __instance)
    {
        GoodAmount reservedYield = (GoodAmount)ReservedYieldField.GetValue(__instance);
        string goodId = reservedYield.GoodId;
        int amount = reservedYield.Amount;
        if (amount <= 0 || !ResourceMultiplier.CanBoost(goodId))
        {
            return;
        }

        GoodCarrier goodCarrier = (GoodCarrier)GoodCarrierField.GetValue(__instance);
        GoodAmount boostedYield = ResourceMultiplier.Multiply(reservedYield);
        PendingYieldDeliveries.Set(goodCarrier, boostedYield);

        if (!_loggedMarker)
        {
            _loggedMarker = true;
            Debug.Log($"[KKDoubleResources] Marked boosted delivery {goodId} {amount} -> {boostedYield.Amount}");
        }
    }
}

[HarmonyPatch(typeof(CarryRootBehavior), "CompleteDelivery")]
internal static class CarryDeliveryMultiplierPatch
{
    private static readonly FieldInfo GoodReserverField =
        AccessTools.Field(typeof(CarryRootBehavior), "_goodReserver");

    private static readonly FieldInfo GoodCarrierField =
        AccessTools.Field(typeof(CarryRootBehavior), "_goodCarrier");

    private static bool _loggedDelivery;

    [HarmonyPrefix]
    public static bool Prefix(CarryRootBehavior __instance, ref Decision __result)
    {
        GoodReserver goodReserver = (GoodReserver)GoodReserverField.GetValue(__instance);
        GoodCarrier goodCarrier = (GoodCarrier)GoodCarrierField.GetValue(__instance);
        GoodReservation capacityReservation = goodReserver.CapacityReservation;

        goodReserver.UnreserveCapacity();

        Inventory inventory = capacityReservation.Inventory;
        GoodAmount reservedGoods = capacityReservation.GoodAmount;
        GoodAmount goodsToGive = GetGoodsToGive(goodCarrier, reservedGoods);
        if (inventory.HasUnreservedCapacity(goodsToGive))
        {
            inventory.Give(goodsToGive);
            goodCarrier.EmptyHands();
            __result = Decision.ReleaseNow();
            return false;
        }

        __result = Decision.ReleaseNextTick();
        return false;
    }

    private static GoodAmount GetGoodsToGive(GoodCarrier goodCarrier, GoodAmount reservedGoods)
    {
        if (!PendingYieldDeliveries.TryGet(goodCarrier, out GoodAmount boostedGoods) ||
            boostedGoods.GoodId != reservedGoods.GoodId ||
            boostedGoods.Amount <= reservedGoods.Amount)
        {
            return reservedGoods;
        }

        if (!_loggedDelivery)
        {
            _loggedDelivery = true;
            Debug.Log(
                $"[KKDoubleResources] Boosted flag delivery {boostedGoods.GoodId} {reservedGoods.Amount} -> {boostedGoods.Amount}");
        }

        return boostedGoods;
    }
}

[HarmonyPatch(typeof(GoodCarrier), nameof(GoodCarrier.EmptyHands))]
internal static class ClearPendingYieldDeliveryPatch
{
    [HarmonyPrefix]
    public static void Prefix(GoodCarrier __instance)
    {
        PendingYieldDeliveries.Clear(__instance);
    }
}
