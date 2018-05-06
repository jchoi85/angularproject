using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using DbAccess.Tools;

namespace DbAccess.DbAdapter
{
    public class DbAdapter : IDbAdapter
    {
        public IDbCommand Cmd { get; private set; }
        public IDbConnection Conn { get; private set; }


        public DbAdapter(IDbCommand command, IDbConnection connection)
        {
            Cmd = command;
            Conn = connection;
        }

        public List<T> LoadObject<T>(string storedProcedure, IDbDataParameter[] parameters = null) where T: class
        {
            List<T> list = new List<T>();

            using (IDbConnection conn = Conn) // 'using' works as try catch, also has a 'finally' statement ot close connection when done
            using (IDbCommand cmd = Cmd)
            {
                if(conn.State != ConnectionState.Open)
                    conn.Open();

                cmd.Connection = conn;
                cmd.CommandTimeout = 5000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcedure;

                if(parameters != null) foreach(IDbDataParameter param in parameters) cmd.Parameters.Add(param);

                IDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(DataMapper<T>.Instance.MapToObject(reader));
                }
            }

            return list;
        }

        public int ExecuteQuery(string storedProcedure, IDbDataParameter[] parameters, Action<IDbDataParameter[]> returnParameters = null)
        {
            using (IDbConnection conn = Conn)
            using (IDbCommand cmd = Cmd)
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                cmd.CommandTimeout = 5000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcedure;
                cmd.Connection = conn;

                foreach (IDbDataParameter param in parameters) cmd.Parameters.Add(param);

                int returnValue = cmd.ExecuteNonQuery();

                returnParameters?.Invoke(parameters); // simplified version, if not null then 'invoke' (or execute) function

                return returnValue;
            }
        }

        public T ExecuteScalar<T>(string storedProcedure, IDbDataParameter[] parameters)
        {
            using (IDbConnection conn = Conn)
            using (IDbCommand cmd = Cmd)
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                cmd.CommandTimeout = 5000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcedure;
                cmd.Connection = conn;

                foreach (IDbDataParameter param in parameters) cmd.Parameters.Add(param);

                object obj = cmd.ExecuteScalar();

                return (T)obj; // (T) casts obj as a (T)
            }
        }
    }
}
