using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace ConnectionFactory
{
    public partial class CfCommand
    {
        #region ExecuteReader
        /// <summary>
        /// Performs datareader
        /// </summary>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>Data Reader</returns> 
        [System.Diagnostics.DebuggerStepThrough]
        public IDataReader ExecuteReader(CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            try
            {
                Logger.LogInformation("ExecuteReader: {0}", cmdText);
                Logger.LogInformation(cmdParms.ToString());

                _conn.EstablishFactoryConnection();
                PrepareCommand(cmdType, cmdText, cmdParms);
                return _cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ExecuteReader", cmdType, cmdText, cmdParms);
                throw new CfException("Unknown Error (Connection Factory: ExecuteReader) " + ex.Message, ex);
            }
        }
        public IDataReader ExecuteReader(CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return ExecuteReader(cmdType, cmdText, cfParams);
        }
        #endregion
    }
}
