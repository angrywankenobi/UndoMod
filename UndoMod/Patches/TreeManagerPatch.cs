using HarmonyLib;
using SharedEnvironment;
using System;
using UndoMod.Utils;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(TreeManager))]
    [HarmonyPatch("ReleaseTree")]
    class TreeManagerPatch_ReleaseTree
    {
        static void Prefix(uint tree)
        {
            ref TreeInstance data = ref ManagerUtils.Tree(tree);
            if (data.m_flags != 0 && PatchUtil.CheckIfObserving()) {
                if (UndoMod.Instance.Observing) {
                    try {
                        var constructable = UndoMod.Instance.WrappersDictionary.RegisterTree(tree);
                        constructable.ForceSetId(0);
                        UndoMod.Instance.ReportObservedAction(new ActionRelease(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instance.InvalidateAll();
                    }
                } else {
                    //Invalidator.Instance.InvalidTrees.Add(tree);
                }
            }
        }
    }

    [HarmonyPatch(typeof(TreeManager))]
    [HarmonyPatch("CreateTree")]
    class TreeManagerPatch_CreateTree
    {
        static void Postfix(bool __result, ref uint tree)
        {
            if (__result && PatchUtil.CheckIfObserving()) {
                if (UndoMod.Instance.Observing) {
                    try {
                        var constructable = UndoMod.Instance.WrappersDictionary.RegisterTree(tree);
                        UndoMod.Instance.ReportObservedAction(new ActionCreate(constructable));
                    }
                    catch (Exception e) {
                        Debug.Log(e);
                        UndoMod.Instance.InvalidateAll();
                    }
                } else {
                    //
                }
            }
            UndoMod.Instance.TerminateObservingIfVanilla();
        }
    }
}
