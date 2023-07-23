using System;
using UnityEngine;

namespace Common.Core
{
	public struct CounterLock : IDisposable
	{
		private int _counter;

		public bool IsLocked => _counter > 0;

		public IDisposable Lock()
		{
			++_counter;
			return this;
		}

		public void Release()
		{
			--_counter;
			Debug.Assert(_counter >= 0);
		}

		public void Clear()
		{
			_counter = 0;
		}

        void IDisposable.Dispose()
		{
			Release();
		}
	}
}