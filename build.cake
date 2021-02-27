var target = Argument("target", "Install");
var configuration = Argument("configuration", "Release");

var installFolder = Directory((EnvironmentVariable("LOCALAPPDATA")) + "/Colossal Order/Cities_Skylines/Addons/Mods/PrecisionEngineering");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    //.WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($"./src/PrecisionEngineering/bin/{configuration}");
    CleanDirectory($"./src/PrecisionEngineering/obj/{configuration}");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    MSBuild("./src/PrecisionEngineering.sln", configurator => configurator.SetConfiguration(configuration));
});

Task("Install")
    .IsDependentOn("Build")
    .Does(() =>
{
    CopyFileToDirectory($"./src/PrecisionEngineering/bin/{configuration}/PrecisionEngineering.dll", installFolder);
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);