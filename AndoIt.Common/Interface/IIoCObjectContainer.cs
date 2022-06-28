namespace AndoIt.Common.Interface
{
	public interface IIoCObjectContainer
	{
		T Get<T>(string id = null);
	}
}
