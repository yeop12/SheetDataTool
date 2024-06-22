namespace Sheet
{
	public interface IDesignSheetData<out T>
	{
		public T Key { get; }
	}
}
