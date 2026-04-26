using System.Diagnostics;
using Mino.Utility.Logging;

namespace Mino.RML;

public class RMLSourceMod {
	internal static readonly List<RMLSourceMod> _smRegistry = new List<RMLSourceMod>();

	public static void Register(RMLSourceMod sm) {
		_smRegistry.Add(sm);
	}
	
	public readonly string ProjectPath;
	public readonly string ProjectName;
	public readonly string ModId;
	
	public RMLSourceMod(string projectPath, string projectName, string modId) {
		ProjectPath = projectPath;
		ProjectName = projectName;
		ModId = modId;
	}

	public void Build() {
		string cmd = $"publish \"{ProjectPath}/{ProjectName}.csproj\" -c Debug -o {ProjectPath}/mod/build";
		
		Process process = new Process {
			StartInfo = new ProcessStartInfo {
				FileName = "dotnet",
				Arguments = cmd,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};
		
		Log.Info($"Building: dotnet {cmd}");
        
		process.Start();
		string output = process.StandardOutput.ReadToEnd();
		string error = process.StandardError.ReadToEnd();
		process.WaitForExit();
		
		if (process.ExitCode != 0) {
			Log.Fatal($"Build error: {error}");
		}
        
		Log.Debug("Dynamic building succeeds");
		Log.Debug(output);
	}
}
