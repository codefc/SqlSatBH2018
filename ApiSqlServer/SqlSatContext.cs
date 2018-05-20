using ApiSqlServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiSqlServer
{
    public class SqlSatContext : DbContext
    {
        public DbSet<Pessoa> Pessoa { get; set; }
        
        public SqlSatContext(DbContextOptions<SqlSatContext> options)
        :base(options)
        {
        }
    }
}