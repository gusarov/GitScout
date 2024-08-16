using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitScout.ViewModels
{
	public class CommitNode // to reduce memory, it is not notifieable view model, just a contract
	{
		public string Message { get; set; }
		public string Author { get; set; }
		public DateTime Date { get; set; }
		public List<string> Branches { get; set; }
		public string CommitId { get; set; }
	}

}
