using GitScout.Utils;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GitScout.Settings.Implementation;

public class SettingsImplementation : ISettings
{
	string _path;
	// WeakReferenceKeyDictionary<object, object?> _dictionary = new WeakReferenceKeyDictionary<object, object?>();
	Dictionary<Type, WeakReference> _objects = new Dictionary<Type, WeakReference>();

	public SettingsImplementation()
	{
		_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GitScout", "Settings");
		Directory.CreateDirectory(_path);
	}

	public T Get<T>() where T : new()
	{
		_objects.TryGetValue(typeof(T), out var instanceWeak);
		T? instance = (T?)instanceWeak?.Target;
		if (instance != null)
		{
			return instance;
		}

		var fileName = Path.Combine(_path, typeof(T).Name);
		T? result = default;
		if (File.Exists(fileName))
		{
			var data = File.ReadAllText(fileName);
			result = JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions
			{
				AllowTrailingCommas = true,
			});
		}
		result ??= new T();
		// _dictionary.Add(result, null);
		_objects.Add(typeof(T), new WeakReference(result));
		return result;
	}

	public void Save<T>(T settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException(nameof(settings));
		}
		/*
		if (!_dictionary.TryGetValue(settings, out var result))
		{
			throw new InvalidOperationException("This setting object was never created by Get operation");
		}
		*/
		if (_objects.TryGetValue(typeof(T), out var instanceWeak))
		{
			var instance = (T?)instanceWeak.Target;
			if (ReferenceEquals(instance, settings))
			{
				var fileName = Path.Combine(_path, typeof(T).Name);
				File.WriteAllText(fileName, JsonSerializer.Serialize(settings, new JsonSerializerOptions
				{
					WriteIndented = true,
					IgnoreReadOnlyFields = true,
					IgnoreReadOnlyProperties = true,
				}));
				return;
			}
		}
		throw new InvalidOperationException("This setting object was never created by Get operation");
	}
}
