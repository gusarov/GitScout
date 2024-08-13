namespace GitScout.Utils.Tests;

[TestClass]
public class HumanReadableUtilsTests
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

[TestClass]
public class WeakCacheTests
{
	[TestMethod]
	public void Should_keep_by_key()
	{
		var dic = new WeakKeyDictionary<DemoKey, string>();

		var key1 = new DemoKey();
		var key2 = new DemoKey();

		dic[key1] = "val1";
		dic[key2] = "val2";

		Assert.AreEqual("val1", dic[key1]);
		Assert.AreEqual("val2", dic[key2]);
	}

	WeakReference RegisterKey(WeakKeyDictionary<DemoKey, string> sut)
	{
		var key = new DemoKey();
		var wr = new WeakReference(key);
		sut.Add(key, "to be deleted");
		return wr;
	}

	[TestMethod]
	public void Should_allow_key_to_be_collected()
	{
		var dic = new WeakKeyDictionary<DemoKey, string>();

		var wr = RegisterKey(dic);
		Assert.IsTrue(wr.IsAlive);
		GC.Collect();
		GC.WaitForFullGCApproach();
		Assert.IsFalse(wr.IsAlive);

		Assert.AreEqual(1, dic.Count);
		Thread.Sleep(TimeSpan.FromSeconds(3));
		Assert.AreEqual(0, dic.Count);
	}

	class DemoKey
	{

	}
}
