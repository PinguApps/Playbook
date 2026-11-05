// snippet:complete
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

var connectionString=Environment.GetEnvironmentVariable("PLAYBOOK_SQL_CONNECTION_STRING") ?? throw new InvalidOperationException("Use isolated SQL Run.");
using var context=new DbContext(new DbContextOptionsBuilder().UseSqlServer(connectionString).Options);
using var connection=new SqlConnection(connectionString);
connection.Open();
using(var create=connection.CreateCommand()) { create.CommandText="CREATE TABLE PatchDocument (Id int PRIMARY KEY, Payload nvarchar(max) NOT NULL); INSERT PatchDocument VALUES (1,N'{}')"; create.ExecuteNonQuery(); }
var generator=new PatchGenerator(context.GetService<UpdateSqlGeneratorDependencies>());
var mapping=context.GetService<IRelationalTypeMappingSource>().FindMapping(typeof(string))!;
var paths=new [] {
#if NET10_0
    "$", "$.station.name", "$.cards[1].name", "$.\"odd.name\""
#else
    StructuredJsonPath.Root,
    new StructuredJsonPath([new("station"),new("name")],[]),
    new StructuredJsonPath([new("cards"),StructuredJsonPathSegment.Array,new("name")],[1]),
    new StructuredJsonPath([new("odd.name")],[])
#endif
};
var values=new [] { "{\"station\":{\"name\":\"old\",\"keep\":17},\"cards\":[{\"name\":\"first\"},{\"name\":\"second\"}],\"odd.name\":\"old\",\"keep\":42}","new station","new card","new odd" };
for(var i=0;i<paths.Length;i++)
{
    var parameters=new ColumnModificationParameters("Payload",values[i],null!,"nvarchar(max)",mapping,paths[i],false,true,false,false,false,true);
    var modification=new ColumnModification(parameters);
    var sql=generator.Render(modification);
    using var command=connection.CreateCommand();
#pragma warning disable CA2100 // Fixed authored identifiers/paths only; values use @value.
    command.CommandText="UPDATE PatchDocument SET Payload="+sql+" WHERE Id=1";
#pragma warning restore CA2100
    command.Parameters.Add(new SqlParameter("@value",System.Data.SqlDbType.NVarChar,-1){Value=values[i]});
    if(command.ExecuteNonQuery()!=1) throw new InvalidOperationException("Update missed row.");
    using var read=connection.CreateCommand(); read.CommandText="SELECT Payload FROM PatchDocument WHERE Id=1";
    using var doc=JsonDocument.Parse((string)read.ExecuteScalar()!);
    var root=doc.RootElement;
    if(root.GetProperty("keep").GetInt32()!=42 || root.GetProperty("station").GetProperty("keep").GetInt32()!=17 || root.GetProperty("cards")[0].GetProperty("name").GetString()!="first") throw new InvalidOperationException("Untouched sibling changed.");
    var actual=i switch {0=>root.GetProperty("odd.name").GetString(),1=>root.GetProperty("station").GetProperty("name").GetString(),2=>root.GetProperty("cards")[1].GetProperty("name").GetString(),_=>root.GetProperty("odd.name").GetString()};
    if(actual!=(i==0?"old":values[i])) throw new InvalidOperationException("Wrong JSON location.");
    Console.WriteLine($"PASS JSON case={i}; path={paths[i]}; intended value and untouched siblings");
}
sealed class PatchGenerator(UpdateSqlGeneratorDependencies dependencies):UpdateSqlGenerator(dependencies)
{
    public string Render(IColumnModification modification)
    {
        var result=new StringBuilder();
        AppendUpdateColumnValue(SqlGenerationHelper,modification,result,"PatchDocument",null);
        return result.ToString();
    }
    protected override void AppendUpdateColumnValue(ISqlGenerationHelper updateSqlGeneratorHelper,IColumnModification columnModification,StringBuilder stringBuilder,string name,string? schema)
    {
        var path=columnModification.JsonPath ?? throw new InvalidOperationException("A JSON path is required.");
#if NET10_0
        var root=path=="$";
        var text=path;
#else
        var root=path.IsRoot;
        var text=path.ToString('"');
#endif
        if(root) { stringBuilder.Append("@value"); return; }
        stringBuilder.Append("JSON_MODIFY(");
        updateSqlGeneratorHelper.DelimitIdentifier(stringBuilder,columnModification.ColumnName);
        stringBuilder.Append(",N'").Append(text.Replace("'","''",StringComparison.Ordinal)).Append("',@value)");
    }
}
// endsnippet:complete

