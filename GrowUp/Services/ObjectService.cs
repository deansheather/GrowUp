using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using System;
using System.Collections.Generic;

namespace GrowUp.Services
{
    internal class ObjectService : IDisposable
    {
        private RedrawService redrawer { get; init; }

        // A list of objects we've touched since last region change.
        // (ObjectId, DataId)
        private readonly List<(uint, uint)> managedObjects = new();

        public ObjectService()
        {
            DalamudServices.Framework.Update += OnUpdate;
            DalamudServices.ClientState.TerritoryChanged += OnTerritoryChanged;
            redrawer = new RedrawService();
        }

        private void OnTerritoryChanged(ushort _)
        {
            DalamudServices.Log.Info("changed territory, cleared managed object list");
            managedObjects.Clear();
        }

        public void Dispose()
        {
            DalamudServices.Framework.Update -= OnUpdate;
            redrawer.Dispose();
        }

        private static int ObjectsPerUpdate = 25;
        private int index = 0;

        public void OnUpdate(IFramework framework)
        {
            for (var i = index; i < index + ObjectsPerUpdate; i++)
            {
                var obj = DalamudServices.ObjectTable[i];
                if (obj == null)
                {
                    if (i >= DalamudServices.ObjectTable.Count)
                    {
                        index = 0;
                        return;
                    }
                    continue;
                }

                //DalamudServices.Log.Debug($"obj id={obj.ObjectId} data={obj.DataId} kind={obj.ObjectKind} name={obj.Name}");

                var fullObjectID = (obj.ObjectId, obj.DataId);
                var known = managedObjects.Contains(fullObjectID);
                var didMatch = false;
                var wantScale = 1.0f;
                foreach (var rule in ConfigurationService.Config.Rules)
                {
                    if (rule.Enabled && rule.Target is not null && rule.Target.Match(obj))
                    {
                        didMatch = true;
                        wantScale = rule.Properties.Scale;
                        break;
                    }
                }

                if (known || didMatch)
                {
                    try
                    {
                        unsafe
                        {
                            var gameObject = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Address;
                            if (gameObject == null || gameObject->DrawObject == null)
                            {
                                return;
                            }

                            var gotScale = gameObject->Scale;

                            if (wantScale != gotScale)
                            {
                                DalamudServices.Log.Debug("Scaling object " + obj.Name + " from " + gotScale + " to " + wantScale);
                                gameObject->Scale = wantScale;
                                gameObject->VfxScale = wantScale;
                                gameObject->DrawObject->Object.Scale = new Vector3(wantScale, wantScale, wantScale);

                                redrawer.QueueRedrawObject(i);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        DalamudServices.Log.Error($"Error scaling object (name={obj.Name}): {e}");
                    }
                }

                if (didMatch && !known)
                {
                    managedObjects.Add(fullObjectID);
                }
                else if (!didMatch && known)
                {
                    managedObjects.Remove(fullObjectID);
                }
            }

            index += ObjectsPerUpdate;
        }
    }
}
