using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.HmiConnections;
using System;

namespace TIAEKtool.Alarms
{
    public static class HmiUtils
    {
        public static HmiConnection FindPlcConnection(HmiSoftware hmi_software, MessageLog log, out string target_id, out string target_label)
        {
            // Find PLC connection

            HmiConnection plc_connection = null;
            target_id = null;
            target_label = null;
            foreach (var connection in hmi_software.Connections)
            {
                if (connection.CommunicationDriver.StartsWith("SIMATIC S7"))
                {
                    if (plc_connection != null)
                    {
                        log?.LogMessage(MessageLog.Severity.Error, "More than one PLC connection found, don't know which one to use.");
                        return null;
                    }
                    plc_connection = connection;



                }
            }

            if (plc_connection == null)
            {
                log?.LogMessage(MessageLog.Severity.Error, "No PLC connection found.");
                return null;
            }


            string[] parts = null;
            CommentParser.Parse(plc_connection.Comment, (type, data) =>
            {
                if (type == "alarm_target")
                {
                    parts = data.Split(new char[] { ':' }, 2);
                   
                }
            });

            if (parts == null)
            {
                log?.LogMessage(MessageLog.Severity.Error, "The PLC connection " + plc_connection.Name + " has no target (alarm_target)");
                return null;
            }

            if (parts.Length < 2)
            {
                log?.LogMessage(MessageLog.Severity.Error, "alarm_target argument must contain ':'");
                return null;
            }
            target_id = parts[0].Trim();
            target_label = parts[1].Trim();

            log?.LogMessage(MessageLog.Severity.Info, "Using connection " + plc_connection.Name + " for target " + target_id);
            return plc_connection;
        }
        
}
}
