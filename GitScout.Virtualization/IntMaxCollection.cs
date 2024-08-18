using GitScout.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitScout.Virtualization;

/// <summary>
/// Here IList is a key ingridient to enable Data Virtualization
/// </summary>
/// <typeparam name="T"></typeparam>
public class IntMaxCollection<T> : IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection<T>, IList<T>, IList
		where T : class, IVirtualizableModel
{
	private readonly WeakValueDictionary<int, T> _dic = new();
	private readonly int _count;

	public int Count => _count;

	public bool IsReadOnly => true;

	public bool IsFixedSize => true;

	public bool IsSynchronized => false;

	public object SyncRoot => this;

	object? IList.this[int index]
	{
		get => this[index];
		set => this[index] = (T)value;
	}

	public T this[int index]
	{
		get { return _dic.GetOrAdd(index, k => (T)Activator.CreateInstance(typeof(T), [this, k])); }
		set { throw new NotSupportedException(); }
	}

	public IntMaxCollection(int count = int.MaxValue)
	{
		_count = count;
	}

	public class Enumerator : IEnumerator<T>, IEnumerator
	{
		private readonly IntMaxCollection<T> _collection;

		const int NotStarted = -1;
		const int Finished = -2;
		const int Disposed = -3;

		private int _position = NotStarted;


		public Enumerator(IntMaxCollection<T> collection)
		{
			_collection = collection;
		}

		public T Current => _position switch
		{
			NotStarted => throw new InvalidOperationException(),
			Finished => throw new InvalidOperationException(),
			Disposed => throw new ObjectDisposedException(""),
			_ => _collection[_position],
		};

		object IEnumerator.Current => Current;

		public void Dispose()
		{
			_position = Disposed;
		}

		public bool MoveNext()
		{
			switch (_position)
			{
				case Finished:
					return false;
				case Disposed:
					throw new ObjectDisposedException("");
				case NotStarted:
				default:
					if (_position >= 16000)
					{
						throw new Exception("You went too far! Iterator is not for this!");
					}
					unchecked
					{
						_position++;
					}
					if (_position < 0) // overflow
					{
						_position = Finished;
						return false;
					}
					if (_position >= _collection._count)
					{
						return false;
					}
					return true;
			}
		}

		public void Reset()
		{
			_position = NotStarted;
		}
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	public void Add(T item)
	{
		throw new NotSupportedException();
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public bool Contains(T item)
	{
		throw new NotImplementedException();
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	public bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	public int IndexOf(T item)
	{
		throw new NotImplementedException();
	}

	public void Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	public int Add(object? value)
	{
		throw new NotSupportedException();
	}

	public bool Contains(object? value)
	{
		throw new NotImplementedException();
	}

	public int IndexOf(object? value)
	{
		if (value is T model)
		{
			if (model.Owner != this)
			{
				return -1;
			}
			return model.Index;
		}
		return -1;
	}

	public void Insert(int index, object? value)
	{
		throw new NotSupportedException();
	}

	public void Remove(object? value)
	{
		throw new NotSupportedException();
	}

	public void CopyTo(Array array, int index)
	{
		throw new NotImplementedException();
	}
}
