﻿using ColossalFramework;
using ICities;
using System;
using UndoMod.UI;

namespace UndoMod
{
    public class Threading : ThreadingExtensionBase
    {
        private static bool _processed = false;

        private static DateTime _lastBeginObserving = DateTime.Now;

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (ModInfo.sc_undo.IsPressed() && !_processed)
            {
                if(CheckCurrentTool())
                    Singleton<SimulationManager>.instance.AddAction(() => {
                        UndoMod.Instance.Undo();
                    });
                _processed = true;
            } else if (ModInfo.sc_redo.IsPressed() && !_processed)
            {
                if (CheckCurrentTool())
                    Singleton<SimulationManager>.instance.AddAction(() => {
                        UndoMod.Instance.Redo();
                    });
                _processed = true;
            } else if (ModInfo.sc_peek.IsPressed() && !_processed)
            {
                if (CheckCurrentTool()) PeekUndoPanel.Instance.Enable();
                _processed = true;
            } else
            {
                PeekUndoPanel.Instance.Disable();
                _processed = false;
            }

            ScheduledObserving();
        }

        private bool CheckCurrentTool()
        {
            if(!ModInfo.sa_disableShortcuts.value) return true;

            ToolBase tool = ToolsModifierControl.toolController.CurrentTool;

            return (
                tool is DefaultTool ||
                tool is NetTool ||
                tool is BuildingTool ||
                tool is PropTool ||
                tool is TreeTool ||
                tool.GetType().Name == "ForestTool");
        }

        private static void ScheduledObserving()
        {
            if(LoadingExtension.Instance.m_detoured)
            {
                if(_lastBeginObserving.AddSeconds(1) < DateTime.Now)
                {
                    _lastBeginObserving = DateTime.Now;
                    Singleton<SimulationManager>.instance.AddAction(() => {
                        UndoMod.Instance.BeginObserving("<unknown>", "", true);
                    });
                }
            }
        }
    }
}
