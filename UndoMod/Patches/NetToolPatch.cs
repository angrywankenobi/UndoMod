using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace UndoMod.Patches
{
    [HarmonyPatch(typeof(NetTool))]
    [HarmonyPatch("CreateNodeImpl", new Type[] { typeof(bool) })]
    class NetToolPatch
    {
        static void Prefix()
        {
            UndoMod.Instance.BeginObserving("Build roads");
        }

        static void Finalizer(Exception __exception)
        {
            UndoMod.Instance.FinalizeObserving(__exception);
        }
    }
}