using Npgsql;

namespace MyZhiHuAPI.Helpers;

public class DbHelper(IConfiguration configuration)
{
    public NpgsqlConnection OpenConnection()
    {
        var conn = new NpgsqlConnection(configuration["DbSetting:ConnectionString"]);
        conn.Open();
        return conn;
    }
}
