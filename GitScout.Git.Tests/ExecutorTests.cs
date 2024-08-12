#define EXEC_SYNC

using GitScout.Git.External;

namespace GitScout.Git.Tests;

[TestClass]
public class ExecutorTests
{

	[TestMethod]
	public void Should_01_handle_shell_execute()
	{
		var result = Executor.Instance.Execute(null, AsmInit.ConsoleWriterPath, "test1");
		Assert.AreEqual(1, result.Count, string.Join(Environment.NewLine, result));
		Assert.IsTrue(result[0].TrimEnd() == "test1");
	}

	[TestMethod]
	public void Should_02_handle_shell_execute()
	{
		var result = Executor.Instance.Execute(null, AsmInit.ConsoleWriterPath, "test2");
		Assert.AreEqual(2, result.Count, string.Join(Environment.NewLine, result));
		Assert.IsTrue(result[0].TrimEnd() == "test1");
		Assert.IsTrue(result[1].TrimEnd() == "test2");
	}

	[TestMethod]
	public void Should_03_handle_shell_execute()
	{
		var result = Executor.Instance.Execute(null, AsmInit.ConsoleWriterPath, "test3", throwOnErrorStream: false);
		foreach (var item in result)
		{
			Console.WriteLine(item);
		}
		Assert.AreEqual(4, result.Count, string.Join(Environment.NewLine, result));
		Assert.AreEqual("test1", result[0].TrimEnd());
		Assert.AreEqual("test2", result[1].TrimEnd());
		Assert.AreEqual("test3", result[2].TrimEnd());
		Assert.AreEqual("test4", result[3].TrimEnd());
	}

	[TestMethod]
	public void Should_03b_handle_shell_execute()
	{
		Assert.ThrowsException<CommandExecutionException>(() =>
		{
			var result = Executor.Instance.Execute(null, AsmInit.ConsoleWriterPath, "test3");
		});
	}

	[TestMethod]
	public void Should_04_handle_shell_execute_bulk_response()
	{
		var result = Executor.Instance.Execute(null, AsmInit.ConsoleWriterPath, "test4");
		Assert.AreEqual(10240, result.Count, string.Join(Environment.NewLine, result));
		for (int i = 0; i < 10240; i++)
		{
			Assert.AreEqual("test" + i, result[i]);
		}
	}
}
