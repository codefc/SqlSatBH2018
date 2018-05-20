# SqlSatBH2018
Repositório contendo uma aplicação em aspnet core 2.0, desenvolvida em modo console, editada no Visual Studio Code, para apresentação da minha palestra no SQL Saturday BH 2018.

# Pre-requisitos

Para reprodução do projeto existente neste repositório através das instruções abaixo, você deve possuir em sua máquina:

- SDK .Net core
- Visual Studio, Visual Studio for Mac, Visual Studio Code ou algum editor de sua preferência
- Docker CE instalado em sua máquina
- Microsoft Sql Server Operation Studio

# Aspnet core

## Criação do projeto

Para criar o projeto, abra o prompt de comando e em um diretório de sua preferência, digite o seguinte:

```bash
dotnet new webapi -n ApiSqlServer
```

O comando acima cria um projeto **web api** com o nome de **ApiSqlServer**.

Agora acesse o diretório criado com o nome do projeto:

```bash
cd ApiSqlServer
```

## Nuget Package

Neste projeto, vamos conectar na base de dados do Sql Server Linux, em um container Docker. Para abstrair o acesso a dados através da aplicação, vamos utilizar o **Nuget Package** **Microsoft.EntityFramework.Core**.

Execute o seguinte comando:

```bash
dotnet add package Microsoft.EntityFramework.Core
```

Tudo certo. Agora é hora de editar o projeto. Abra o editor de preferência na pasta do projeto.

## Objetos de acesso a dados

Para acessar o banco de dados, devemos criar algumas classes. Para isso, na Raiz do projeto, crie uma pasta chamada **Models**. Em seguida, nesta pasta, crie uma **classe** chamada **Pessoa.cs**:

```csharp
namespace ApiSqlServer.Models
{
    public class Pessoa
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }
}
```
A classe **Pessoa** representará uma tabela da base de dados. Para facilitar o exemplo, será utilizada a convenção do Entity Framework, onde haverá a inferência do nome da tabela para **Pessoa**, a chave par **Id**(padrão do Entity) e, obviamente, a propriedade **Nome** representará a coluna nome.

Na raiz do projeto, crie uma **classe** chamada **SqlSatContext.cs**:

```csharp
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
```

A classe acima é uma abstração do DbContext onde necessitamos apenas de criar as propriedades do tipo **DbSet** para determinar que na base de dados existe uma tabela **Pessoa**. Além disso, temos que criar o contrutor da classe recebendo como parâmetro o tipo **DbContextOptions<SqlSatContext>**. Desta forma, ao injetar o contexto no container de injeção de dependência, já teremos condições de informar a **Connection String**.

## Injeção de dependência

O aspnet core trabalha nativamente com injeção de dependência. Esse partão premite o desacoplamento. Agora, será necessário configurar o DbContext no container e injeção de dependência. Desta forma, quando necessário, podemos utilizar o acesso à base de dados apenas solicitando ao container uma instância da calsse SqlSatContext.

Abra o arquivo **Startup.cs**. Localize o método **ConfigurationServices** e substitua-o para o código abaixo:

```csharp
public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<SqlSatContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddMvc();
        }
```

No código acima, apenas adicionamos o **SqlSatContext** ao container de injeção de dependências e informamos a string de conexão com a base de dados através do parâmetro **options**. Observe que é utilizada a obtenção da configuração através do método **GetConnectionString**.

Não esqueça de adicionar a referência para o namespace do EntityFramework:

```csharp
using Microsoft.EntityFrameworkCore;
```

Agora é necessário configurar a **Connection String**.

Abra o arquivo **appsettings.json** e substitua seu conteúdo pelo código abaixo:

```json
{
  "Logging": {
    "IncludeScopes": false,
    "Debug": {
      "LogLevel": {
        "Default": "Warning"
      }
    },
    "Console": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost,1433;user id=sa;password=SqlSaturdayBH_2018;initial catalog=sat2018"
  }
}
```

A **string de conexão** definida já contém os parâmetros da base de dados do Sql Server Linux que será executada através do Docker.

## Controller

Com os acessos ao banco de dados criados, é hora de criar um serviço que nos permita visualizar a interação da aplicação como Sql Server.

Na pasta **Controllers**, crie um novo arquivo chamado **PessoaController.cs**:

```csharp
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
```

De maneira bem simples, apenas criamos uma propriedade com o DbContext e delegamos a responsabilidade de obtenção de sua instância ao Container de Injeção de Dependência do Aspnet core, ao definir o contrutor da controller com o DbContext como parâmetro.

O método Get apenas faz uma consulta na tabela de pessoas e retorna o seu conteúdo.

# Docker

Com a aplicação criada, é hora de executar o banco de dados. A tafefa será muito simples. Execute a seguinte instrução no prompt de comando:

```bash
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=SqlSaturdayBH_2018' -p 1433:1433 --name sql1   -d microsoft/mssql-server-linux:2017-latest
````

- **ACCEPT_EULA=Y** - apenas estamos aceitamos os termos de uso. Este container será executado em modo Developer Edition.
- ***SA_PASSWORD=SqlSaturdayBH_2018** - definindo a senha do usuário **sa**.
- **-p 1433:1433** - Definição da porta 1433 no **host**(sua máquina), apontando para a porta 1433 do **container**.
- **-d** - execução do container em background.
- **microsoft/mssql-server-linux:2017-latest** - imagem do Sql Server for linux, obtida no registry **Docker Hub**.

Após a execução do comando, um código **Hash** deve ter sido gerado em seu prompt.

Para verificar se o container está em execução, execute o comando:

```bash
docker ps
````

Será exebida uma linha com dados do seu container.

## Banco de dados e tabela

Para a aplicação funcionar corretamente, devemos criar a base de dados e, consequentemente, a tabela na qual faremos consultas.

Para isso, abra o Microsoft Sql Server Operation Studio e conecte na base de dados com os parâmetros:

- **Server**: localhost,1433
- **Authentication type**: SQL Login
- **User name**: sa
- **Password**: SqlSaturdayBH_2018

Crie uma nova query e, em seguida, digite o script abaixo:

```sql
CREATE DATABASE Sat2018;
GO

USE Sat2018;
GO

CREATE TABLE Pessoa (
    Id INT NOT NULL,
    Nome VARCHAR(50) NOT NULL
)
GO
```

Tudo pronto. Agora podemos conectar no banco de dados pela aplicação!

# Executando a aplicação

Ainda no prompt de comando e no diretório da aplicação criada, digite o seguinte comando para buildar a aplicação:

```bash
dotnet build
```

Em seguida o digite:

```bash
dotnet run
```

A saída do comando acima deve ser:

```bash
Hosting environment: Production
Content root path: /Users/albert/Documents/Projetos/2018/Sat2018/SqlSatBH2018/ApiSqlServer
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
````

A aplicação encontra-se em execução. Abra o navegador de sua preferência e digite o seguinte endereço: 

**http://localhost:5000/api/Pessoa**

A princípio, a saída do seu browser deverá ser, apenas a seguinte: **[]**. Isso se deve ao fato de não haver qualquer registro na base de dados. Insira novos registros e teste novamente a aplicação.

Referência do artigo:
http://codefc.com.br/sql-saturday-bh-2018/