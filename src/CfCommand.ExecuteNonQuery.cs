using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace ConnectionFactory
{
    public partial class CfCommand
    {
        #region ExecuteNonQuery
        //[System.Diagnostics.DebuggerStepThrough]
        public int ExecuteNonQuery(CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            Logger.LogDebug("Begin Method");
            try
            {
                Logger.LogInformation("ExecuteNonQuery: {0}", cmdText);
                Logger.LogInformation(cmdParms.ToString());

                _conn.EstablishFactoryConnection();
                PrepareCommand(cmdType, cmdText, cmdParms);

                Logger.LogDebug("End Method");
                return _cmd.ExecuteNonQuery();
            }
            catch (DbException dbe)
            {
                Logger.LogError(dbe, "ExecuteNonQuery",cmdType, cmdText, cmdParms);
                throw new CfException("Unknown error in database: " + dbe.Message, dbe);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ExecuteNonQuery", cmdType, cmdText, cmdParms);
                throw new CfException("Error on Connection Factory Mechanism: " + ex.Message, ex);
            }
        }
        public int ExecuteNonQuery(CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return ExecuteNonQuery(cmdType, cmdText, cfParams);
        }
        #endregion
    }

}