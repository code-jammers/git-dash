using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GitDash.Services;

public class VariableStorage
{
    private SqliteConnection Connect()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string databasePath = Path.Combine(appDataPath, "git-dash", "variables.db");
        //Console.WriteLine(databasePath);
        if (!Directory.Exists(Path.Combine(appDataPath, "git-dash")))
        {
            Directory.CreateDirectory(Path.Combine(appDataPath, "git-dash"));
        }
        bool createTable = !File.Exists(databasePath);
        var connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        if (createTable)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"create table variables (name text not null, value text not null);";
            command.ExecuteNonQuery();
        }
        return connection;
    }
    public void Add(string name, string value)
    {
        var connection = Connect();

        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = "insert into variables (name, value) VALUES (@name, @value);";
        insertCommand.Parameters.AddWithValue("@name", name);
        insertCommand.Parameters.AddWithValue("@value", value);
        insertCommand.ExecuteNonQuery();

        connection.Close();
    }

    public Dictionary<string, string> GetAll()
    {
        var connection = Connect();

        var nameValues = new Dictionary<string, string>();
        var selectCommand = connection.CreateCommand();
        selectCommand.CommandText = "select name, value from variables;";
        using var reader = selectCommand.ExecuteReader();
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                var name = reader.GetString(0);
                var value = reader.GetString(1);
                nameValues[name] = value;
            }
        }
        reader.Close();
        connection.Close();
        return nameValues;
    }
}
