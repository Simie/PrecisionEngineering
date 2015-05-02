// include Fake lib
#r @"FAKE/FakeLib.dll"
open Fake

open Fake.FileSystem;
open Fake.FileSystemHelper;
open Fake.FileUtils;
open Fake.EnvironmentHelper;

let buildDir = "./Build/"

let installFolder = (environVar "LOCALAPPDATA") + "/Colossal Order/Cities_Skylines/Addons/Mods/PrecisionEngineering"

Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "Build" (fun _ -> 
    !! "./Src/**/*.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "AppBuild-Output: "
)

Target "Install" (fun _ -> 
    
    mkdir (installFolder)
    CopyFile installFolder (buildDir + "PrecisionEngineering.dll")
            
)

// Dependencies
"Clean"
  ==> "Build"

"Build"
  ==> "Install"

// start build
RunTargetOrDefault "Install"