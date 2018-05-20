using System.Collections.Generic;
using ApiSqlServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ApiSqlServer.Controllers
{
    [Route("/api/[controller]")]
    public class PessoaController : Controller
    {
        private readonly SqlSatContext _dbContext;

        public PessoaController(SqlSatContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<Pessoa> Get()
        {
            return _dbContext.Pessoa.ToList();
        }
    }
}