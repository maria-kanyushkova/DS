# PA7. Знакомство с волновыми алгоритмами. Кольцевой алгоритм

## Цель
Понимать предназначение и принцип работы волновых алгоритмов, уметь реализовать волновой алгоритм в распределённой системе.

## Задание

Требуется разработать алгоритм, который ищет максимальное значение числа *X* в распределённой системе и распространяет его по всем процессам.

### Инициация

Каждый процесс после старта инициирует локальную переменную *X* (целое число) полученным через стандартный ввод значением.

### Кольцевой алгоритм

Процессы распределённой системы образуют замкнутую цепочку - *кольцо*: первый процесс отправляет сообщение второму и получает сообщение от последнего; второй - отправляет третьему и получает от второго и т.д.

Процессы (кроме первого) выполняют следующие действия:
1. инициирует сокет для входящих сообщений;
2. инициирует переменную X значением из стандартного ввода;
3. получает от предыдущего соседа число Y;
4. отправляет следующему соседу максимальное значение между X и Y;
5. получает от предыдущего соседа конечное значение X и выводит его в консоль.

Первый процесс является *инициатором* алгоритма.  Действия инициатора:
1. инициирует сокет для входящих сообщений;
2. инициирует переменную X значением из стандартного ввода;
3. отправляет следующему соседу значение X;
4. получает от предыдущего соседа число Y и записывает в переменную X;
5. отправляет следующему соседу значение X;
6. получает от предыдущего соседа конечное значение X и выводит его в консоль.

## Расположение проекта

В корне репозитория должна находиться папка **Chain** - проект типа консольное приложение.

Параметры запуска процесса *Chain*:

```
<listening-port> <next-host> <next-port> [true]
```

1. *listening-port*: номер порта, по которому процесс принимает сообщение от предыдущего соседа в кольце
2. *next-host*, *next-port*: имя хоста и порт для отправки сообщений следующему соседу в кольце
3. *true* - необязательный параметр, если указан - процесс является инициаторм алгоритма.

Пример запуска процесса инициатора:
```
$> dotnet run 1234 machine2.address 1235 true
```

Пример запуска процесса:
```
$> dotnet run 1235 machine3.address 1236
```

#### Консольный вывод процессов

Вывод каждого процесса должен состоять из одной строки - максимального значения X в распределённой системе.
К примеру, если процессам на вход были переданы значения 4, 17, -5, 32, 4, 5 то каждый процесс должен вывести строку со значением 32.