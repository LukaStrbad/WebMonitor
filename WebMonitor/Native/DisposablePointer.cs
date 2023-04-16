using System.Collections;
using System.Runtime.InteropServices;

namespace WebMonitor.Native;

/// <summary>
/// A smart pointer that ensures the memory is freed in "using statements"
/// </summary>
/// <typeparam name="T">Type of the struct</typeparam>
internal class DisposablePointer<T> : IDisposable, IEnumerable<T>
where T : struct
{
	/// <summary>
	/// Number of allocated items
	/// </summary>
	public int NumElements { get; }

	/// <summary>
	/// Allocated pointer
	/// </summary>
	/// <value>Memory address</value>
	public nint Pointer { get; }

	/// <summary>
	/// Total amount of allocated memory
	/// </summary>
	public int Size => NumElements * Marshal.SizeOf<T>();

	/// <summary>
	/// Allocates memory for the specified number of elements
	/// </summary>
	/// <param name="numElements">Number of T elements to allocate</param>
	public DisposablePointer(int numElements = 1)
	{
		NumElements = numElements;
		Pointer = Marshal.AllocHGlobal(Size);
	}
	
	/// <summary>
	/// Allocates memory and copies the element to the allocated pointer
	/// </summary>
	/// <param name="element">Existing struct value</param>
	/// <remarks>This is useful for calling APIs that require the struct size to be set explicitly</remarks>
	public DisposablePointer(T element)
	{
		NumElements = 1;
		Pointer = Marshal.AllocHGlobal(Size);
		// Copies the element to the allocated pointer
		Marshal.StructureToPtr(element, Pointer, false);
	}

	public void Dispose()
	{
		Marshal.FreeHGlobal(Pointer);
	}

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= NumElements)
				throw new IndexOutOfRangeException("Index must be greater than zero and less than the number of elements");

			return Marshal.PtrToStructure<T>(Pointer + index * Marshal.SizeOf<T>());
		}
	}

	/// <summary>
	/// Provides an implicit conversion to nint
	/// </summary>
	public static implicit operator nint(DisposablePointer<T> dPtr) => dPtr.Pointer;

	/// <summary>
	/// Provides an explicit cast to void*
	/// </summary>
	/// <param name="dPtr"></param>
	public static unsafe explicit operator void*(DisposablePointer<T> dPtr) => (void*)dPtr.Pointer;

	public IEnumerator<T> GetEnumerator() => new DisposablePointerEnumerator(this);

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private class DisposablePointerEnumerator : IEnumerator<T>
	{
		private readonly DisposablePointer<T> _dPtr;
		private int _curIndex = -1;

		public T Current => _dPtr[_curIndex];

		object IEnumerator.Current => Current;

		public DisposablePointerEnumerator(DisposablePointer<T> dPtr)
		{
			_dPtr = dPtr;
		}

		public void Dispose()
		{ }

		public bool MoveNext()
		{
			_curIndex++;
			// If index is out of range return false
			return _curIndex < _dPtr.NumElements;
		}

		public void Reset() => _curIndex = -1;
	}
}
