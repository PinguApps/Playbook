// snippet:complete
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddEntityFrameworkDesignTimeServices();
Console.WriteLine("PASS explicit design-time services registered.");
// endsnippet:complete
