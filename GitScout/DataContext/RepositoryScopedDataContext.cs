using GitScout.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitScout.DataContext
{
	internal class RepositoryScopedDataContext : ViewModel
	{
		private readonly RepoInfo _repoInfo;

		public RepositoryScopedDataContext(RepoInfo repoInfo)
		{
			_repoInfo = repoInfo;
		}
	}
}
