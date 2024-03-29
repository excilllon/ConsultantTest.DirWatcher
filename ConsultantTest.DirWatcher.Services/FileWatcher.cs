﻿using ConsultantTest.DirWatcher.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsultantTest.DirWatcher.Services
{
	/// <summary>
	/// Сервис для мониторинга появления файлов в папке
	/// </summary>
	public class FileWatcher: BackgroundService
	{
		private readonly IFileLettersCounter _lettersCounter;
		private readonly ILogger<FileWatcher> _logger;
		// возможно FileSystemWatcher не будет работать в Linux, тогда можно перейти на PhysicalFilesWatcher
		private FileSystemWatcher _watcher;
		private readonly string _srcPath;

		public FileWatcher(IConfiguration config, IFileLettersCounter lettersCounter, ILogger<FileWatcher> logger)
		{
			_lettersCounter = lettersCounter;
			_logger = logger;
			_srcPath = config["src"];
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			if (!Directory.Exists(_srcPath))
			{
				_logger.Log(LogLevel.Information, $"Папка {_srcPath} не найдена");
				return Task.CompletedTask;
			}

			_logger.Log(LogLevel.Information, $"Обработка имеющихся на момент запуска файлов");
			EnqueueExistingFiles();

			_watcher = new FileSystemWatcher(_srcPath);
			_watcher.NotifyFilter = NotifyFilters.Attributes
			                        | NotifyFilters.CreationTime
			                        | NotifyFilters.DirectoryName
			                        | NotifyFilters.FileName
			                        | NotifyFilters.LastAccess
			                        | NotifyFilters.LastWrite
			                        | NotifyFilters.Security
			                        | NotifyFilters.Size;
			// перехватывает только копирование и создание файлов с раширением txt в заданную папку
			_watcher.Created += OnCreated;
			_watcher.Filter = "*.txt";
			_watcher.IncludeSubdirectories = false;
			_watcher.EnableRaisingEvents = true;
			return Task.CompletedTask;
		}

		private void OnCreated(object sender, FileSystemEventArgs e)
		{
			_logger.Log(LogLevel.Information, $"Обнаружен файл {e.FullPath}");
			_lettersCounter.Enqueue(e.FullPath);
		}

		/// <summary>
		/// Обработка существующих на момент запуска в папке файлов
		/// </summary>
		private void EnqueueExistingFiles()
		{
			foreach (var fileName in Directory.GetFiles(_srcPath,"*.txt"))
			{
				_logger.Log(LogLevel.Information, $"Обрабатываем файл {fileName}");
				_lettersCounter.Enqueue(fileName);
			}
		}

		public override void Dispose()
		{
			_watcher.Dispose();
			base.Dispose();
		}
	}
}
