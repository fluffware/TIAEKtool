using Siemens.Engineering;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Xml;

namespace TIAEKtool
{
    public class CreatePlcCompileTask: SequentialTask
    {
        TiaPortal portal;
       
        PlcSoftware plc;
        public CreatePlcCompileTask(TiaPortal portal, PlcSoftware plc)
        {
            this.portal = portal;
            this.plc = plc;
          
           
            Description = "Complie PLC software for " + plc.Name;
        }

        private MessageLog.Severity compiler_state_to_severity(CompilerResultState state)
        {
            switch (state)
            {
                case CompilerResultState.Success:
                    return MessageLog.Severity.None;
                case CompilerResultState.Information:
                    return MessageLog.Severity.Info;
                case CompilerResultState.Warning:
                    return MessageLog.Severity.Warning;
                case CompilerResultState.Error:
                    return MessageLog.Severity.Error;
                default:
                    return MessageLog.Severity.Debug;
            }
        }
        private void LogCompilerMessage(CompilerResultMessage msg) {
            LogMessage(compiler_state_to_severity(msg.State), msg.Path + ": " + msg.Description);
        }

        private void LogCompilerMessages(CompilerResultMessageComposition msgs)
        {
            foreach (var msg in msgs)
            {
                LogCompilerMessage(msg);
                LogCompilerMessages(msg.Messages);
            }
        }

        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {
                    ICompilable compileService = plc.GetService<ICompilable>();
                    CompilerResult res = compileService.Compile();
                    if (res.State != CompilerResultState.Success)
                    {
                        LogCompilerMessages(res.Messages);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to compile:\n" + ex.Message);
                    return;
                }

              
            }
        }
    }
}