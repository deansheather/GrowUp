using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using GrowUp.Windows;
using GrowUp.Services;
using System;

namespace GrowUp
{
    public sealed class Plugin : IDalamudPlugin
    {
        internal static string Name => "Grow Up";
        private const string CommandName = "/growup";

        public WindowSystem WindowSystem = new("GrowUp");

        private ConfigWindow ConfigWindow { get; init; }

        private RuleWindow RuleWindow { get; init; }

        private ObjectService objectService { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface
        ) {
            pluginInterface.Create<DalamudServices>();
            ConfigurationService.Load();

            // serialize json and log it
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ConfigurationService.Config, Newtonsoft.Json.Formatting.Indented);
            DalamudServices.Log.Debug(json);

            objectService = new ObjectService();

            ConfigWindow = new ConfigWindow(this);
            WindowSystem.AddWindow(ConfigWindow);

            RuleWindow = new RuleWindow();
            WindowSystem.AddWindow(RuleWindow);

            DalamudServices.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) {
                HelpMessage = "Open Grow Up configuration window"
            });

            DalamudServices.PluginInterface.UiBuilder.Draw += DrawUI;
            DalamudServices.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUI;
        }

        public void Dispose() {
            DalamudServices.CommandManager.RemoveHandler(CommandName);

            objectService.Dispose();

            WindowSystem.RemoveAllWindows();
            RuleWindow.Dispose();
            ConfigWindow.Dispose();
        }

        private void OnCommand(string command, string args) {
            OpenConfigUI();
        }

        private void DrawUI() {
            WindowSystem.Draw();
        }

        public void OpenConfigUI() {
            ConfigWindow.IsOpen = true;
        }

        public void OpenCreateRuleUI() {
            RuleWindow.CreateRule();
        }

        public void OpenEditRuleUI(string ruleID) {
            if (ruleID == null || ruleID == "") {
                throw new ArgumentException("ruleID cannot be null or empty");
            }
            RuleWindow.EditRule(ruleID);
        }
    }
}
