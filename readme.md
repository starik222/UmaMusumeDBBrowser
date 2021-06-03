# UmaMusumeDBBrowser
## Описание
UmaMusumeDBBrowser предназначена для просмотра базы данных игры Uma Musume Pretty Derby, а также считывания событий и умений с окна игры во время тренировки.

## Дополнительная информация
- Рекомендуется как можно сильнее растягивать окно игры, чтобы считывание данных с него было более качественным.
- Некоторые события и умения могут не распознаваться.
- Если какое-то умение не распозналось, то можно попробовать переместить это умение в другое место списка. 
Например, нераспознанное умение в игре находится вверху списка, если переместить это умение в середину или конец списка, то оно может распознаться.

## Скриншоты
![](https://user-images.githubusercontent.com/1236582/120617648-83bf1f80-c484-11eb-9809-45f068aad643.png)
![](https://user-images.githubusercontent.com/1236582/120617746-9b96a380-c484-11eb-997a-082388f2376e.png)
![](https://user-images.githubusercontent.com/1236582/120617799-a7826580-c484-11eb-86c7-0eb3a0ddf1f9.png)

## Редактирование отображаемых таблиз из БД
Настройки по отображаемым в программе таблицам хранятся в файле [Settings.ini](https://github.com/starik222/UmaMusumeDBBrowser/blob/master/UmaMusumeDBBrowser/Settings.ini).
В этом же файле есть описание структуры файла настроек.

## Использованные ресурсы
Данные по тренировочным событиям взяты из [UmaUmaCruise](https://github.com/amate/UmaUmaCruise) (Собственно как и идея:)), а также с ресурса https://gamerch.com/umamusume/

## Использованные библиотеки
[Emgu CV](https://github.com/emgucv/emgucv)

[Newtonsoft.Json](https://www.newtonsoft.com/json)

[Microsoft office core ver 15](https://www.nuget.org/packages/MicrosoftOfficeCore/15.0.0)

[Microsoft.Data.Sqlite](https://docs.microsoft.com/ru-ru/dotnet/standard/data/sqlite/?tabs=netcore-cli)

[Microsoft.Office.Interop.Excel](https://www.nuget.org/packages/Microsoft.Office.Interop.Excel/15.0.4795.1000)

## Отказ от ответственности
Автор не несет никакой ответственности за действия выполняемые программой. Пользователь совершает все действия на свой страх и риск.
