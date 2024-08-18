using System.Globalization;
using System.Text;

namespace GitScout;

public class HumanReadable
{
	public static HumanReadable Instance = new HumanReadable();

	private HumanReadable()
	{
	}

	public string GetHumanReadable(string code, CultureInfo? culture = null)
	{
		culture ??= CultureInfo.CurrentCulture;
		var sb = new StringBuilder();
		for (int i = 0; i < code.Length; i++)
		{
			if (char.IsUpper(code[i]) && sb.Length > 0)
			{
				sb.Append(' ');
				sb.Append(char.ToLower(code[i], culture));
			}
			else
			{
				sb.Append(code[i]);
			}
		}
		return sb.ToString();
	}
	public string GetHumanReadableExceptionType(Exception exception)
	{
		var code = exception.GetType().Name;
		var exc = "Exception";
		if (code.EndsWith(exc))
		{
			code = code.Substring(0, code.Length - exc.Length);
		}
		return GetHumanReadable(code);
	}
}

