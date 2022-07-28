# OrangeFRN
Extra GPIO features for AlterFRN client

Бинарник нужно положить рядом с лог-файлом, выдать ему права (сhmod +x)
Перед запуском нужно подготовить конфигурационный файл config.json:

Пример конфига. Комментарии даны для наглядности, в Json-файле их быть не должно!
```javascript
{
  "Pins": [ 3, 5, 7 ], // Пины, которые мы будем использовать для управления тангентой
  "CommandPrefix": "MOTO", // набор символов, переводящий GPIO в режим готовности к набору на тангенте
  "CommandSuffix": "!", // набор символов, обозначающий окончание управления тангентой
  "DefaultLevel": 0, // Уровень на пинах Pins по-умолчанию. Для схемы с диодами - 1, для транзисторов - 0
  "ClickTimeMs": 300, // время удержания пинов в мс перед возвратом к дефолтному (ненажатому) состоянию
  "Commands": {
    "7": [ 1, 0, 1 ], // чтобы нажать цифру 7 нужно подать единицу на пин 3, нолик на пин 5 и единицу на пин 7
    "3": [ 0, 1, 0 ] // чтобы нажать цифру 3 нужно подать ноль на пин 3, единицу на пин 5 и нолик на пин 7
  }
}
```

Команды на нажатие клавиш могут быть внутри сообщения, должны начинаться с CommandPrefix, разделяться пробелами и заканчиваться CommandSuffix. Пример:
```javascript
Всем привет, давайте нажмем на тангенте семерочка троечка MOTO 7 3 ! все готово
```

Простейший конфиг управления пином 18:
```javascript
{
  "Pins": [ 18 ],
  "CommandPrefix": "MOTO", 
  "CommandSuffix": "!", 
  "DefaultLevel": 0, 
  "ClickTimeMs": 2000, // 2 сек
  "Commands": {
    "ON": [ 1 ]
  }
}
```

```MOTO ON!```
