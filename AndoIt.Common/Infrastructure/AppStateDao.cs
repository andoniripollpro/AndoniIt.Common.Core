using AndoIt.Common.Common;
using AndoIt.Common.Interface;
using Dapper;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Diagnostics;
using System.Linq;

namespace AndoIt.Common.Infrastructure
{
    public class AppStateDao<T>
    {
        private readonly ILog log;
        private readonly string connectionString;
        private readonly string appId;
        private readonly OracleConnection connection;

        private const string SELECT_TEMPLATE = "SELECT STATE FROM APP_STATE WHERE APP_ID='{appId}' ";
        private const string INITIALIZE_INSERT_TEMPLATE = "INSERT INTO APP_STATE (APP_ID, STATE) " +
            "VALUES( '{appId}','{}')";
        private const string UPDATE_TEMPLATE = "UPDATE APP_STATE SET STATE = {clobs} " +
            "WHERE APP_ID = '{appId}' ";

        public AppStateDao(ILog log, string connectionString, string appId)
        {
            try
            {
                this.log = log ?? throw new ArgumentNullException("log");
                this.log.Info("Start", new StackTrace());
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new ArgumentNullException("connectionString");
                this.connectionString = connectionString;
                this.connection = new OracleConnection(this.connectionString);
                if (string.IsNullOrWhiteSpace(appId))
                    throw new ArgumentNullException("appId");
                this.appId = appId;                

                Initilize();

                this.log.Info("End", new StackTrace());
            }
            catch (Exception ex)
            {
                log.Fatal($"Error inicializando: '{ex.Message}'", ex);
                throw;
            }
        }

        private void Initilize()
        {
            this.log.Debug("Start", new StackTrace());
            connection.EnsureDatabaseConnection();
            // Si no existe crea uno vacío
            T state = this.AppState;
            if (state == null)
                InsertEmptyState();
            else
                this.AppState = state;
            this.log.Debug("End", new StackTrace());
        }

        private void InsertEmptyState()
        {
            this.connection.EnsureDatabaseConnection();
            string insertStatement = INITIALIZE_INSERT_TEMPLATE.Replace("{appId}", this.appId);
            this.log.Debug($"Va lanzar este statement: '{insertStatement}'", new StackTrace());
            int result = this.connection.Execute(insertStatement);
            if (result != 1)
                this.log.Fatal($"La sentencia ha devuelto {result}. Debería ser imposible", null, new StackTrace());
            else
                this.log.Debug($"End", new StackTrace());
        }

        public T AppState
        {
            get
            {
                T appState = default(T);
                this.connection.EnsureDatabaseConnection();
                string query = SELECT_TEMPLATE.Replace("{appId}", this.appId);
                this.log.Debug($"Va lanzar esta query: '{query}'", new StackTrace());
                var stateList = this.connection.Query<string>(query).ToList();
                string stateStr = string.Empty;
                if (stateList.Count == 1)
                    stateStr = stateList[0];
                else
                    this.log.Warn($"No existía estado en la tabla.", null, new StackTrace());
                this.log.Info($"stateStr='{stateStr}'");
                try { appState = JsonConvert.DeserializeObject<T>(stateStr); } catch (Exception ex) { this.log.Error("No se puede deserializar", ex); }
                this.log.Info($"AppState='{JsonConvert.SerializeObject(appState)}'");
                return appState;
            }
            set
            {
                this.connection.EnsureDatabaseConnection();
                string appStateStr = "{}";
                try { appStateStr = JsonConvert.SerializeObject(value); } catch { appStateStr = "{}"; };
                if (appStateStr == "null") appStateStr = "{}";
                appStateStr = appStateStr.Replace("'", "''");
                this.log.Debug($"AppState='{appStateStr}'", new StackTrace());
                string clobs = FromOneStringToNClobs(appStateStr);
                string updateStatement = UPDATE_TEMPLATE.Replace("{appId}", this.appId).Replace("{clobs}", clobs);                
                this.log.Debug($"Va lanzar este statement: '{updateStatement}'", new StackTrace());
                int result = this.connection.Execute(updateStatement);
                if (result != 1)
                    this.log.Fatal($"La sentencia ha devuelto {result}. Debería ser imposible", null, new StackTrace());
                else
                    this.log.Debug($"End", new StackTrace());
            }
        }

        private string FromOneStringToNClobs(string appStateStr)
        {
            if (appStateStr.Length <= 3900)
                return $" to_clob('{appStateStr}')";
            else
            {
                string firstClob = appStateStr.Substring(0, 3900 - 1);
                string restOfClobs = appStateStr.Substring(3900, appStateStr.Length -3900);
                string result = $"to_clob('{firstClob}') || {FromOneStringToNClobs(restOfClobs)}";
                return result;
            }
        }
    }
}
