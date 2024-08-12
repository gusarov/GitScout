using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitScout.Settings;

internal class ReposList
{
	public ObservableCollection<RepoInfo> Repos { get; set; } = new ObservableCollection<RepoInfo>
	{
		// new RepoInfo { Path = "asd1/asd1/asd1", },
		// new RepoInfo { Path = "asd2/asd2/asd2", },
	};
}

internal class RepoInfo
{
	public string Path { get; set; }

	public string Short => System.IO.Path.GetFileName(Path);
}
