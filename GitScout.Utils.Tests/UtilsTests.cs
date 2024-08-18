using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitScout.Utils.Tests;

[TestClass]
public class ObjectExtensionsTests
{
	public class MyModel
	{

	}

	public class MyModelExtensionFactorable
	{
		public MyModel Model { get; }

		public MyModelExtensionFactorable(MyModel model)
		{
			Model = model;
		}
	}
	public class MyModelExtensionFactorableNewable
	{
		public MyModel Model { get; }

		public MyModelExtensionFactorableNewable()
		{

		}

		public MyModelExtensionFactorableNewable(MyModel model)
		{
			Model = model;
		}
	}

	public class MyModelExtensionNewable
	{

	}

	[TestMethod]
	public void Should_extend_object_with_newable()
	{
		var commit1 = new MyModel();
		var commit2 = new MyModel();

		var adaptedCommit1 = ObjectExtensions<MyModelExtensionNewable>.Get(commit1);
		var adaptedCommit2 = ObjectExtensions<MyModelExtensionNewable>.Get(commit2);

		Assert.AreSame(adaptedCommit1, ObjectExtensions<MyModelExtensionNewable>.Get(commit1));
		Assert.AreSame(adaptedCommit2, ObjectExtensions<MyModelExtensionNewable>.Get(commit2));
	}

	[TestMethod]
	public void Should_extend_object_with_factorable()
	{
		var commit1 = new MyModel();
		var commit2 = new MyModel();

		var adaptedCommit1 = ObjectExtensions.Get(commit1, x => new MyModelExtensionFactorable(x));
		var adaptedCommit2 = ObjectExtensions.Get(commit2, x => new MyModelExtensionFactorable(x));

		Assert.AreSame(adaptedCommit1, ObjectExtensions.Get(commit1, x => new MyModelExtensionFactorable(x)));
		Assert.AreSame(adaptedCommit2, ObjectExtensions.Get(commit2, x => new MyModelExtensionFactorable(x)));
	}

	[TestMethod]
	public void Should_extend_object_with_default_factory()
	{
		var commit1 = new MyModel();
		var commit2 = new MyModel();

		var adaptedCommit1 = ObjectExtensions<MyModel, MyModelExtensionFactorable>.Instance.Get(commit1, x => new MyModelExtensionFactorable(x));
		var adaptedCommit2 = ObjectExtensions<MyModel, MyModelExtensionFactorable>.Instance.Get(commit2);

		Assert.AreSame(adaptedCommit1, ObjectExtensions<MyModel, MyModelExtensionFactorable>.Instance.Get(commit1, x => new MyModelExtensionFactorable(x)));
		Assert.AreSame(adaptedCommit2, ObjectExtensions<MyModel, MyModelExtensionFactorable>.Instance.Get(commit2));
		Assert.AreSame(commit2, adaptedCommit2.Model);
		Assert.AreSame(commit2, ObjectExtensions<MyModel, MyModelExtensionFactorable>.Instance.Get(commit2).Model);
	}

	[TestMethod]
	public void Should_extend_object_with_same_newable_as_factorable()
	{
		var commit1 = new MyModel();
		var adaptedCommit1 = ObjectExtensions.Get(commit1, x => new MyModelExtensionFactorableNewable(x)); // factorable api
		Assert.AreSame(adaptedCommit1, ObjectExtensions<MyModelExtensionFactorableNewable>.Get(commit1)); // newable api
	}

	(WeakReference, WeakReference) RegisterExtension()
	{
		var item = new MyModel();
		var ext = ObjectExtensions<MyModelExtensionNewable>.Get(item);
		return (new WeakReference(item), new WeakReference(ext));
	}

	[TestMethod]
	[Timeout(3000)]
	public void Should_not_leak()
	{
		var (model, extension) = RegisterExtension();
		Assert.IsTrue(model.IsAlive);
		Assert.IsTrue(extension.IsAlive);
		GC.Collect();
		Assert.IsFalse(model.IsAlive);

		// Usually extension is still alive till the cache maintenance
		while (extension.IsAlive)
		{
			Thread.Sleep(100);
			GC.Collect();
		}
		Assert.IsFalse(extension.IsAlive);
	}
}

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
public class WeakKeyTests
{
	[TestMethod]
	public void Should_keep_by_key()
	{
		var dic = new WeakKeyDictionary2<DemoKey, string>();

		var key1 = new DemoKey();
		var key2 = new DemoKey();

		dic[key1] = "val1";
		dic[key2] = "val2";

		Assert.AreEqual("val1", dic[key1]);
		Assert.AreEqual("val2", dic[key2]);
	}

	WeakReference RegisterKey(WeakKeyDictionary2<DemoKey, string> sut)
	{
		var key = new DemoKey();
		var wr = new WeakReference(key);
		sut[key] = "to be deleted";
		return wr;
	}

	[TestMethod]
	public void Should_allow_key_to_be_collected()
	{
		var dic = new WeakKeyDictionary2<DemoKey, string>();

		var wr = RegisterKey(dic);
		Assert.IsTrue(wr.IsAlive);
		GC.Collect();
		GC.WaitForFullGCApproach();
		Assert.IsFalse(wr.IsAlive);

		while (dic.Count != 0)
		{
			Thread.Sleep(100);
		}
		// Assert.AreEqual(1, dic.Count);
		// Thread.Sleep(TimeSpan.FromSeconds(3));
		Assert.AreEqual(0, dic.Count);
	}

	class DemoKey
	{

	}
}

[TestClass]
public class WeakValueTests
{
	[TestMethod]
	public void Should_keep_by_key()
	{
		var dic = new WeakValueDictionary<string, DemoValue>();

		var value1 = new DemoValue();
		var value2 = new DemoValue();

		dic["a"] = value1;
		dic["b"] = value2;

		Assert.AreEqual(value1, dic["a"]);
		Assert.AreEqual(value2, dic["b"]);
	}

	WeakReference Register(string key, WeakValueDictionary<string, DemoValue> sut)
	{
		var value = new DemoValue();
		var wr = new WeakReference(value);
		sut[key] = value;
		return wr;
	}

	[TestMethod]
	public void Should_allow_values_to_be_collected()
	{
		var dic = new WeakValueDictionary<string, DemoValue>();

		var wr = Register("a", dic);
		Assert.IsTrue(wr.IsAlive);
		Assert.AreEqual(1, dic.Count);

		GC.Collect();
		GC.WaitForFullGCApproach();
		Assert.IsFalse(wr.IsAlive);

		while (dic.Count != 0)
		{
			Thread.Sleep(100);
		}
		// Assert.AreEqual(1, dic.Count);
		// Thread.Sleep(TimeSpan.FromSeconds(3));
		Assert.AreEqual(0, dic.Count);
	}

	class DemoValue
	{

	}
}
