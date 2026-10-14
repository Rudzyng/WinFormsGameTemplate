# WinForms-шаблон игры LingvoGameOs

Это desktop-шаблон с актуальным API начисления баллов по игровой сессии и
HMAC-токену. Он теперь находится рядом с Flask- и ASP.NET-шаблонами в общей
папке `shablons`.

## Настройка API

Создайте `WinFormsGameTemplate/.env` на основе `.env.example` либо задайте
`LINGVO_API_URL` и реальный `LINGVO_GAME_ID` в окружении. Рабочий `.env` не
коммитится; при сборке он копируется рядом с exe.

## Передача игровой сессии

Шаблон понимает аргументы запуска:

```powershell
dotnet run --project .\WinFormsGameTemplate -- --session=123 --token=hmac-token
```

Также можно передать абсолютную launch-ссылку с query или переменные
`LINGVO_GAME_SESSION_ID` и `LINGVO_GAME_SESSION_TOKEN`. Контекст доступен в
`Form1.LaunchContext`, а результат отправляется методом
`SubmitScoreAsync(amount)`. При открытии формы шаблон сначала вызывает
`POST /api/game/session/resolve`, сверяет `GameId` и показывает имя и баллы
подтверждённого пользователя в заголовке окна.

Важно: WinForms-приложение не работает внутри iframe и не публикуется как
`/games/<slug>`. Для выдачи настоящих `session/token` ему потребуется отдельный
desktop-launcher или зарегистрированный URI-протокол на стороне портала. Сам
API-контракт уже тот же, что у веб-игр: `POST /api/score` с
`gameSessionId`, `score` и `token`.
