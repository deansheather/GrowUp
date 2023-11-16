using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using GrowUp.Data;
using GrowUp.Services;
using ImGuiNET;

namespace GrowUp.Windows;

public class RuleWindow : Window, IDisposable
{
    public string ruleID = ""; // blank means "create new"
    public bool enabled = true;
    // TODO: this isn't good targetting code but it's good for MVP
    public string characterName = "";
    public float scale = 1.0f ;

    public RuleWindow() : base(
        $"Grow Up - Rule Editor",
        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
        Size = new Vector2(500, 350);
        SizeCondition = ImGuiCond.Appearing;
    }

    public void CreateRule() {
        ruleID = "";
        enabled = true;
        characterName = "";
        scale = 1.0f;
        IsOpen = true;
    }

    public void EditRule(string ruleID) {
        if (ruleID == null || ruleID == "") {
            throw new Exception($"Invalid rule ID {ruleID}");
        }

        var rule = ConfigurationService.Config.Rules.Find(r => r.ID == ruleID);
        if (rule == null) {
            throw new Exception($"Rule with ID {ruleID} not found");
        }
        if (rule.Target is not Target.CharacterName) {
            throw new Exception($"Rule with ID {ruleID} has invalid target type");
        }

        this.ruleID = rule.ID;
        enabled = rule.Enabled;
        characterName = rule.Target?.TargetString() ?? "";
        scale = rule.Properties.Scale;
        IsOpen = true;
    }

    public void Dispose() { }

    public override void Draw() {
        if (ruleID == "") {
            ImGui.Text("New Rule:");
        } else {
            ImGui.Text("Edit Rule:");
        }

        ImGui.Checkbox("##enabled", ref enabled);
        ImGui.SameLine();
        ImGui.Text("Enabled");

        ImGui.Text("Target:");
        ImGui.SameLine();
        ImGui.InputText("##target", ref characterName, 32);
        ImGui.SameLine();
        if (ImGui.Button("Use Target") && DalamudServices.ClientState.LocalPlayer != null) {
            DalamudServices.Log.Info("Use Target button pressed");
            var target = DalamudServices.TargetManager.Target;
            if (target != null) {
                DalamudServices.Log.Info($"target {target.Name.ToString()}");
                characterName = target.Name.ToString();
            }
        }

        ImGui.Text("Scale:");
        ImGui.SameLine();
        ImGui.InputFloat("##scale", ref scale);

        ImGui.NewLine();
        if (ImGui.Button("Save")) {
            Save();
            Close();
        }

        foreach (var problem in Validate()) {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), problem);
        }
    }

    public List<string> Validate() {
        var problems = new List<string>();
        if (characterName == "") {
            problems.Add("Character name cannot be blank");
        }
        if (scale <= 0) {
            problems.Add("Scale must be greater than 0");
        }

        return problems;
    }

    public void Save() {
        if (Validate().Count > 0) {
            return;
        }

        var rule = new Rule {
            ID = ruleID,
            Enabled = enabled,
            Target = new Target.CharacterName(characterName),
            Properties = new ObjectProperties {
                Scale = scale,
            },
        };
        if (ruleID == "") {
            rule.ID = Guid.NewGuid().ToString();
            ConfigurationService.Config.Rules.Add(rule);
        } else {
            var index = ConfigurationService.Config.Rules.FindIndex(r => r.ID == ruleID);
            if (index == -1) {
                throw new Exception($"Rule with ID {ruleID} not found");
            }
            ConfigurationService.Config.Rules[index] = rule;
        }

        ConfigurationService.Save();
    }

    public void Close() {
        IsOpen = false;
    }
}

