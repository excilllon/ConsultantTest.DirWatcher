namespace ConsultantTest.DirWatcher.Abstractions;

public interface IFileLettersCounter
{
	void Enqueue(string fullFileName);
}