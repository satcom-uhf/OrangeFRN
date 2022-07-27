# OrangeFRN
Extra GPIO features for AlterFRN client

Пример конфига
```json
{
  "DefaultState": {
    "3": 1,
    "5": 0,
    "7": 1
  },
  "Commands": {
    "bang!": {
      "pins": {
        "5": 1,
        "7": 0
      },
      "time": "00:00:05"
    },
    "booms": {
      "pins": {
        "3": 0
      },
      "time": "00:00:03"
    }
  }
}
```

Default state - объект, указывающий состояние пинов по-умолчанию (во время простоя и после выполнения команды). Ключ - номер пина в кавычках, через двоеточие указывается уровень 1 или 0.
Commands - объект, указывающий как реагировать на фразы в строчках лог-файла FRN. Каждая фраза описывается двумя настройками: pins - указывает на состояние пинов, которое должно быть установлено в случае обнаружения нужного текста в логе. Time указывает время ожидания до сброса в Defaul state.