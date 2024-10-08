﻿using System.Windows.Controls;

namespace GitScout.Virtualization;

public class MyTextBox : TextBox
{
	public MyTextBox()
	{
		ObjectCountTracker.Instance.RegisterConstruction(this);
	}

#if DEBUG
	~MyTextBox()
	{
		ObjectCountTracker.Instance.RegisterDestruction(this);
	}
#endif
}
