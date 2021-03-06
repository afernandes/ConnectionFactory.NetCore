﻿using System;
using System.Collections.Generic;
using System.Data;

namespace ConnectionFactory
{
    public partial class CfCommand
    {
        #region QueryForList

        #region Generic methods
        /// <summary>
        /// Datareader Performs and returns the list loaded entities
        /// </summary>
        /// <typeparam name="T">type of entity returned</typeparam>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>list of entities</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public IList<T> QueryForList<T>(
            CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null) where T : new()
        {
            return QueryForList<T>(ExecuteReader(cmdType, cmdText, cmdParms));
        }

        public IList<T> QueryForList<T>(
            CfCommandType cmdType, string cmdText, object cmdParms) where T : new()
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return QueryForList<T>(cmdType, cmdText, cfParams);
        }

        public IList<T> QueryForList<T>(
            string cmdText, IEnumerable<CfParameter> cmdParms = null) where T : new()
        {
            return QueryForList<T>(CfCommandType.Text, cmdText, cmdParms);
        }

        public IList<T> QueryForList<T>(
            string cmdText, object cmdParms) where T : new()
        {
            return QueryForList<T>(CfCommandType.Text, cmdText, cmdParms);
        }

        /// <summary>
        /// Datareader Performs and returns the list loaded entities
        /// </summary>
        /// <typeparam name="T">type of entity returned</typeparam>
        /// <param name="dr">datareader loaded</param>
        /// <returns>list of entities</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static IList<T> QueryForList<T>(IDataReader dr) where T : new()
        {
            Type entityType = typeof(T);
            var entitys = new List<T>();
            try
            {
                var properties = CfMapCache.GetInstance(entityType);
                while (dr.Read())
                {
                    var newObject = new T();
                    for (var index = 0; index < dr.FieldCount; index++)
                    {
                        if (properties.ContainsKey(dr.GetName(index).ToUpper()))
                        {
                            var info = properties[dr.GetName(index).ToUpper()];
                            if ((info != null) && info.CanWrite && !dr.IsDBNull(index))
                            {
                                try
                                {
                                    info.SetValue(newObject, dr.GetValue(index), null);
                                }
                                catch (Exception ex)
                                {
                                    //Logger.Error(ex);
                                    throw new CfException(String.Format("Conversion error in {0}.{1} - (Connection Factory: QueryForList) > {2}", entityType.Name, dr.GetName(index), ex.Message), ex);
                                }
                            }
                        }
                        else
                        {
                            //Logger.Debug("Property not exist: " + entityType.FullName + dr.GetName(index));
                        }
                    }
                    entitys.Add(newObject);
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
                throw new CfException("Unknown Error (Connection Factory: QueryForList) " + ex.Message, ex);
            }
            finally
            {
                dr.Close();
            }
            //Logger.Debug("End method");

            return (entitys.Count > 0) ? entitys : default(List<T>);

        }

        #endregion

        #region Dynamic methods
        /// <summary>
        /// Datareader Performs and returns the list loaded entities
        /// </summary>
        /// <param name="cmdType">Command Type (text, procedure or table)</param>
        /// <param name="cmdText">Command Text, procedure or table name</param>
        /// <param name="cmdParms">Command Parameters (@parameter)</param>
        /// <returns>list of entities (ExpandoObject)</returns>
        public IList<dynamic> QueryForList(
            CfCommandType cmdType, string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            return QueryForList(ExecuteReader(cmdType, cmdText, cmdParms));
        }

        public IList<dynamic> QueryForList(
            CfCommandType cmdType, string cmdText, object cmdParms)
        {
            var cfParams = ConvertObjectToCfParameters(cmdParms);
            return QueryForList(cmdType, cmdText, cfParams);
        }

        public IList<dynamic> QueryForList(
            string cmdText, IEnumerable<CfParameter> cmdParms = null)
        {
            return QueryForList(CfCommandType.Text, cmdText, cmdParms);
        }

        public IList<dynamic> QueryForList(
            string cmdText, object cmdParms)
        {
            return QueryForList(CfCommandType.Text, cmdText, cmdParms);
        }

        /// <summary>
        /// Datareader Performs and returns the list loaded entities
        /// </summary>
        /// <param name="dr">datareader loaded</param>
        /// <returns>list of entities (ExpandoObject)</returns>
        public static IList<dynamic> QueryForList(IDataReader dr)
        {
            var returnValue = new List<dynamic>();

            while (dr.Read())
            {
                returnValue.Add(dr.ToExpando());
            }

            return (returnValue.Count > 0) ? returnValue : default(List<dynamic>); ;
        }
        
        #endregion
        
        #endregion
    }
}
