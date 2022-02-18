using ColossalFramework.Math;
using HarmonyLib;
using SharedEnvironment;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(BuildingManager))]
    [HarmonyPatch("ReleaseBuilding")]
    class BuildingManagerPatch_ReleaseBuilding
    {
        static void Prefix(ushort building)
        {
            UndoMod.Instance.ObservingOnlyBuildings++;
            ref Building data = ref ManagerUtils.BuildingS(building);
            if ((data.m_flags != Building.Flags.None && (data.m_flags & Building.Flags.Deleted) == Building.Flags.None) && !UndoMod.Instance.PerformingAction
                    && !UndoMod.Instance.Invalidated) {
                if (UndoMod.Instance.Observing) {
                    try {
                        var constructable = UndoMod.Instance.WrappersDictionary.RegisterBuilding(building);
                        constructable.ForceSetId(0);
                        UndoMod.Instance.ReportObservedAction(new ActionRelease(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidBuildings.Add(building);
                }
            }
        }

        static void Finalizer()
        {
            UndoMod.Instance.ObservingOnlyBuildings--;
        }
    }

    [HarmonyPatch(typeof(BuildingManager))]
    [HarmonyPatch("CreateBuilding")]
    class BuildingManagerPatch_CreateBuilding
    {
        static void Prefix()
        {
            UndoMod.Instance.ObservingOnlyBuildings++;
        }

        static void Finalizer(bool __result, ref ushort building, Exception __exception)
        {
            UndoMod.Instance.ObservingOnlyBuildings--;
            if (__result && __exception == null && !UndoMod.Instance.PerformingAction && !UndoMod.Instance.Invalidated /*&& CheckCaller()*/) {
                if (UndoMod.Instance.Observing) {
                    try {
                        var constructable = UndoMod.Instance.WrappersDictionary.RegisterBuilding(building);
                        UndoMod.Instance.ReportObservedAction(new ActionCreate(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidBuildings.Add(building);
                }
            }
            if (UndoMod.Instance.ObservingOnlyBuildings == 0) {
                UndoMod.Instance.TerminateObservingIfVanilla();
            }
        }
    }

    [HarmonyPatch(typeof(BuildingManager))]
    [HarmonyPatch("RelocateBuilding")]
    class BuildingManagerPatch_RelocateBuilding
    {
        static void Postfix()
        {
            UndoMod.Instance.InvalidateAll(false);
        }
    }
}