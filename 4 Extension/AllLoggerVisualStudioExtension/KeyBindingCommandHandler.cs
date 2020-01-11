using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace AllLoggerVisualStudioExtension {
    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name("KeyBinding")]
    internal class
        KeyBindingCommandHandler : ICommandHandler<TypeCharCommandArgs> {
        public string DisplayName => "KeyBinding";

        public CommandState GetCommandState(TypeCharCommandArgs args) {
            return CommandState.Unspecified;
        }

        public bool ExecuteCommand(TypeCharCommandArgs args,
            CommandExecutionContext executionContext) {
            KeyMonitor.Instance.KeyPressed(args);
            return false;
        }
    }
}