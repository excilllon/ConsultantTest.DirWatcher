Тестовое задание: Разработать консольную программу-сервис обрабатывающую документы в TXT формате.

https://github.com/excilllon/ConsultantTest.DirWatcher/tree/master/ConsultantTest.DirWatcher - основной проект консольного приложения
https://github.com/excilllon/ConsultantTest.DirWatcher/tree/master/ConsultantTest.DirWatcher.Services - реализация сервисов мониторинга папки и подсчета букв
https://github.com/excilllon/ConsultantTest.DirWatcher/tree/master/ConsultantTest.DirWatcher.Abstractions - интерфейсы

Пример запуска: 
"ConsultantTest.DirWatchAndCount.exe" src="d:\consultant" res="d:\consultant\res"
src - путь к исходной папке, где будут появлться новые файлы с расширением TXT. Новые файлы можно только копировать в папку или сохранять прямо там.
res - путь к папке результатов, в ней для каждого файла из src будет создан файл с таким же названием и в его будет записано количество букв из исходного

Для корректного завершения приложения нужно нажимать Ctrl-C, только тогда вызывутся методы Dispose в сервисах, которые дадут потокам корректно закончить текущую работу.
