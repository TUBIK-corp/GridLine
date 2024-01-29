<p align="center">
    <br>
      <img src="Assets/Grid_Line_Normal.png"  width=25%  align="center"/>
      <h1 align="center">GridLine IDE</h1>
    <br>
<p>

<b> GridLine IDE <b> — это среда разработки для языка программирования LangLine, который предназначен для управления движением исполнителя по двумерной сетке. GridLine IDE позволяет создавать, редактировать, запускать и отлаживать программы на LangLine, а также визуализировать работу исполнителя на сетке. GridLine IDE — это идеальный инструмент для обучения программированию и робототехнике, так как он позволяет легко и наглядно создавать и тестировать программы на LangLine, а также изучать основы алгоритмизации и разработки программных систем.

## Установка и запуск
Для того, чтобы установить и запустить GridLine IDE, тебе нужно выполнить следующие шаги:
- Скачайте и установите GridLine из <b> [РЕЛИЗОВ](https://github.com/TUBIK-corp/GridLine/releases/tag/GridLine) <b>.
- Запустите установщик и следуйте шагам. (ВНИМАНИЕ! Не рекомендуется устанавливать в общую папку Program Files)

## Использование
Для того, чтобы использовать GridLine IDE, тебе нужно знать основы языка программирования LangLine, который предназначен для управления движением исполнителя по двумерной сетке. Ты можешь посмотреть справочник по LangLine [в его репозитории](https://github.com/TUBIK-corp/LangLine/).

GridLine IDE имеет удобный и интуитивный интерфейс, который состоит из следующих элементов:
- Редактор кода. Это область, где вы можете писать и изменять код на LangLine. Редактор кода поддерживает проверку ошибок и форматирование кода.
- Панель инструментов. Это область, где вы можете выполнять различные действия с твоим проектом, такие как создание, открытие, сохранение, запуск, остановка, шаговая отладка и т.д.
- Сетка. Это область, где вы можете видеть двумерную сетку, по которой перемещается исполнитель.
- Консоль. Это область, где вы можете видеть вывод твоей программы. Консоль также показывает сообщения об ошибках, предупреждениях и логах.

Вот пример программы на LangLine, которая заставляет исполнителя обойти сетку по периметру 64 раза:
```
PROCEDURE HelloWorld
  IFBLOCK RIGHT
    DOWN 1
  ENDIF
  IFBLOCK DOWN
    LEFT 1
  ENDIF
  IFBLOCK UP
    RIGHT 1
  ENDIF
  IFBLOCK LEFT
    UP 1
  ENDIF
ENDPROC

REPEAT 64
  CALL HelloWorld
ENDREPEAT
```

