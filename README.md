# starmania

LABYRINTH
#########



Описание
--------
Одиночная/Многопользовательская игра, в которой надо найти выход-портал из лабтиринта.
Лабиринт генерируется случайным образом.



Управление
----------
На экране:
Кнопка "Host" - создать Хост (или старт однопользовательского прохождения)
Кнопка "Client" - подсоединиться к Хосту
 
На клавиатуре:
AWSD 		- ходьба
AWSD и LShift 	- бег
Лев. кн. мыши	- стрельба
Прав. кн. мыши	- отвесить канат (работает только над ямами)
Tab 		- Карта
Esc		- Режим курсора
Enter		- Возврат в режим игры

Настройки
---------
Путь к настройкам:  %userprofile%\AppData\LocalLow\PulsarUniverse\Labyrinth\settings.json


Правила и механика
------------------

В лабиринте надо найти сундук с сокровищами. 

Слева отображается список игроков с информацией о расстоянии до цели. 

При стрельбе из игрока вылетает белый шар.
При попадании в стену белый шар превращается в круглую платформу.
Платформа исчезает со временем.

Если игрок стоит на краю ямы или пропасти, то он может "постороить" канат.

По всему лабиринту раскинуты "точки сохранения" ввиде темных шайб-таблеток.
Если игрок наступает на такую таблетку, то она становится его точкой респа.

Игрок умирает когда:
- попадает в лаву
- падает с большой высоты
- задевает моба

