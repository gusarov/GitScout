using GitScout.Settings;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace GitScout.DataContext;

static class ViewModelsExtensions
{
	public static async Task Sync<T>(this ObservableCollection<T> actual, IEnumerable<T> expected)
	{
		await Sync(actual, new HashSet<T>(expected));
	}

	public static async Task Sync<T>(this ObservableCollection<T> actual, IImmutableSet<T> expectedReusable)
	{
		// to remove roots
		List<T> toDelete = new List<T>();
		foreach (var existingBranch in actual)
		{
			if (!expectedReusable.Contains(existingBranch))
			{
				toDelete.Add(existingBranch);
			}
			else
			{
				expectedReusable = expectedReusable.Remove(existingBranch); // remove from pending addition, if it is alread there
			}
		}
		await UiServiceLocator.Instance.Dispatcher.BeginInvoke(() =>
		{
			foreach (var item in expectedReusable)
			{
				actual.Add(item);
			}
			foreach (var item in toDelete)
			{
				actual.Remove(item);
			}
		});
	}

	// [Obsolete("This method will corrupt original set, make sure it is no longer needed")]
	/// <summary>
	/// This method will corrupt original set, make sure it is no longer needed
	/// </summary>
	public static async Task SyncConsumeSet<T>(this ObservableCollection<T> actual, ISet<T> expectedReusable)
	{
		// to remove roots
		List<T> toDelete = new List<T>();
		foreach (var existingBranch in actual)
		{
			if (!expectedReusable.Contains(existingBranch))
			{
				toDelete.Add(existingBranch);
			}
			else
			{
				expectedReusable.Remove(existingBranch); // remove from pending addition, if it is alread there
			}
		}
		await UiServiceLocator.Instance.Dispatcher.BeginInvoke(() =>
		{
			foreach (var item in expectedReusable)
			{
				actual.Add(item);
			}
			foreach (var item in toDelete)
			{
				actual.Remove(item);
			}
			expectedReusable.Clear(); // to destroy original set completely and avoid unexpected behavior if consumer forgot about it
		});
	}
}
