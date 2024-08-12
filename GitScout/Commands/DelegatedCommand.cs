using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GitScout.Commands;
internal class DelegatedCommand : ICommand
{
	private readonly Action<object?> _action;

	public DelegatedCommand(Action<object?> action)
	{
		_action = action ?? throw new ArgumentNullException(nameof(action));
	}

    public event EventHandler? CanExecuteChanged
	{
		add { }
		remove { }
	}

	public bool CanExecute(object? parameter)
	{
		return true;
	}

	public void Execute(object? parameter)
	{
		_action(parameter);
	}
}
