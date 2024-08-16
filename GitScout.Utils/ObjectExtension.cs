using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitScout.Utils;

public static class ObjectExtensions
{
	public static TExtensions Get<TModel, TExtensions>(TModel obj, Func<TModel, TExtensions> factory)
		where TModel : class
		where TExtensions : class
	{
		return ObjectExtensions<TModel, TExtensions>.Instance.Get(obj, factory);
	}
}

public static class ObjectExtensions<TExtensions>
		where TExtensions : class, new()
{
	/*
	public static TViewModel OpenVm<TModel, TViewModel>(TModel model)
		where TModel : class
		where TViewModel : class, new()
	{
		return ObjectExtensions<TModel, TViewModel>.Instance.GetExtensions(model, model => new TViewModel());
	}

	public static TViewModel OpenVm<TModel, TViewModel>(TModel model, Func<TModel, TViewModel> factory)
		where TModel : class
		where TViewModel : class
	{
		return ObjectExtensions<TModel, TViewModel>.Instance.GetExtensions(model, factory);
	}
	*/

	public static TExtensions Get<TModel>(TModel obj)
		where TModel : class
	{
		return ObjectExtensions<TModel, TExtensions>.Instance.Get(obj, model => new TExtensions());
	}


}

public class ObjectExtensions<TModel, TExtensions>
	where TModel : class
	where TExtensions : class
{
	public static ObjectExtensions<TModel, TExtensions> Instance = new ObjectExtensions<TModel, TExtensions>();

	private ObjectExtensions()
	{
	}

	WeakKeyDictionary2<TModel, TExtensions> _extensions = new WeakKeyDictionary2<TModel, TExtensions>();

	public TExtensions Get(TModel model, Func<TModel, TExtensions> factory)
	{
		return _extensions.GetOrAdd(model, factory)!;
	}
}

