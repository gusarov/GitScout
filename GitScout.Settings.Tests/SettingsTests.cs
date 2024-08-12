using GitScout.Settings.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitScout.Settings.Tests;

[TestClass]
public class SettingsTests
{
	ISettings settings = new SettingsImplementation();

	[TestInitialize]
	public void Setup()
	{

	}

	[TestMethod]
	public void Should_save_generic_class_with_settings()
	{
		var demoSettings = settings.Get<DemoSettings>();
		var val1 = $"test_{Guid.NewGuid()}";
		var val2 = Random.Shared.Next(100);
		demoSettings.SomeString1 = val1;
		demoSettings.SomeInt1 = val2;
		settings.Save(demoSettings);

		settings = new SettingsImplementation();
		var demoSettings2 = settings.Get<DemoSettings>();
		Assert.AreNotSame(demoSettings, demoSettings2);
		Assert.AreEqual(val1, demoSettings2.SomeString1);
		Assert.AreEqual(val2, demoSettings2.SomeInt1);
	}

	[TestMethod]
	public void Should_not_allow_Self_constructed_objects()
	{
		Assert.ThrowsException<InvalidOperationException>(() => settings.Save(new DemoSettings()));
	}


	WeakReference GetWeakRefFromSettings()
	{
		var demoSettings = settings.Get<DemoSettings>();
		demoSettings.SomeString1 = "xaxa";
		var wr = new WeakReference(demoSettings);
		demoSettings = null;
		return wr;
	}

	[TestMethod]
	public void Should_not_leak_by_instance()
	{
		var wr = GetWeakRefFromSettings();
		GC.Collect();
		GC.WaitForFullGCApproach();
		Console.WriteLine(((DemoSettings?)wr.Target)?.SomeString1);
		Assert.IsFalse(wr.IsAlive);
	}

	[TestMethod]
	public void Should_get_after_gc()
	{
		var wr = GetWeakRefFromSettings();
		GC.Collect();
		GC.WaitForFullGCApproach();
		Assert.IsFalse(wr.IsAlive);
		wr = GetWeakRefFromSettings();
		Assert.IsTrue(wr.IsAlive);
		// Console.WriteLine(((DemoSettings?)wr.Target)?.SomeString1);
		GC.Collect();
		GC.WaitForFullGCApproach();
		Console.WriteLine(((DemoSettings?)wr.Target)?.SomeString1);
		Assert.IsFalse(wr.IsAlive);
	}
}

public class DemoSettings
{
	public string SomeString1 { get; set; }
	public int SomeInt1 { get; set; }
}