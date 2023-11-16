using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;

namespace GrowUp.Data
{
    [Serializable]
    public abstract class Target
    {
        // TODO: change target system from a "does this match?" method to a
        // "here's a list of all objects, give me all the matches" method
        public abstract bool Match(GameObject gameObject);
        public abstract string TargetString();

        [Serializable]
        public class CharacterName : Target
        {
            private static List<ObjectKind> ObjectKinds = new() {
                ObjectKind.Player,
                ObjectKind.BattleNpc,
                ObjectKind.EventNpc,
            };
            public string name;

            public CharacterName(string name) {
                this.name = name;
            }

            public override bool Match(GameObject gameObject) {
                return ObjectKinds.Contains(gameObject.ObjectKind) &&
                    gameObject.Name.ToString() == name;
            }

            public override string TargetString() => name;
        }
    }
}
