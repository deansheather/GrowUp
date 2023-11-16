using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;

namespace GrowUp.Services
{
    [Flags]
    public enum DrawState : uint
    {
        Invisibility = 0x00_00_00_02,
        IsLoading = 0x00_00_08_00,
        SomeNpcFlag = 0x00_00_01_00,
        MaybeCulled = 0x00_00_04_00,
        MaybeHiddenMinion = 0x00_00_80_00,
        MaybeHiddenSummon = 0x00_80_00_00,
    }

    public enum ScreenActor : ushort
    {
        CutsceneStart = 200,
        GPosePlayer = 201,
        CutsceneEnd = 240,
        CharacterScreen = CutsceneEnd,
        ExamineScreen = 241,
        FittingRoom = 242,
        DyePreview = 243,
        Portrait = 244,
        Card6 = 245,
        Card7 = 246,
        Card8 = 247,
        ScreenEnd = Card8 + 1,
    }

    public unsafe class RedrawService : IDisposable
    {
        private readonly List<int> invisibleQueue = new();
        private readonly List<int> visibleQueue = new();

        public RedrawService() {
            DalamudServices.Framework.Update += OnUpdate;
        }

        public void Dispose() {
            DalamudServices.Framework.Update -= OnUpdate;
        }

        public void QueueRedrawObject(int idx) {
            invisibleQueue.Add(idx);
        }

        public static DrawState* ActorDrawState(GameObject actor)
            => (DrawState*)(&((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)actor.Address)->RenderFlags);

        private void WriteVisibility(GameObject? actor, int tableIndex, bool visible) {
            // Do not ever redraw any of the UI Window actors.
            if (tableIndex is >= (int)ScreenActor.CharacterScreen and <= (int)ScreenActor.Card8) {
                return;
            }

            if (visible) {
                *ActorDrawState(actor!) &= ~DrawState.Invisibility;
            } else {
                *ActorDrawState(actor!) |= DrawState.Invisibility;
            }

            if (actor is PlayerCharacter && DalamudServices.ObjectTable[tableIndex + 1] is { ObjectKind: ObjectKind.MountType } mount) {
                WriteVisibility(mount, tableIndex + 1, visible);
            }
        }

        private void OnUpdate(IFramework framework) {
            ProcessQueue(visibleQueue, true);
            ProcessQueue(invisibleQueue, false);
        }

        private void ProcessQueue(List<int> queue, bool visible) {
            if (queue.Count == 0) {
                return;
            }

            for (var i = 0; i < queue.Count; i++) {
                var idx = queue[i];
                var obj = DalamudServices.ObjectTable[idx];
                if (obj != null) {
                    if (visible) {
                        DalamudServices.Log.Debug($"Making {obj.Name} (idx={idx}, id={obj.ObjectId}, data={obj.DataId}) visible");
                        WriteVisibility(obj, idx, true);
                    } else {
                        DalamudServices.Log.Debug($"Making {obj.Name} (idx={idx}, id={obj.ObjectId}, data={obj.DataId}) INvisible");
                        WriteVisibility(obj, idx, false);
                        visibleQueue.Add(idx);
                    }
                }
            }

            queue.Clear();
        }
    }
}
