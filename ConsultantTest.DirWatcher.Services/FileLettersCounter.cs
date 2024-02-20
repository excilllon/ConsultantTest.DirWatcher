using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using System.Threading.Channels;
using ConsultantTest.DirWatcher.Abstractions;
using Microsoft.Extensions.Logging;

namespace ConsultantTest.DirWatcher.Services
{
	/// <summary>
	/// 
	/// </summary>
	public class FileLettersCounter : IDisposable, IFileLettersCounter
	{
		private readonly ILogger<FileLettersCounter> _logger;
		Channel<string> channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions(){SingleWriter = true});
		private int _bufferLength = 1024 * 1024;
		private readonly string _resPath;
		private bool _isInitialized;
		private const int _taksCount = 4;
		private readonly CancellationTokenSource _cancelTokenSource = new();

		public FileLettersCounter(IConfiguration config, ILogger<FileLettersCounter> logger)
		{
			_logger = logger;
			_resPath = config["res"];
		}

		public void Enqueue(string fullFileName)
		{
			if (!_isInitialized)
			{
				StartProcessing();
			}
			channel.Writer.WriteAsync(fullFileName);
		}

		/// <summary>
		/// запуск потоков обработки
		/// </summary>
		private void StartProcessing()
		{
			if (!Directory.Exists(_resPath))
			{
				_logger.Log(LogLevel.Information, $"Папка {_resPath} не найдена");
			}
			var token = _cancelTokenSource.Token;
			for (var i = 0; i < _taksCount; i++)
			{
				Task.Run(async () =>
				{
					while (!_cancelTokenSource.IsCancellationRequested)
					{
						string fileName = null;
						try
						{
							fileName = await channel.Reader.ReadAsync(token);
							var res = await CountLetters(fileName);
							await WriteResult(fileName, res);
							_logger.Log(LogLevel.Information, $"Файл {fileName} обработан");
						}
						catch (Exception e)
						{
							_logger.Log(LogLevel.Error, e, $"Ошибка при обработке файла {fileName}");
						}
					}
				});
			}
			_isInitialized = true;
		}

		/// <summary>
		/// Подсчет букв в файле 
		/// </summary>
		/// <param name="fullFileName"></param>
		/// <returns></returns>
		private async Task<long> CountLetters(string fullFileName)
		{
			if (!File.Exists(fullFileName)) return -1;
			long result = 0;
			// Только файлы в UTF-8
			using var reader = File.OpenText(fullFileName);
			var buffer = new char[_bufferLength];
			var read = await reader.ReadBlockAsync(buffer, 0, _bufferLength);
			while (read > 0)
			{
				result += buffer.Take(read).Count(c => char.IsLetter(c));
				read = await reader.ReadBlockAsync(buffer, 0, _bufferLength);
			}
			return result;
		}

		/// <summary>
		/// Записать результат в файл с таким же именем, но в папку результатов
		/// </summary>
		/// <param name="fullFileName">Исходный файл</param>
		/// <param name="res">Количество букв для записи</param>
		/// <returns></returns>
		private async Task WriteResult(string fullFileName, long res)
		{
			var fileName = Path.GetFileName(fullFileName);
			await using var reader = File.CreateText(Path.Combine(_resPath, fileName));
			await reader.WriteAsync(res.ToString());
		}

		public void Dispose()
		{
			_cancelTokenSource.Cancel();
		}
	}
}
