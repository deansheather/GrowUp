using System;
using System.Numerics;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Windowing;
using GrowUp.Services;
using ImGuiNET;

namespace GrowUp.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Plugin plugin { get; init; }

    public ConfigWindow(Plugin plugin) : base(
        "Grow Up",
        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
        this.plugin = plugin;
        Size = new Vector2(500, 350);
        SizeCondition = ImGuiCond.Appearing;
    }

    public void Dispose() { }

    public override void Draw() {
        ImGui.Text("Rules:");
        ImGui.SameLine();
        if (ImGui.Button("New")) {
            DalamudServices.Log.Information("New button pressed");
            plugin.OpenCreateRuleUI();
        }

        // Scrolling table of rules
        ImGui.BeginChild("Rules", new Vector2(0, 0), true);
        ImGui.Columns(4, "Rules"); // enabled, target, scale_property, edit/delete
        ImGui.Text("Enabled");
        ImGui.NextColumn();
        ImGui.Text("Target");
        ImGui.NextColumn();
        ImGui.Text("Scale");
        ImGui.NextColumn();
        ImGui.Text("Actions");
        ImGui.NextColumn();
        ImGui.Separator();

        var shouldSave = false;
        for (var i = 0; i < ConfigurationService.Config.Rules.Count; i++) {
            ImGui.PushID($"##rule{i}");
            var rule = ConfigurationService.Config.Rules[i];

            var toggledEnable = ImGui.Checkbox("", ref rule.Enabled);
            ImGui.NextColumn();
            ImGui.Text(rule.Target?.TargetString() ?? "None");
            ImGui.NextColumn();
            ImGui.Text(rule.Properties.Scale.ToString());
            ImGui.NextColumn();
            var isEditing = ImGui.Button("Edit");
            ImGui.SameLine();
            var isDeleting = ImGui.Button("Delete");
            if (ImGui.IsItemHovered()) {
                ImGui.BeginTooltip();
                ImGui.Text("Hold Ctrl+Shift to delete");
                ImGui.EndTooltip();
            }
            ImGui.NextColumn();

            // Event handlers
            if (toggledEnable) {
                shouldSave = true;
            }
            if (isEditing) {
                DalamudServices.Log.Information("Edit button pressed");
                plugin.OpenEditRuleUI(rule.ID);
            }
            if (isDeleting && ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift) {
                DalamudServices.Log.Information("Delete button pressed");
                ConfigurationService.Config.Rules.RemoveAt(i);
                DalamudServices.PluginInterface.UiBuilder.AddNotification("[Grow Up] Deleted rule", null, NotificationType.Success);
                shouldSave = true;
            }

            ImGui.PopID();
        }

        if (shouldSave) {
            ConfigurationService.Save();
        }
    }
}
