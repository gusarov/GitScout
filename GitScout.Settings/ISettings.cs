namespace GitScout.Settings;

public interface ISettings
{
	T Get<T>() where T : new();
	void Save<T>(T settings);
}
