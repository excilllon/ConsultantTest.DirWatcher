namespace ConsultantTest.DirWatcher.Abstractions;

/// <summary>
/// Подсчет букв в файлах
/// </summary>
public interface IFileLettersCounter
{
	/// <summary>
	/// Полный путь к файлу
	/// </summary>
	void Enqueue(string fullFileName);
}