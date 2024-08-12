namespace GitScout.Utils.Tests;

[TestClass]
public class UtilsTests
{
	[DataTestMethod]
	[DataRow("MethodNotFound", "Method not found")]
	public void Should_convert_to_human_readable(string code, string expected)
	{
		Assert.AreEqual(expected, HumanReadable.Instance.GetHumanReadable(code));
	}

	[TestMethod]
	public void Should_convert_to_human_readable_exception()
	{
		Assert.AreEqual("Missing method", HumanReadable.Instance.GetHumanReadableExceptionType(new MissingMethodException()));
	}
}