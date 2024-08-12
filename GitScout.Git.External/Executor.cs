#define EXEC_STREAM
using System.Diagnostics;

namespace GitScout.Git.External;

public class Executor
{
	readonly TimeSpan _maxExecution = TimeSpan.FromSeconds(5);

	public static Executor Instance = new Executor();

	public IReadOnlyList<string> Execute(string dir, string command, string arguments, bool throwOnErrorStream = true)
	{
		string logCommand = $"{command} {arguments}{Environment.NewLine}";

		var psi = new ProcessStartInfo(command, arguments)
		{
			WorkingDirectory = dir,
			RedirectStandardError = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			UseShellExecute = false,
			CreateNoWindow = true,
		};

		var process = Process.Start(psi);
		List<string> outputData = new List<string>();
		List<string> errorData = new List<string>();
		List<string> totalData = new List<string>();
#if EXEC_ASYNC
		process.OutputDataReceived += (sender, e) => { if (e.Data is not null) { lock (totalData) { outputData.Add(e.Data); totalData.Add(e.Data); } } };
		process.ErrorDataReceived += (sender, e) => { if (e.Data is not null) { lock (totalData) { errorData.Add(e.Data); totalData.Add(e.Data); } } };
		process.EnableRaisingEvents = true;
		try
		{
			process.BeginOutputReadLine();
		}
		catch (Exception ex)
		{
			// _logger.LogError(ex, "Exception while BeginOutputReadLine");
			throw;
		}
		try
		{
			process.BeginErrorReadLine();
		}
		catch (Exception ex)
		{
			// _logger.LogError(ex, "Exception while BeginOutputReadLine");
			throw;
		}
#endif
#if EXEC_STREAM
		void WatchForStream(StreamReader streamReader, List<string> data, AutoResetEvent syncEvent)
		{
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					while (!streamReader.EndOfStream)
					{
						var line = streamReader.ReadLine();
						if (line != null)
						{
							data.Add(line);
							lock (totalData)
							{
								totalData.Add(line);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("ThreadPool Exception: " + ex);
				}
				syncEvent.Set();
			});
		}
		var outputSyncDone = new AutoResetEvent(false);
		var errorSyncDone = new AutoResetEvent(false);
		WatchForStream(process.StandardOutput, outputData, outputSyncDone);
		WatchForStream(process.StandardError, errorData, errorSyncDone);
#endif
		if (!process.WaitForExit(_maxExecution))
		{
			throw new Exception($"{logCommand}Timed out");
		}

#if EXEC_STREAM
		outputSyncDone.WaitOne();
		errorSyncDone.WaitOne();
#endif

		if (throwOnErrorStream && errorData.Count > 0)
		{
			throw new Exception($"{logCommand}{string.Join(Environment.NewLine, errorData)}{Environment.NewLine}Exit Code: {process.ExitCode}");
		}
		if (process.ExitCode != 0)
		{
			throw new Exception($"{logCommand}{string.Join(Environment.NewLine, totalData)}{Environment.NewLine}Exit Code: {process.ExitCode}");
		}
		return totalData;
	}
}
