switch (args?[0])
{
	case "test1":
	{
		Console.WriteLine("test1");
		break;
	}
	case "test2":
	{
		Console.WriteLine("test1");
		Console.WriteLine("test2");
		break;
	}
	case "test3":
	{
		Console.WriteLine("test1");
		Thread.Sleep(100);
		Console.Error.WriteLine("test2");
		Thread.Sleep(100);
		Console.WriteLine("test3");
		Thread.Sleep(100);
		Console.Error.WriteLine("test4");
		break;
	}
	case "test4":
	{
		for (int i = 0; i < 10240; i++)
		{
			Console.WriteLine("test" + i);
		}
		break;
	}
	default:
		Console.WriteLine("Unknown arguments");
		break;
}