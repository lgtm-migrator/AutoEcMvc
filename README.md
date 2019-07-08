# AutoEcMvc

This is a Code Generation project which uses T4 to automatically generate the Models, Controllers and Views for an ASP.NET Core web app using Entity Framework Core. The original project is based off [Microsoft tutorials](https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/intro?view=aspnetcore-2.2) and the source code for the tutorial can be found in the [ASP.NET documentation repo](https://github.com/aspnet/AspNetCore.Docs/tree/master/aspnetcore/data/ef-mvc/intro/samples/cu-final). I have started blogging about this project on [Medium](https://medium.com/@christopher.r.froehlich/code-generation-connecting-t4-to-entity-framework-core-654e2a0933e8), and you can view the current deployed version of the app on [Azure](https://autoecmvc.luddites.me).

## Building

* `git clone git@github.com:crfroehlich/AutoEcMvc.git`
* `cd AutoEcMvc`
* `msbuild CodeGeneration\CodeGeneration.csproj /t:TransformAll`
* `msbuild`

After the solution compiles, adjust the connection string in `appsettings.json` if needed. By default, the project uses LocalDb, which is a lightweight database stored in your user profile; it should not require any additional permissions to run. You can switch this to target any EF Core supported database (might require the installation of additional NuGet packages to support the target database).

You can then launch the app. 
