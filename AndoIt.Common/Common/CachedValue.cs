using System;

namespace AndoIt.Common.Common
{
	/// <summary>
	/// Obtiene un valor la primera vez y no vuelve a solicitarlo hasta que no pase el timestamp definido
	/// </summary>
	public class CachedValue<T>
	{
		private readonly Func<T> func;
		private readonly TimeSpan expiresIn;
		private readonly Interface.ILog log;
		private DateTime nextTimeToRead;
		private T value;

		public CachedValue(Func<T> func, TimeSpan expiresIn, Interface.ILog log = null)
		{
			this.func = func ?? throw new ArgumentNullException("func");
			this.expiresIn = expiresIn;
			this.log = log;
			this.nextTimeToRead = DateTime.MinValue;
		}

		public bool ValueRefreshedInLastCall { get; private set; } = false;

		public T Value
		{
			get {
				if (DateTime.Now > this.nextTimeToRead)
				{
					this.value = func.Invoke();
					this.nextTimeToRead = DateTime.Now.Add(this.expiresIn);
					this.log?.Info($"Value was read. Expires in {this.nextTimeToRead}");
					this.ValueRefreshedInLastCall = true;
				} else {
					this.log?.Debug($"Value was NOT read yet");
					this.ValueRefreshedInLastCall = false;
				}
				return this.value;
			}
		}
	}
}
