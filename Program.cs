using System.Text.RegularExpressions;
using CryptoBot;
using CryptoBot.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static Dictionary<long, Telegram.Bot.Types.Message> lastMessage = new Dictionary<long, Telegram.Bot.Types.Message>();
    private static decimal MonoPercentage = 1.5m;
    private static decimal PryvatPercentage = 1.5m;
    private static decimal InshePercentage = 1.7m;
    private static List<string> currencies = new List<string>();
    private static Dictionary<long, CostomerModel> costomerModel = new Dictionary<long, CostomerModel>();
    private static Dictionary<long, bool> HowMuchGet = new Dictionary<long, bool>();
    private static long adminChatId;
    private static HttpClient httpClient = new HttpClient();
    private static BinanceAPIService binanceService = new BinanceAPIService(httpClient);
    private static Dictionary<long, bool> ifInshaHotivkaTaken = new Dictionary<long, bool>();
    private static Dictionary<long, bool> ifNotBankingTaken = new Dictionary<long, bool>();
    private static Dictionary<long, bool> ifCheckNumber = new Dictionary<long, bool>();
    private static Dictionary<long, bool> insheService = new Dictionary<long, bool>();
    private static Dictionary<long, bool> inshe = new Dictionary<long, bool>();
    private static Dictionary<long, bool> ifTRC20Taken = new Dictionary<long, bool>();
    private static Dictionary<long, bool> ifCheckOrder = new Dictionary<long, bool>();
    private static Dictionary<long, Message> AdminMessage = new Dictionary<long, Message>();
    private static Dictionary<long, Message> UserMessage = new Dictionary<long, Message>();
    static async Task Main(string[] args)
    {
        //Start 
        currencies.Add("Tether, USDT");
        currencies.Add("Monobank, UAH");
        currencies.Add("ПриватБанк, UAH");
        currencies.Add("Інший банкінг, UAH");
        currencies.Add("Готівка(USD,EUR,тощо)");
        currencies.Add("Інша послуга(переклад,інвойс,тощо)");
        currencies.Add("Інше(BTC,Revolut,тощо)");
        currencies.Add("Долар, USD");
        currencies.Add("Євро, EUR");
        currencies.Add("Перестановка готівки по світу ($/€)");
        currencies.Add("Оплата і прийом безготівки $/€ (товар, послуги, авто аукціони в США і т.п.)");
        currencies.Add("Оплата безготівки на фіз. обличчя (IBAN, Wise, Revolut, Zen та інші)");
        currencies.Add("Оплата будь-яких сум на ФОП");
        currencies.Add("Оплата юаня на карти фіз. облич");
        currencies.Add("Обмін з електронних платіжних систем (Advcash, Perfect money, Skrill, Payeer, NETELLER, Payoneer, Wise, Zen та інші)");
        currencies.Add("Виплата на карти Європи (при наявності IBAN карти)");
        currencies.Add("Інше (обмін BTC, GBP, індивідуальне завдання і т.п.)");
        currencies.Add("Гривня, UAH");

        var apiToken = Environment.GetEnvironmentVariable("API_TOKEN");

        var bot = new TelegramBotClient("7360889953:AAE5IDHDjW7ctNcpxLm0q2Bj9qdPU8T1QBs");

        var me = await bot.GetMeAsync();
        Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions
        );

        await Task.Delay(Timeout.Infinite);
    }

    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Type == UpdateType.Message && update.Message!.Text != null)
            {
                await OnMessage(botClient, update.Message, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await OnCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
            }
            else if (update.Message != null && update.Message.Contact != null)
            {
                OnAddNumber(botClient, update, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Невідома помилка: {ex.Message}");
            return;
        }
    }

    static async Task OnAddNumber(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var chatId = update.Message.Chat.Id;
            costomerModel[chatId].Phone = update.Message.Contact.PhoneNumber;
            costomerModel[chatId].FirstName = update.Message.Contact.FirstName;
            costomerModel[chatId].LastName = update.Message.Contact.LastName;

            var keyboard = new ReplyKeyboardMarkup(new[]
                    {
                    new[]
                    {
                        new KeyboardButton("Нова заявка 📥"),
                        new KeyboardButton("Умови та про нас 📃")
                    },
                    new[]
                    {
                        new KeyboardButton("Ваші відгуки 💬"),
                        new KeyboardButton("Наша спільнота 📣")
                    }
                })
            {
                OneTimeKeyboard = false,
                ResizeKeyboard = true
            };

            await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"*Контакт отримано* ✅ Тепер ми на зв'язку.",
                   replyMarkup: keyboard,
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );

            ProccesHowManyGive(botClient, chatId, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Невідома помилка: {ex.Message}");
            return;
        }
    }
    static async Task OnMessage(ITelegramBotClient botClient, Message msg, CancellationToken cancellationToken)
    {
        var chatId = msg.Chat.Id;

        try
        {
            if (msg.Text == "/start")
            {
                ZeroVariables(botClient, chatId, cancellationToken);
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[]
                    {
                        new KeyboardButton("Нова заявка 📥"),
                        new KeyboardButton("Умови та про нас 📃")
                    },
                    new[]
                    {
                        new KeyboardButton("Ваші відгуки 💬"),
                        new KeyboardButton("Наша спільнота 📣")
                    }
                })
                {
                    OneTimeKeyboard = false,
                    ResizeKeyboard = true
                };

                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "*Меню* 🔁:",
                   replyMarkup: keyboard,
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (msg.Text == "Нова заявка 📥")
            {
                ZeroVariables(botClient, chatId, cancellationToken);

                if (!costomerModel.ContainsKey(chatId))
                {
                    costomerModel.Add(chatId, new CostomerModel() { Username = msg.From.Username });
                }
                else
                {
                    costomerModel[chatId].Username = msg.From.Username;
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Обмін Гривня 🔁 USDT (банкінг) 💳", "exchangeUAH")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Обмін Готівка 🔁 USDT 💵", "exchangeUSD")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Інший обмін та послуги з криптовалютами🧾", "other")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("Менеджер👨🏻‍💻", "https://t.me/exchanger13")
                    },
                });

                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "Нова заявка 📥",
                   replyMarkup: inlineKeyboard,
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (msg.Text == "Умови та про нас 📃")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("Ознайомитись↗️", "https://telegra.ph/Umovi-obm%D1%96nu-ta-graf%D1%96k-roboti-09-09")
                    }
                });

                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "Умови та про нас 📃",
                   replyMarkup: inlineKeyboard,
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (msg.Text == "Ваші відгуки 💬")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("Переглянути↗️", "t.me/reviews_13exchanger"),
                    }
                });

                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "Ваші відгуки 💬",
                   replyMarkup: inlineKeyboard,
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (msg.Text == "Наша спільнота 📣")
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("Приєднатись↗️", "https://t.me/+upJjUrcOTR8wMjAy"),
                    }
                });

                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "Наша спільнота 📣",
                   replyMarkup: inlineKeyboard,
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (decimal.TryParse(msg.Text, out decimal count) && count.ToString().Length != 16 && ifCheckNumber.ContainsKey(chatId) && ifCheckNumber[chatId] == false)
            {
                if (costomerModel.ContainsKey(chatId))
                {
                    if (HowMuchGet.ContainsKey(chatId))
                    {
                        if (!ifTRC20Taken[chatId])
                        {
                            if (costomerModel[chatId].IfEnd)
                            {
                                if (HowMuchGet[chatId] == true)
                                {
                                    if (costomerModel[chatId].CurrencyCell == currencies[0])
                                    {
                                        if (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[3])
                                        {
                                            var course = costomerModel[chatId].Course;
                                            costomerModel[chatId].HowMuchGives = Math.Round(count / course, 2);
                                            costomerModel[chatId].HowMuchGet = count;
                                            var min = 100 * course;
                                            var max = 20000 * course;

                                            if (count < min || count > max)
                                            {
                                                await botClient.SendTextMessageAsync(
                                                   chatId: chatId,
                                                   text: $"❗ Некоректна форма введення ❗️",
                                                   parseMode: ParseMode.Markdown,
                                                   cancellationToken: cancellationToken
                                               );
                                                return;
                                            }

                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                                },
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                                }
                                            });

                                            Random random = new Random();
                                            int randomNumber = random.Next(100, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            var order = costomerModel[chatId].Order == true ? "Так, через ордер" : "Ні, без ордера";
                                            var getCurr = costomerModel[chatId].CurrencyGet == currencies[1] ? "UAH" : "USDT";
                                            var card = costomerModel[chatId].Order ? " " : $"\n💳 Номер карти: *{costomerModel[chatId].CardNumber}*";

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} USDT*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} UAH*\n \n🔐 P2P-ордер: *{order}*{card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}\n \nСтатус заявки: *Не підтверджена* ⚠️",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                        else if (costomerModel[chatId].CurrencyGet == currencies[2])
                                        {
                                            var course = costomerModel[chatId].Course;
                                            costomerModel[chatId].HowMuchGives = Math.Round(count / course, 2);
                                            costomerModel[chatId].HowMuchGet = count;

                                            var min = 100 * course;
                                            var max = 20000 * course;

                                            if (count < min || count > max)
                                            {
                                                await botClient.SendTextMessageAsync(
                                                   chatId: chatId,
                                                   text: $"❗ Некоректна форма введення ❗️",
                                                   parseMode: ParseMode.Markdown,
                                                   cancellationToken: cancellationToken
                                               );
                                                return;
                                            }

                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                                    });

                                            Random random = new Random();
                                            int randomNumber = random.Next(100, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            var order = costomerModel[chatId].Order == true ? "Так, через ордер" : "Ні, без ордера";
                                            var getCurr = costomerModel[chatId].CurrencyGet == currencies[1] ? "UAH" : "USDT";
                                            var card = costomerModel[chatId].Order ? " " : $"\n💳 Номер карти: *{costomerModel[chatId].CardNumber}*";

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} USDT*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} UAH*\n \n🔐 P2P-ордер: *{order}*{card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                        else if (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17])
                                        {
                                            if (costomerModel[chatId].CurrencyGet == currencies[17] && (count < 20000 || count > 4500000))
                                            {
                                                await botClient.SendTextMessageAsync(
                                                    chatId: chatId,
                                                    text: $"❗ Некоректна форма введення ❗️",
                                                    parseMode: ParseMode.Markdown,
                                                    cancellationToken: cancellationToken
                                                );
                                                return;
                                            }
                                            else if ((costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8]) && (count < 500 || count > 100000))
                                            {
                                                await botClient.SendTextMessageAsync(
                                                    chatId: chatId,
                                                    text: $"❗ Некоректна форма введення ❗️",
                                                    parseMode: ParseMode.Markdown,
                                                    cancellationToken: cancellationToken
                                                );
                                                return;
                                            }

                                            costomerModel[chatId].HowMuchGet = count;
                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                                },
                                                new[]
                                                {
                                                    InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                                }
                                            });

                                            Random random = new Random();
                                            int randomNumber = random.Next(101, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            string valute = costomerModel[chatId].CurrencyGet == currencies[7] ? "USD" : "EUR";
                                            if (costomerModel[chatId].CurrencyGet == currencies[17]) valute = "UAH";

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID: *{randomNumber}*\n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами валюти повідомить менеджер після підтвердження.\n \n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} {valute}*\n \n📲 Контакт: *Ярослав*, @yarius13\n \nСтатус заявки: *Не підтверджена* ⚠️",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                    }
                                    else if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3] || costomerModel[chatId].CurrencyCell == currencies[2])
                                    {
                                        if (count < 100 || count > 20000)
                                        {
                                            await botClient.SendTextMessageAsync(
                                               chatId: chatId,
                                               text: "❗ Некоректна форма введення ❗️",
                                               parseMode: ParseMode.Markdown,
                                               cancellationToken: cancellationToken
                                           );
                                            return;
                                        }
                                        if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3])
                                        {
                                            var course = costomerModel[chatId].Course;
                                            costomerModel[chatId].HowMuchGives = count * course;
                                            costomerModel[chatId].HowMuchGet = count;

                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                                    });

                                            Random random = new Random();
                                            int randomNumber = random.Next(101, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            var order = costomerModel[chatId].Order == true ? "Так, через ордер" : "Ні, без ордера";
                                            var getCurr = costomerModel[chatId].CurrencyGet == currencies[1] ? "UAH" : "USDT";
                                            var card = costomerModel[chatId].Order ? " " : $"\n💸 Адреса гаманця TRC20: *{costomerModel[chatId].CardNumber}*";

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} UAH*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} USDT*\n \n🔐 P2P-ордер: *{order}*{card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                        else if (costomerModel[chatId].CurrencyCell == currencies[2])
                                        {
                                            var course = costomerModel[chatId].Course;
                                            costomerModel[chatId].HowMuchGives = count * course;
                                            costomerModel[chatId].HowMuchGet = count;

                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                                    });

                                            Random random = new Random();
                                            int randomNumber = random.Next(101, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            var order = costomerModel[chatId].Order == true ? "Так, через ордер" : "Ні, без ордера";
                                            var getCurr = costomerModel[chatId].CurrencyGet == currencies[1] ? "UAH" : "USDT";
                                            var card = costomerModel[chatId].Order ? " " : $"\n💸 Адреса гаманця TRC20: *{costomerModel[chatId].CardNumber}*";

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} UAH*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} USDT*\n \n🔐 P2P-ордер: *{order}*{card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                    }
                                    else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])
                                    {
                                        if (count < 500 || count > 100000)
                                        {
                                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"❗ Некоректна форма введення ❗️",
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            return;
                                        }
                                        costomerModel[chatId].HowMuchGet = count;

                                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                        {
                                            new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                            },
                                            new[]
                                            {
                                                InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                            }
                                        });

                                        Random random = new Random();
                                        int randomNumber = random.Next(101, 1000);
                                        costomerModel[chatId].Id = randomNumber;

                                        UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: $"📥 Заявка ID: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами валюти повідомить менеджер після підтвердження.\n \n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} USDT*\n \n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}\n \nСтатус заявки: *Не підтверджена* ⚠️",
                                            replyMarkup: inlineKeyboard,
                                            parseMode: ParseMode.Markdown,
                                            cancellationToken: cancellationToken
                                        );
                                        SendToAdmin(botClient, chatId, cancellationToken);
                                    }
                                }
                                else
                                {
                                    if (costomerModel[chatId].CurrencyCell == currencies[0])
                                    {
                                        if (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[3])
                                        {
                                            if (count < 100 || count > 20000)
                                            {
                                                await botClient.SendTextMessageAsync(
                                                   chatId: chatId,
                                                   text: "❗ Некоректна форма введення ❗️",
                                                   parseMode: ParseMode.Markdown,
                                                   cancellationToken: cancellationToken
                                               );
                                                return;
                                            }

                                            var course = costomerModel[chatId].Course;
                                            costomerModel[chatId].HowMuchGives = count;
                                            costomerModel[chatId].HowMuchGet = count * course;

                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                                    });

                                            Random random = new Random();
                                            int randomNumber = random.Next(101, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            var getCurr = costomerModel[chatId].CurrencyGet == currencies[1] ? "UAH" : "USDT";
                                            var order = costomerModel[chatId].Order == true ? "Так, через ордер" : "Ні, без ордера";
                                            var confirm = "Не підтверджена ⚠️";
                                            var card = costomerModel[chatId].Order ? " " : $"\n💳 Номер карти: *{costomerModel[chatId].CardNumber}*";

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} USDT*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} UAH*\n \n🔐 P2P-ордер: *{order}*{card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                        else if (costomerModel[chatId].CurrencyGet == currencies[2])
                                        {
                                            if (count < 100 || count > 20000)
                                            {
                                                await botClient.SendTextMessageAsync(
                                                   chatId: chatId,
                                                   text: "❗ Некоректна форма введення ❗️",
                                                   parseMode: ParseMode.Markdown,
                                                   cancellationToken: cancellationToken
                                               );
                                                return;
                                            }

                                            var course = costomerModel[chatId].Course;
                                            costomerModel[chatId].HowMuchGives = count;
                                            costomerModel[chatId].HowMuchGet = count * course;

                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                                    });

                                            Random random = new Random();
                                            int randomNumber = random.Next(101, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            var getCurr = costomerModel[chatId].CurrencyGet == currencies[1] ? "UAH" : "USDT";
                                            var order = costomerModel[chatId].Order == true ? "Так, через ордер" : "Ні, без ордера";
                                            var confirm = "Не підтверджена ⚠️";
                                            var card = costomerModel[chatId].Order ? " " : $"\n💳 Номер карти: *{costomerModel[chatId].CardNumber}*";

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} USDT*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} UAH*\n \n🔐 P2P-ордер: *{order}*{card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                        else if (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17])
                                        {
                                            if (count < 500 || count > 100000)
                                            {
                                                await botClient.SendTextMessageAsync(
                                               chatId: chatId,
                                               text: "❗ Некоректна форма введення ❗️",
                                               parseMode: ParseMode.Markdown,
                                               cancellationToken: cancellationToken
                                               );
                                                return;
                                            }
                                            costomerModel[chatId].HowMuchGives = count;

                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                                    });

                                            Random random = new Random();
                                            int randomNumber = random.Next(101, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID: *{randomNumber}*\n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами валюти повідомить менеджер після підтвердження.\n \n💰Сума, яку віддаєте: *{costomerModel[chatId].HowMuchGives} USDT*\n \n📲 Контакт: *Ярослав*, @yarius13\n \nСтатус заявки: *Не підтверджена* ⚠️",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                    }
                                    else if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3] || costomerModel[chatId].CurrencyCell == currencies[2])
                                    {
                                        if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3])
                                        {
                                            var course = costomerModel[chatId].Course;
                                            costomerModel[chatId].HowMuchGives = count;
                                            costomerModel[chatId].HowMuchGet = Math.Round(count / course, 2);

                                            var min = 100 * course;
                                            var max = 20000 * course;

                                            if (count < min || count > max)
                                            {
                                                await botClient.SendTextMessageAsync(
                                                   chatId: chatId,
                                                   text: $"❗ Некоректна форма введення ❗️",
                                                   parseMode: ParseMode.Markdown,
                                                   cancellationToken: cancellationToken
                                               );
                                                return;
                                            }

                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                                    });

                                            Random random = new Random();
                                            int randomNumber = random.Next(101, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            var getCurr = costomerModel[chatId].CurrencyGet == currencies[1] ? "UAH" : "USDT";
                                            var order = costomerModel[chatId].Order == true ? "Так, через ордер" : "Ні, без ордера";
                                            var card = costomerModel[chatId].Order ? " " : $"\n💸 Адреса гаманця TRC20: *{costomerModel[chatId].CardNumber}*";

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} UAH*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} USDT*\n \n🔐 P2P-ордер: *{order}*{card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                        else if (costomerModel[chatId].CurrencyCell == currencies[2])
                                        {

                                            var course = costomerModel[chatId].Course;
                                            costomerModel[chatId].HowMuchGives = count;
                                            costomerModel[chatId].HowMuchGet = Math.Round(count / course, 2);

                                            var min = 100 * course;
                                            var max = 20000 * course;

                                            if (count < min || count > max)
                                            {
                                                await botClient.SendTextMessageAsync(
                                                   chatId: chatId,
                                                   text: $"❗ Некоректна форма введення ❗️",
                                                   parseMode: ParseMode.Markdown,
                                                   cancellationToken: cancellationToken
                                               );
                                                return;
                                            }

                                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                            {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                                    });

                                            Random random = new Random();
                                            int randomNumber = random.Next(101, 1000);
                                            costomerModel[chatId].Id = randomNumber;

                                            var order = costomerModel[chatId].Order == true ? "Так, через ордер" : "Ні, без ордера";
                                            var getCurr = costomerModel[chatId].CurrencyGet == currencies[1] ? "UAH" : "USDT";
                                            var card = costomerModel[chatId].Order ? " " : $"\n💸 Адреса гаманця TRC20: *{costomerModel[chatId].CardNumber}*";

                                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"📥 Заявка ID:: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} UAH*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} USDT*\n \n🔐 P2P-ордер: *{order}*{card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}",
                                                replyMarkup: inlineKeyboard,
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            SendToAdmin(botClient, chatId, cancellationToken);
                                        }
                                    }
                                    else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])
                                    {
                                        costomerModel[chatId].HowMuchGives = count;

                                        if (costomerModel[chatId].CurrencyCell == currencies[17] && count < 20000 || count > 4000000)
                                        {
                                            await botClient.SendTextMessageAsync(
                                                chatId: chatId,
                                                text: $"❗ Некоректна форма введення ❗️",
                                                parseMode: ParseMode.Markdown,
                                                cancellationToken: cancellationToken
                                            );
                                            return;
                                        }
                                        if ((costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8]) && (count < 500 || count > 100000))
                                        {
                                            await botClient.SendTextMessageAsync(
                                               chatId: chatId,
                                               text: $"❗ Некоректна форма введення ❗️",
                                               parseMode: ParseMode.Markdown,
                                               cancellationToken: cancellationToken
                                            );
                                            return;
                                        }

                                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                                {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                                    });

                                        Random random = new Random();
                                        int randomNumber = random.Next(101, 1000);
                                        costomerModel[chatId].Id = randomNumber;

                                        string valute = costomerModel[chatId].CurrencyCell == currencies[7] ? "USD" : "EUR";
                                        if (costomerModel[chatId].CurrencyCell == currencies[17]) valute = "UAH";

                                        UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: $"📥 Заявка ID: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами валюти повідомить менеджер після підтвердження.\n \n💰Сума, яку віддаєте: *{costomerModel[chatId].HowMuchGives} {valute}*\n \n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}\n \nСтатус заявки: *Не підтверджена* ⚠️",
                                            replyMarkup: inlineKeyboard,
                                            parseMode: ParseMode.Markdown,
                                            cancellationToken: cancellationToken
                                        );
                                        SendToAdmin(botClient, chatId, cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                               chatId: chatId,
                                               text: $"❗ Некоректна форма введення ❗️",
                                               parseMode: ParseMode.Markdown,
                                               cancellationToken: cancellationToken
                                           );
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                               chatId: chatId,
                                               text: $"❗ Некоректна форма введення ❗️",
                                               parseMode: ParseMode.Markdown,
                                               cancellationToken: cancellationToken
                                           );
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                                           chatId: chatId,
                                           text: $"❗ Некоректна форма введення ❗️",
                                           parseMode: ParseMode.Markdown,
                                           cancellationToken: cancellationToken
                                       );
                    }
                }
            }
            else if (long.TryParse(msg.Text, out long cardNumber) && cardNumber.ToString().Length == 16 && ifCheckNumber.ContainsKey(chatId))
            {
                if (!ifCheckNumber[chatId])
                {
                    await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "❗ Некоректна форма введення ❗️",
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
                   );

                    return;
                }

                ifCheckNumber[chatId] = false;

                if (costomerModel.ContainsKey(chatId))
                {
                    costomerModel[chatId].CardNumber = cardNumber.ToString();
                    if (costomerModel[chatId].Phone == null)
                    {
                        var keyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new[]
                            {
                            KeyboardButton.WithRequestContact("Надіслати контакт 📲")
                            }
                        })
                        {
                            OneTimeKeyboard = true,
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Надішліть ваш *контакт* Telegram, щоб *менеджер*👨🏻‍💻 міг з вами зв'язатись.",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: keyboard);
                    }
                    else
                    {
                        ProccesHowManyGive(botClient, chatId, cancellationToken);
                    }
                }
            }
            else if (msg.Text == "/admingroup")
            {
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "Введіть пароль",
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (msg.Text == "/vsbhupw383e2asnx390g")
            {
                adminChatId = chatId;


                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "Цей чат тепер для адмінів",
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (ifInshaHotivkaTaken.ContainsKey(chatId) && ifTRC20Taken.ContainsKey(chatId) && (ifInshaHotivkaTaken[chatId] || ifTRC20Taken[chatId]))
            {
                if (ifInshaHotivkaTaken[chatId])
                {
                    costomerModel[chatId].CurrencyGet = msg.Text;

                    if (costomerModel[chatId].Phone == null)
                    {
                        var keyboard = new ReplyKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            KeyboardButton.WithRequestContact("Надіслати контакт 📲")
                        }
                    })
                        {
                            OneTimeKeyboard = true,
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Надішліть ваш *контакт* Telegram, щоб *менеджер*👨🏻‍💻 міг з вами зв'язатись.",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: keyboard);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: "Дякуємо за довіру, очікуйте декілька хвилин з вами зв'яжеться менеджер для здійснення угоди.",
                           parseMode: ParseMode.Markdown,
                           cancellationToken: cancellationToken
                       );
                    }
                }
                else if (ifTRC20Taken[chatId])
                {
                    string pattern = @"^[a-zA-Z0-9]+$";

                    bool isValid = Regex.IsMatch(msg.Text, pattern);
                    if (!isValid)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "❗ Некоректна форма введення ❗️"
                        );
                        return;
                    }
                    if (msg.Text.Length > 100)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "❗Максимум 100 знаків❗"
                        );
                        return;
                    }

                    costomerModel[chatId].CardNumber = msg.Text;
                    ifTRC20Taken[chatId] = false;

                    if (costomerModel[chatId].Phone == null)
                    {
                        var keyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new[]
                            {
                            KeyboardButton.WithRequestContact("Надіслати контакт 📲")
                            }
                        })
                        {
                            OneTimeKeyboard = true,
                            ResizeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Надішліть ваш *контакт* Telegram, щоб *менеджер*👨🏻‍💻 міг з вами зв'язатись.",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: keyboard);
                    }
                    else
                    {
                        decimal min = 0;
                        decimal max = 0;
                        if (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3]))
                        {
                            decimal course = costomerModel[chatId].Course;

                            min = 100 * course;
                            max = 20000 * course;

                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки отримаю ⬅️", "howManyGet"),
                                }
                            });

                            await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: $"Введіть суму *{costomerModel[chatId].CurrencyCell}* яку віддаєте ➡️ (ліміт: *{min} UAH* - *{max} UAH*):",
                               replyMarkup: inlineKeyboard,
                               parseMode: ParseMode.Markdown,
                               cancellationToken: cancellationToken
                           );
                        }
                        else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки отримаю ⬅️", "howManyGet"),
                                }
                            });

                            string valute = costomerModel[chatId].CurrencyCell == currencies[7] ? "USD" : "EUR";
                            if (costomerModel[chatId].CurrencyCell == currencies[17]) valute = "UAH";
                            min = costomerModel[chatId].CurrencyCell == currencies[17] ? 20000 : 500;
                            max = costomerModel[chatId].CurrencyCell == currencies[17] ? 4000000 : 100000;

                            await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: $"Введіть скільки ви віддаєте ➡️ (ліміт: *{min} {valute} - {max} {valute}*):",
                               replyMarkup: inlineKeyboard,
                               parseMode: ParseMode.Markdown,
                               cancellationToken: cancellationToken
                           );
                        }
                    }
                }
            }
            else if (inshe.ContainsKey(chatId) && ifCheckNumber.ContainsKey(chatId))
            {
                if (ifCheckNumber[chatId])
                {
                    await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❗ Некоректна форма введення ❗️",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
                    return;
                }
                if (inshe[chatId] == true)
                {
                    if (msg.Text.Replace(" ", "").Length > 500)
                    {
                        await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "❗ Некоректна форма введення ❗️",
                       parseMode: ParseMode.Markdown,
                       cancellationToken: cancellationToken
                       );
                        return;
                    }

                    costomerModel[chatId].Service = msg.Text;

                    if (costomerModel[chatId].Phone == null)
                    {
                        var keyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new[]
                            {
                            KeyboardButton.WithRequestContact("Надіслати контакт 📲")
                            }
                        })
                        {
                            OneTimeKeyboard = true,
                            ResizeKeyboard = true
                        };

                        insheService[chatId] = true;

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Надішліть ваш *контакт* Telegram, щоб *менеджер*👨🏻‍💻 міг з вами зв'язатись.",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: keyboard);
                    }
                    else
                    {
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                            },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                        });

                        Random random = new Random();
                        int randomNumber = random.Next(100, 1000);
                        costomerModel[chatId].Id = randomNumber;

                        UserMessage[chatId] = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"📥 Заявка ID: *{randomNumber}*\n \n💰 Послуга: *{costomerModel[chatId].Service}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами послуги повідомить менеджер після підтвердження.\n \n📲 Контакт: *Ярослав*, @yarius13\n \nСтатус заявки: *Не підтверджена* ⚠️",
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );

                        SendToAdmin(botClient, chatId, cancellationToken);
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "❗ Некоректна форма введення ❗️",
                       parseMode: ParseMode.Markdown,
                       cancellationToken: cancellationToken
                   );
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "❗ Некоректна форма введення ❗️",
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
        }
        catch (Exception ex)
        {
            await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "❗ Некоректна форма введення ❗️",
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            Console.WriteLine($"Невідома помилка: {ex.Message}");
            return;
        }
    }

    static async Task OnCallbackQuery(ITelegramBotClient botClient, CallbackQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var chatId = query.Message.Chat.Id;
            if (query.Data != null)
            {
                if (currencies.Any(x => x.ToLower() == query.Data || currencies.Any(x => x.ToLower() == query.Data.Substring(0, query.Data.Length - 1))) || currencies.Any(x => x.ToLower() == query.Data.Substring(0, query.Data.Length - 2)))
                {
                    for (int i = 0; i < currencies.Count; i++)
                    {
                        if (query.Data == currencies[i].ToLower() && currencies[i] != null)
                        {
                            await AddCellCurrency(currencies[i]);
                            break;
                        }
                        else if (query.Data == (currencies[i] + $"{i + 1}").ToLower() && (currencies[i] + $"{i + 1}") != null)
                        {
                            await AddGetCurrency(currencies[i]);
                            break;
                        }
                    }
                }
                else if (query.Data == "exchangeUAH")
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[0], currencies[0].ToLower()),
                            InlineKeyboardButton.WithCallbackData(currencies[1], currencies[1].ToLower()),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[2], currencies[2].ToLower()),
                            InlineKeyboardButton.WithCallbackData(currencies[3], currencies[3].ToLower()),
                        },
                    });

                    await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "Оберіть, що *віддаєте* ➡️:",
                       replyMarkup: inlineKeyboard,
                       parseMode: ParseMode.Markdown,
                       cancellationToken: cancellationToken
                   );
                }
                else if (query.Data == "exchangeUSD")
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(currencies[0], currencies[0].ToLower()),
                    InlineKeyboardButton.WithCallbackData(currencies[7], currencies[7].ToLower()),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(currencies[8], currencies[8].ToLower()),
                    InlineKeyboardButton.WithCallbackData(currencies[17], currencies[17].ToLower()),
                },
            });

                    ifNotBankingTaken[chatId] = true;

                    await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "Оберіть, що *віддаєте* ➡️:",
                       replyMarkup: inlineKeyboard,
                       parseMode: ParseMode.Markdown,
                       cancellationToken: cancellationToken
                   );
                }
                else if (query.Data == "YesOrder" || query.Data == "NoOrder")
                {
                    if (query.Data == "NoOrder")
                    {
                        if (costomerModel[chatId].CurrencyCell == currencies[0])
                        {
                            if (costomerModel[chatId].CurrencyGet == currencies[1])
                            {
                                ProcessGetValue(botClient, chatId, cancellationToken, 1);
                            }
                            else if (costomerModel[chatId].CurrencyGet == currencies[2])
                            {
                                ProcessGetValue(botClient, chatId, cancellationToken, 2);
                            }
                            else if (costomerModel[chatId].CurrencyGet == currencies[3])
                            {
                                ProcessGetValue(botClient, chatId, cancellationToken, 3);
                            }
                            else if (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17])
                            {
                                ProcessGetValue(botClient, chatId, cancellationToken, 8);
                            }
                        }
                        else
                        {
                            ifTRC20Taken[chatId] = true;
                            ProcessGetValue(botClient, chatId, cancellationToken, 0);
                        }
                    }
                    else
                    {
                        costomerModel[chatId].Order = true;

                        if (costomerModel[chatId].Phone == null)
                        {
                            var keyboard = new ReplyKeyboardMarkup(new[]
                            {
                            new[]
                            {
                            KeyboardButton.WithRequestContact("Надіслати контакт 📲")
                            }
                        })
                            {
                                OneTimeKeyboard = true,
                                ResizeKeyboard = true
                            };

                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Надішліть ваш *контакт* Telegram, щоб *менеджер*👨🏻‍💻 міг з вами зв'язатись.",
                                parseMode: ParseMode.Markdown,
                                replyMarkup: keyboard);
                        }
                        else
                        {
                            ProccesHowManyGive(botClient, chatId, cancellationToken);
                        }
                    }
                }
                else if (query.Data == "howManyGet")
                {
                    HowMuchGet[chatId] = true;
                    if (costomerModel.ContainsKey(chatId))
                    {
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки віддаю ➡️", "howMuchGive"),
                            }
                        });

                        if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[2] || costomerModel[chatId].CurrencyGet == currencies[3]))
                        {
                            var course = costomerModel[chatId].Course;
                            var min = 100 * course;
                            var max = 20000 * course;
                            await botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: $"Введіть суму *{costomerModel[chatId].CurrencyGet}*, яку отримаєте ⬅️ (ліміт: *{min} UAH* - *{max} UAH*):",
                               parseMode: ParseMode.Markdown,
                               replyMarkup: inlineKeyboard,
                               cancellationToken: cancellationToken
                           );
                        }
                        else if (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3]))
                        {
                            var min = 100;
                            var max = 20000;
                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Введіть суму *{costomerModel[chatId].CurrencyGet}*, яку отримаєте ⬅️ (ліміт: *{min} USDT* - *{max} USDT*):",
                                replyMarkup: inlineKeyboard,
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17]))
                        {
                            string valute = costomerModel[chatId].CurrencyGet == currencies[7] ? "USD" : "EUR";
                            if (costomerModel[chatId].CurrencyGet == currencies[17]) valute = "UAH";
                            var min = costomerModel[chatId].CurrencyGet == currencies[17] ? 20000 : 500;
                            var max = costomerModel[chatId].CurrencyGet == currencies[17] ? 4000000 : 100000;
                            await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $"Введіть суму *{costomerModel[chatId].CurrencyGet}*, яку отримаєте ⬅️ (ліміт: *{min} {valute}* - *{max} {valute}*):",
                                    replyMarkup: inlineKeyboard,
                                    parseMode: ParseMode.Markdown,
                                    cancellationToken: cancellationToken
                                );
                        }
                        else if (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17]))
                        {
                            await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $"Введіть суму *{costomerModel[chatId].CurrencyGet}*, яку отримаєте ⬅️ (ліміт: *500 USDT* - *100000 USDT*):",
                                    parseMode: ParseMode.Markdown,
                                    replyMarkup: inlineKeyboard,
                                    cancellationToken: cancellationToken
                                );
                        }
                    }
                }
                else if (query.Data == "howMuchGive")
                {
                    HowMuchGet[chatId] = false;
                    ProccesHowManyGive(botClient, chatId, cancellationToken);
                }
                else if (query.Data == "other")
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[9], "w1"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[10], "w2"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[11], "w3"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[12], "w4"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[13], "w5"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[14], "w6"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[15], "w7"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(currencies[16], "w8"),
                        },
                    });

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Інший обмін та послуги з криптовалютами🧾",
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken

                    );
                }
                else if (query.Data == "w1" || query.Data == "w2" || query.Data == "w3" || query.Data == "w4" || query.Data == "w5" || query.Data == "w6" || query.Data == "w7")
                {
                    if (costomerModel.ContainsKey(chatId))
                    {
                        switch (query.Data)
                        {
                            case "w1":
                                Check(currencies[9]);
                                break;
                            case "w2":
                                Check(currencies[10]);
                                break;
                            case "w3":
                                Check(currencies[11]);
                                break;
                            case "w4":
                                Check(currencies[12]);
                                break;
                            case "w5":
                                Check(currencies[13]);
                                break;
                            case "w6":
                                Check(currencies[14]);
                                break;
                            case "w7":
                                Check(currencies[15]);
                                break;
                        }

                        if (costomerModel[chatId].Phone == null)
                        {
                            var keyboard = new ReplyKeyboardMarkup(new[]
                            {
                            new[]
                            {
                            KeyboardButton.WithRequestContact("Надіслати контакт 📲")
                            }
                        })
                            {
                                OneTimeKeyboard = true,
                                ResizeKeyboard = true
                            };

                            insheService[chatId] = true;

                            await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Надішліть ваш *контакт* Telegram, щоб *менеджер*👨🏻‍💻 міг з вами зв'язатись.",
                                parseMode: ParseMode.Markdown,
                                replyMarkup: keyboard);
                        }
                        else
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                            });

                            Random random = new Random();
                            int randomNumber = random.Next(100, 1000);
                            costomerModel[chatId].Id = randomNumber;

                            UserMessage[chatId] = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"📥 Заявка ID: *{randomNumber}*\n \n💰 Послуга: *{costomerModel[chatId].Service}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами послуги повідомить менеджер після підтвердження.\n \n📲 Контакт: *Ярослав*, @yarius13\n \nСтатус заявки: *Не підтверджена* ⚠️",
                                replyMarkup: inlineKeyboard,
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                            SendToAdmin(botClient, chatId, cancellationToken);
                        }
                    }
                }
                else if (query.Data == "accses")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "<b>Заявку підтверджено</b> ✅\nЧерез декілька хвилин з вами зв'яжеться <b>менеджер</b> для здійснення угоди.\n\n<i>Графік роботи: Пн-Нд, 09:00 - 22:00</i> 📆\n\n<b>Дякуємо за довіру</b> ❤️",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken
                    );
                    await ChangeAgminMessage(botClient, chatId, cancellationToken, true);
                    await ChangeUserMessage(botClient, chatId, cancellationToken, true);
                    await ZeroVariables(botClient, chatId, cancellationToken);
                }
                else if (query.Data == "disable")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "*Меню* 🔁:",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );

                    ChangeAgminMessage(botClient, chatId, cancellationToken, false);
                    ChangeUserMessage(botClient, chatId, cancellationToken, false);
                    ZeroVariables(botClient, chatId, cancellationToken);
                }
                else if (query.Data == "w8")
                {
                    inshe[chatId] = true;
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Опишіть *детально* ваше завдання або обмін, які хочете здійснити. Вкажіть більше інформації та суму для того, щоб ми могли вам допомогти (ліміт: *500 символів*)📝",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "❗ Incorrect command ❗",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                }
            }

            async Task Check(string op)
            {
                if (!costomerModel.ContainsKey(chatId))
                {
                    costomerModel.Add(chatId, new CostomerModel() { Service = op });
                }
                else
                {
                    costomerModel[chatId].Service = op;
                }

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                    },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                                        }
                });
            }

            async Task AddCellCurrency(string currency)
            {
                if (ifNotBankingTaken[chatId] == true)
                {
                    if (currency == currencies[0])
                    {
                        if (!costomerModel.ContainsKey(chatId))
                        {
                            costomerModel.Add(chatId, new CostomerModel() { CurrencyCell = currency });
                        }
                        else
                        {
                            costomerModel[chatId].CurrencyCell = currency;
                        }

                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(currencies[7], currencies[7].ToLower() + "8"),
                                InlineKeyboardButton.WithCallbackData(currencies[8], currencies[8].ToLower() + "9")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(currencies[17], currencies[17].ToLower() + "18")
                            }
                        });

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Віддаєте: *{currency}*. Оберіть, що *отримаєте* ⬅️:",
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                    }
                    else if (currency == currencies[1] || currency == currencies[2] || currency == currencies[3])
                    {
                        if (!costomerModel.ContainsKey(chatId))
                        {
                            costomerModel.Add(chatId, new CostomerModel() { CurrencyCell = currency });
                        }
                        else
                        {
                            costomerModel[chatId].CurrencyCell = currency;
                        }

                        await AddGetCurrency(currencies[0]);
                    }
                    else if (currency == currencies[4])
                    {
                        if (!costomerModel.ContainsKey(chatId))
                        {
                            costomerModel.Add(chatId, new CostomerModel() { CurrencyCell = currency });
                        }
                        else
                        {
                            costomerModel[chatId].CurrencyCell = currency;
                        }


                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(currencies[0], currencies[0].ToLower() + "1")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(currencies[5], currencies[5].ToLower() + "6")
                    },
                });

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Будь ласка, оберіть валюту яку отримаєте",
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                    }
                    else if (currency == currencies[7] || currency == currencies[8] || currency == currencies[17])
                    {
                        if (!costomerModel.ContainsKey(chatId))
                        {
                            costomerModel.Add(chatId, new CostomerModel() { CurrencyCell = currency });
                        }
                        else
                        {
                            costomerModel[chatId].CurrencyCell = currency;
                        }

                        await AddGetCurrency(currencies[0]);
                    }
                }
                else
                {
                    if (currency == currencies[0])
                    {
                        if (!costomerModel.ContainsKey(chatId))
                        {
                            costomerModel.Add(chatId, new CostomerModel() { CurrencyCell = currency });
                        }
                        else
                        {
                            costomerModel[chatId].CurrencyCell = currency;
                        }
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(currencies[1], currencies[1].ToLower() + "2"),
                        InlineKeyboardButton.WithCallbackData(currencies[2], currencies[2].ToLower() + "3")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(currencies[3], currencies[3].ToLower() + "4")
                    },
                });

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Віддаєте: *{currency}*. Оберіть, що *отримаєте* ⬅️:",
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                    }
                    else if (currency == currencies[1] || currency == currencies[2] || currency == currencies[3])
                    {
                        if (!costomerModel.ContainsKey(chatId))
                        {
                            costomerModel.Add(chatId, new CostomerModel() { CurrencyCell = currency });
                        }
                        else
                        {
                            costomerModel[chatId].CurrencyCell = currency;
                        }

                        await AddGetCurrency(currencies[0]);
                    }
                    else if (currency == currencies[4])
                    {
                        if (!costomerModel.ContainsKey(chatId))
                        {
                            costomerModel.Add(chatId, new CostomerModel() { CurrencyCell = currency });
                        }
                        else
                        {
                            costomerModel[chatId].CurrencyCell = currency;
                        }


                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(currencies[0], currencies[0].ToLower() + "1")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(currencies[5], currencies[5].ToLower() + "6")
                    },
                });

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Будь ласка, оберіть валюту яку отримаєте",
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                    }
                    else if (currency == currencies[7] || currency == currencies[8])
                    {
                        if (!costomerModel.ContainsKey(chatId))
                        {
                            costomerModel.Add(chatId, new CostomerModel() { CurrencyCell = currency });
                        }
                        else
                        {
                            costomerModel[chatId].CurrencyCell = currency;
                        }

                        await AddGetCurrency(currencies[0]);
                    }
                }

            }
            async Task AddGetCurrency(string currency)
            {
                costomerModel[chatId].CurrencyGet = currency;

                if (costomerModel[chatId].CurrencyCell == currencies[0])
                {
                    if (costomerModel[chatId].CurrencyGet == currencies[1])
                    {
                        ProcessOrder(botClient, chatId, cancellationToken, 1);
                    }
                    else if (costomerModel[chatId].CurrencyGet == currencies[2])
                    {
                        ProcessOrder(botClient, chatId, cancellationToken, 2);
                    }
                    else if (costomerModel[chatId].CurrencyGet == currencies[3])
                    {
                        ProcessOrder(botClient, chatId, cancellationToken, 3);
                    }
                    else if (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17])
                    {
                        ProcessGetValue(botClient, chatId, cancellationToken, 8);
                    }
                }
                else if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3])
                {
                    ProcessOrder(botClient, chatId, cancellationToken, 0);
                }
                else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])
                {
                    ProcessGetValue(botClient, chatId, cancellationToken, 8);
                }

                return;
            }
        }
        catch (Exception ex)
        {
            var chatId = query.Message.Chat.Id;;
            await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❗ Некоректна форма введення ❗️",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            Console.WriteLine(ex.Message);
            return;
        }
    }


    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
    static async Task ProcessOrder(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, int bank)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Так, через ордер", "YesOrder"),
                            InlineKeyboardButton.WithCallbackData("Ні, без ордера", "NoOrder"),
                        }
                    });

        if (costomerModel[chatId].CurrencyCell == currencies[0])
        {
            switch (bank)
            {
                case 1:
                    costomerModel[chatId].Course = await binanceService.CountLeftProcentPriceAsync(@"../monoLeftBuyRequest.json", @"../monoLeftSellRequest.json", MonoPercentage);
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{costomerModel[chatId].Course}*",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❗️Чи бажаєте ви здійснити дану угоду через *ордер на P2P-платформі Binance* (виступає як гарант угоди)?",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown);

                    break;
                case 2:
                    costomerModel[chatId].Course = await binanceService.CountLeftProcentPriceAsync(@"../pryvatLeftBuyRequest.json", @"../pryvatLeftSellRequest.json", PryvatPercentage);
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{costomerModel[chatId].Course}*",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❗️Чи бажаєте ви здійснити дану угоду через *ордер на P2P-платформі Binance* (виступає як гарант угоди)?",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown);

                    break;
                case 3:
                    costomerModel[chatId].Course = await binanceService.CountLeftProcentPriceAsync(@"../monoLeftBuyRequest.json", @"../monoLeftSellRequest.json", InshePercentage);
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{costomerModel[chatId].Course}*",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❗️Чи бажаєте ви здійснити дану угоду через *ордер на P2P-платформі Binance* (виступає як гарант угоди)?",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown);

                    break;
                case 8:

                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Введіть суму *Tether, USDT*, яку віддаєте ➡️ (ліміт: *500 USDT* - *100000 USDT*):",
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                    break;
            }
        }
        else if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3])
        {
            if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3])
            {
                if (costomerModel[chatId].CurrencyCell == currencies[1])
                {
                    costomerModel[chatId].Course = await binanceService.CountRightProcentPriceAsync(@"../monoRightBuyRequest.json", @"../monoRightSellRequest.json", MonoPercentage);
                }
                else
                {
                    costomerModel[chatId].Course = await binanceService.CountRightProcentPriceAsync(@"../monoRightBuyRequest.json", @"../monoRightSellRequest.json", InshePercentage);
                }
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{costomerModel[chatId].Course}*",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❗️Чи бажаєте ви здійснити дану угоду через *ордер на P2P-платформі Binance* (виступає як гарант угоди)?",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown);

            }
            else if (costomerModel[chatId].CurrencyCell == currencies[2])
            {
                costomerModel[chatId].Course = await binanceService.CountRightProcentPriceAsync(@"../pryvatRightBuyRequest.json", @"../pryvatRightSellRequest.json", PryvatPercentage);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{costomerModel[chatId].Course}*",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );

                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❗️Чи бажаєте ви здійснити дану угоду через *ордер на P2P-платформі Binance* (виступає як гарант угоди)?",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown);
            }
        }
    }
    static async Task ProcessGetValue(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, int bank)
    {
        if (costomerModel[chatId].CurrencyCell == currencies[0])
        {
            switch (bank)
            {
                case 1:

                    if (!ifCheckNumber.ContainsKey(chatId))
                    {
                        ifCheckNumber.Add(chatId, true);
                    }
                    else
                    {
                        ifCheckNumber[chatId] = true;
                    }

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Введіть <b>номер карти</b> 💳, куди бажаєте отримати кошти (16 цифр). <i>Наприклад: 1111333311113333.</i>",
                    parseMode: ParseMode.Html);
                    break;
                case 2:

                    if (!ifCheckNumber.ContainsKey(chatId))
                    {
                        ifCheckNumber.Add(chatId, true);
                    }
                    else
                    {
                        ifCheckNumber[chatId] = true;
                    }

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Введіть <b>номер карти</b> 💳, куди бажаєте отримати кошти (16 цифр). <i>Наприклад: 1111333311113333.</i>",
                    parseMode: ParseMode.Html);
                    break;
                case 3:

                    if (!ifCheckNumber.ContainsKey(chatId))
                    {
                        ifCheckNumber.Add(chatId, true);
                    }
                    else
                    {
                        ifCheckNumber[chatId] = true;
                    }

                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Введіть <b>номер карти</b> 💳, куди бажаєте отримати кошти (16 цифр). <i>Наприклад: 1111333311113333.</i>",
                    parseMode: ParseMode.Html);
                    break;
                case 8:
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки отримаю ⬅️", "howManyGet"),
                                }
                            });

                    await botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: $"Введіть суму *Tether, USDT*, яку віддаєте ➡️ (ліміт: *500 USDT* - *100000 USDT*):",
                       replyMarkup: inlineKeyboard,
                       parseMode: ParseMode.Markdown,
                       cancellationToken: cancellationToken
                   );
                    break;
            }
        }
        else
        {
            ifTRC20Taken[chatId] = true;

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Введіть <b>адресу гаманця TRC20</b> 💸, куди бажаєте отримати кошти. <i>Наприклад: TNsQfs521...</i>",
                parseMode: ParseMode.Html
            );
        }

        async Task printExeptionValue1(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {

            await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: $"Мінімальна сума = 100 одиниць\nМаксимальна сума = 20000 одиниць",
               parseMode: ParseMode.Markdown,
               cancellationToken: cancellationToken
           );
            return;
        }

        async Task printExeptionValue2(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {

            await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: $"Мінімальна сума = 4000 гривень\nМаксимальна сума = 900000 гривень",
               parseMode: ParseMode.Markdown,
               cancellationToken: cancellationToken
           );
            return;
        }
    }

    static async Task ProccesHowManyGive(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        if (costomerModel.ContainsKey(chatId))
        {
            decimal min = 0;
            decimal max = 0;
            if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[2] || costomerModel[chatId].CurrencyGet == currencies[3]))
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки отримаю ⬅️", "howManyGet"),
                }
            });

                min = 100;
                max = 20000;
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"Введіть суму *Tether, USDT*, яку віддаєте ➡️ (ліміт: *{min} USDT* - *{max} USDT*):",
                   replyMarkup: inlineKeyboard,
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3]))
            {
                decimal course = costomerModel[chatId].Course;

                min = 100 * course;
                max = 20000 * course;

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки отримаю ⬅️", "howManyGet"),
                }
            });

                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"Введіть суму *{costomerModel[chatId].CurrencyCell}*, яку віддаєте ➡️ (ліміт: *{min} UAH* - *{max} UAH*):",
                   replyMarkup: inlineKeyboard,
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17]))
            {
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"Зазначне суму *Tether, USDT*, яку віддаєте ➡️ (ліміт: 500 USDT - 100000 USDT):",
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки отримаю ⬅️", "howManyGet"),
                                }
                            });

                string valute = costomerModel[chatId].CurrencyCell == currencies[7] ? "USD" : "EUR";
                if (costomerModel[chatId].CurrencyCell == currencies[17]) valute = "UAH";
                min = costomerModel[chatId].CurrencyCell == currencies[17] ? 20000 : 500;
                max = costomerModel[chatId].CurrencyCell == currencies[17] ? 4000000 : 100000;

                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"Зазначне суму {costomerModel[chatId].CurrencyCell}, яку віддаєте ➡️ (ліміт: *{min} {valute} - {max} {valute}*):",
                   replyMarkup: inlineKeyboard,
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
            else if (insheService.ContainsKey(chatId) && insheService[chatId] == true)
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Скасувати ❌", "disable"),
                    }
                });

                Random random = new Random();
                int randomNumber = random.Next(101, 1000);
                costomerModel[chatId].Id = randomNumber;

                UserMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"📥 Заявка ID: *{randomNumber}*\n \n💰 Послуга: *{costomerModel[chatId].Service}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами послуги повідомить менеджер після підтвердження.\n \n📲 Контакт: *Ярослав*, @yarius13\n \nСтатус заявки: *Не підтверджена* ⚠️",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
                SendToAdmin(botClient, chatId, cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: $"З вмами скоро зв'яжеться адміністратор",
                   parseMode: ParseMode.Markdown,
                   cancellationToken: cancellationToken
               );
            }
        }
    }

    static async Task ZeroVariables(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        inshe[chatId] = false;
        insheService[chatId] = false;
        ifCheckNumber[chatId] = false;
        HowMuchGet[chatId] = false;
        ifInshaHotivkaTaken[chatId] = false;
        ifNotBankingTaken[chatId] = false;
        ifTRC20Taken[chatId] = false;
        if (costomerModel.ContainsKey(chatId))
        {
            costomerModel[chatId].Order = false;
            costomerModel[chatId].Service = null;
            costomerModel[chatId].CurrencyGet = null;
            costomerModel[chatId].CardNumber = null;
            costomerModel[chatId].CurrencyCell = null;
            costomerModel[chatId].Course = 0;
            costomerModel[chatId].HowMuchGives = 0;
            costomerModel[chatId].HowMuchGet = 0;
            costomerModel[chatId].IfEnd = true;
        }
        else
        {
            costomerModel.Add(chatId, new CostomerModel
            {
                Order = false,
                Service = null,
                CurrencyGet = null,
                CardNumber = null,
                CurrencyCell = null,
                Course = 0,
                HowMuchGives = 0,
                HowMuchGet = 0,
                IfEnd = true
            });
        }
    }
    static async Task SendToAdmin(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        costomerModel[chatId].IfEnd = false;
        if ((costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[2] || costomerModel[chatId].CurrencyGet == currencies[3])) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3])))
        {
            var order = costomerModel[chatId].Order ? "Так" : "Ні";
            AdminMessage[adminChatId] = await botClient.SendTextMessageAsync(
                chatId: adminChatId,
                text: $"*Заявка банкінг:*\n \nId: {costomerModel[chatId].Id}\nКлієнт: {costomerModel[chatId].FirstName} {costomerModel[chatId].LastName} @{costomerModel[chatId].Username}\nНомер телефону: {costomerModel[chatId].Phone}\nВіддає: {costomerModel[chatId].CurrencyCell}\nОтримує: {costomerModel[chatId].CurrencyGet}\nСкільки віддає: {costomerModel[chatId].HowMuchGives}\nСкільки отримує: {costomerModel[chatId].HowMuchGet}\nКурс *1:{costomerModel[chatId].Course}*\nРеквізити: {costomerModel[chatId].CardNumber}\nЧерез ордер: {order}\nПідтверджена: Ні",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else if ((costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17])) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])))
        {
            AdminMessage[adminChatId] = await botClient.SendTextMessageAsync(
                chatId: adminChatId,
                text: $"*Заявка готівка:*\n \nId: {costomerModel[chatId].Id}\nКлієнт: {costomerModel[chatId].FirstName} {costomerModel[chatId].LastName} @{costomerModel[chatId].Username}\nНомер телефону: {costomerModel[chatId].Phone}\nВіддає: {costomerModel[chatId].CurrencyCell}\nОтримує: {costomerModel[chatId].CurrencyGet}\nСкільки віддає: {costomerModel[chatId].HowMuchGives}\nСкільки отримує: {costomerModel[chatId].HowMuchGet}\nРеквізити: {costomerModel[chatId].CardNumber}\n \nПідтверджена: Ні",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            AdminMessage[adminChatId] = await botClient.SendTextMessageAsync(
                chatId: adminChatId,
                text: $"*Заявка послуга:*\n \nId: {costomerModel[chatId].Id}\nКлієнт: {costomerModel[chatId].FirstName} {costomerModel[chatId].LastName} @{costomerModel[chatId].Username}\nНомер телефону: {costomerModel[chatId].Phone}\nПослуга: {costomerModel[chatId].Service}\n \nПідтверджена: Ні",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
    }

    static async Task ChangeAgminMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, bool ifAccess)
    {
        if (ifAccess)
        {
            if ((costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[2] || costomerModel[chatId].CurrencyGet == currencies[3])) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3])))
            {
                var order = costomerModel[chatId].Order ? "Так" : "Ні";
                await botClient.EditMessageTextAsync(
                    chatId: adminChatId,
                    messageId: AdminMessage[adminChatId].MessageId,
                    text: $"*Заявка банкінг:*\n \nId: {costomerModel[chatId].Id}\nКлієнт: {costomerModel[chatId].FirstName} {costomerModel[chatId].LastName} @{costomerModel[chatId].Username}\nНомер телефону: {costomerModel[chatId].Phone}\nВіддає: {costomerModel[chatId].CurrencyCell}\nОтримує: {costomerModel[chatId].CurrencyGet}\nСкільки віддає: {costomerModel[chatId].HowMuchGives}\nСкільки отримує: {costomerModel[chatId].HowMuchGet}\nКурс *1:{costomerModel[chatId].Course}*\nРеквізити: {costomerModel[chatId].CardNumber}\nЧерез ордер: {order}\nПідтверджена: *Так*",
                    parseMode: ParseMode.Markdown
                );
            }
            else if ((costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17])) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])))
            {
                await botClient.EditMessageTextAsync(
                    chatId: adminChatId,
                    messageId: AdminMessage[adminChatId].MessageId,
                    text: $"*Заявка готівка:*\n \nId: {costomerModel[chatId].Id}\nКлієнт: {costomerModel[chatId].FirstName} {costomerModel[chatId].LastName} @{costomerModel[chatId].Username}\nНомер телефону: {costomerModel[chatId].Phone}\nВіддає: {costomerModel[chatId].CurrencyCell}\nОтримує: {costomerModel[chatId].CurrencyGet}\nСкільки віддає: {costomerModel[chatId].HowMuchGives}\nСкільки отримує: {costomerModel[chatId].HowMuchGet}\nРеквізити: {costomerModel[chatId].CardNumber}\n \nПідтверджена: *Так*",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.EditMessageTextAsync(
                    chatId: adminChatId,
                    messageId: AdminMessage[adminChatId].MessageId,
                    text: $"*Заявка послуга:*\n \nId: {costomerModel[chatId].Id}\nКлієнт: {costomerModel[chatId].FirstName} {costomerModel[chatId].LastName} @{costomerModel[chatId].Username}\nНомер телефону: {costomerModel[chatId].Phone}\nПослуга: {costomerModel[chatId].Service}\n \nПідтверджена: *Так*",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
        }
        else
        {
            if ((costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[2] || costomerModel[chatId].CurrencyGet == currencies[3])) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3])))
            {
                var order = costomerModel[chatId].Order ? "Так" : "Ні";
                await botClient.EditMessageTextAsync(
                    chatId: adminChatId,
                    messageId: AdminMessage[adminChatId].MessageId,
                    text: $"*Заявка банкінг:*\n \nId: {costomerModel[chatId].Id}\nКлієнт: {costomerModel[chatId].FirstName} {costomerModel[chatId].LastName} @{costomerModel[chatId].Username}\nНомер телефону: {costomerModel[chatId].Phone}\nВіддає: {costomerModel[chatId].CurrencyCell}\nОтримує: {costomerModel[chatId].CurrencyGet}\nСкільки віддає: {costomerModel[chatId].HowMuchGives}\nСкільки отримує: {costomerModel[chatId].HowMuchGet}\nКурс *1:{costomerModel[chatId].Course}*\nРеквізити: {costomerModel[chatId].CardNumber}\nЧерез ордер: {order}\n*Скасована* ❌",
                    parseMode: ParseMode.Markdown
                );
            }
            else if ((costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17])) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])))
            {
                await botClient.EditMessageTextAsync(
                    chatId: adminChatId,
                    messageId: AdminMessage[adminChatId].MessageId,
                    text: $"*Заявка готівка:*\n \nId: {costomerModel[chatId].Id}\nКлієнт: {costomerModel[chatId].FirstName} {costomerModel[chatId].LastName} @{costomerModel[chatId].Username}\nНомер телефону: {costomerModel[chatId].Phone}\nВіддає: {costomerModel[chatId].CurrencyCell}\nОтримує: {costomerModel[chatId].CurrencyGet}\nСкільки віддає: {costomerModel[chatId].HowMuchGives}\nСкільки отримує: {costomerModel[chatId].HowMuchGet}\nРеквізити: {costomerModel[chatId].CardNumber}\n \n*Скасована* ❌",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.EditMessageTextAsync(
                    chatId: adminChatId,
                    messageId: AdminMessage[adminChatId].MessageId,
                    text: $"*Заявка послуга:*\n \nId: {costomerModel[chatId].Id}\nКлієнт: {costomerModel[chatId].FirstName} {costomerModel[chatId].LastName} @{costomerModel[chatId].Username}\nНомер телефону: {costomerModel[chatId].Phone}\nПослуга: {costomerModel[chatId].Service}\n \n*Скасована* ❌",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
        }
    }
    static async Task ChangeUserMessage(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, bool ifAccess)
    {
        if (ifAccess)
        {
            var order = costomerModel[chatId].Order ? "Так, через ордер" : "Ні, без ордера";
            if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[2] || costomerModel[chatId].CurrencyGet == currencies[3]) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3])))
            {
                var sendValute = costomerModel[chatId].CurrencyCell == currencies[0] ? "USDT" : "UAH";
                var getCurr = costomerModel[chatId].CurrencyGet == currencies[0] ? "USDT" : "UAH";
                var card = costomerModel[chatId].Order ? " " : $"\n💳 Номер карти: *{costomerModel[chatId].CardNumber}*";
                if (costomerModel[chatId].CurrencyGet == currencies[0])
                {
                    card = costomerModel[chatId].Order ? " " : $"\n💸 Адреса гаманця TRC20: *{costomerModel[chatId].CardNumber}*";
                }
                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: UserMessage[chatId].MessageId,
                    text: $"📥 Заявка ID: *{costomerModel[chatId].Id}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{costomerModel[chatId].Course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} {sendValute}*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} {getCurr}*\n \n🔐 P2P-ордер: *{order}* {card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}\n \nСтатус заявки: *Підтверджена* ✅",
                    parseMode: ParseMode.Markdown
                );
            }
            else if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17]) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])))
            {
                string valute = "";
                var getOrGive = HowMuchGet[chatId] == true ? "💰 Сума, яку отримаєте:" : "💰Сума, яку віддаєте:";
                if (HowMuchGet[chatId] == true)
                {
                    if (costomerModel[chatId].CurrencyCell == currencies[0])
                    {
                        valute = costomerModel[chatId].CurrencyGet == currencies[7] ? "USD" : "EUR";
                        if (costomerModel[chatId].CurrencyGet == currencies[17]) valute = "UAH";

                    }
                    else
                    {
                        valute = "USDT";
                    }
                }
                else
                {
                    if (costomerModel[chatId].CurrencyCell == currencies[0])
                    {
                        valute = "USDT";
                    }
                    else
                    {
                        valute = costomerModel[chatId].CurrencyCell == currencies[7] ? "USD" : "EUR";
                        if (costomerModel[chatId].CurrencyCell == currencies[17]) valute = "UAH";
                    }
                }


                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: UserMessage[chatId].MessageId,
                    text: $"📥 Заявка ID: *{costomerModel[chatId].Id}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами валюти повідомить менеджер після підтвердження.\n \n{getOrGive} *{(HowMuchGet[chatId] == true ? costomerModel[chatId].HowMuchGet : costomerModel[chatId].HowMuchGives)} {valute}*\n \n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}\n \nСтатус заявки: *Підтверджена* ✅",
                    parseMode: ParseMode.Markdown
                );
            }
            else if (costomerModel[chatId].Service != null)
            {
                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: UserMessage[chatId].MessageId,
                    text: $"📥 Заявка ID: *{costomerModel[chatId].Id}*\n \n💰 Послуга: *{costomerModel[chatId].Service}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами послуги повідомить менеджер після підтвердження.\n \n📲 Контакт: *Ярослав*, @yarius13\n \nСтатус заявки: *Підтверджена* ✅",
                    parseMode: ParseMode.Markdown
                );
            }
        }
        else
        {
            var order = costomerModel[chatId].Order ? "Так, через ордер" : "Ні, без ордера";
            if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[2] || costomerModel[chatId].CurrencyGet == currencies[3]) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3])))
            {
                var sendValute = costomerModel[chatId].CurrencyCell == currencies[0] ? "USDT" : "UAH";
                var getCurr = costomerModel[chatId].CurrencyGet == currencies[0] ? "USDT" : "UAH";
                var card = costomerModel[chatId].Order ? " " : $"\n💳 Номер карти: *{costomerModel[chatId].CardNumber}*";
                if (costomerModel[chatId].CurrencyGet == currencies[0])
                {
                    card = costomerModel[chatId].Order ? " " : $"\n💸 Адреса гаманця TRC20: *{costomerModel[chatId].CardNumber}*";
                }
                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: UserMessage[chatId].MessageId,
                    text: $"📥 Заявка ID: *{costomerModel[chatId].Id}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{costomerModel[chatId].Course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} {sendValute}*\n💰 Сума, яку отримаєте: *{costomerModel[chatId].HowMuchGet} {getCurr}*\n \n🔐 P2P-ордер: *{order}* {card}\n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}\n \nСтатус заявки: *Скасована* ❌",
                    parseMode: ParseMode.Markdown
                );
            }
            else if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8] || costomerModel[chatId].CurrencyGet == currencies[17]) || (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8] || costomerModel[chatId].CurrencyCell == currencies[17])))
            {
                string valute = "";
                var getOrGive = HowMuchGet[chatId] == true ? "💰 Сума, яку отримаєте:" : "💰Сума, яку віддаєте:";
                if (HowMuchGet[chatId] == true)
                {
                    if (costomerModel[chatId].CurrencyCell == currencies[0])
                    {
                        valute = costomerModel[chatId].CurrencyGet == currencies[7] ? "USD" : "EUR";
                        if (costomerModel[chatId].CurrencyGet == currencies[17]) valute = "UAH";

                    }
                    else
                    {
                        valute = "USDT";
                    }
                }
                else
                {
                    if (costomerModel[chatId].CurrencyCell == currencies[0])
                    {
                        valute = "USDT";
                    }
                    else
                    {
                        valute = costomerModel[chatId].CurrencyCell == currencies[7] ? "USD" : "EUR";
                        if (costomerModel[chatId].CurrencyCell == currencies[17]) valute = "UAH";
                    }
                }


                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: UserMessage[chatId].MessageId,
                    text: $"📥 Заявка ID: *{costomerModel[chatId].Id}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами валюти повідомить менеджер після підтвердження.\n \n{getOrGive} *{(HowMuchGet[chatId] == true ? costomerModel[chatId].HowMuchGet : costomerModel[chatId].HowMuchGives)} {valute}*\n \n📲 Контакт: *{costomerModel[chatId].FirstName}*, @{costomerModel[chatId].Username}\n \nСтатус заявки: *Скасована* ❌",
                    parseMode: ParseMode.Markdown
                );
            }
            else if (costomerModel[chatId].Service != null)
            {
                await botClient.EditMessageTextAsync(
                    chatId: chatId,
                    messageId: UserMessage[chatId].MessageId,
                    text: $"📥 Заявка ID: *{costomerModel[chatId].Id}*\n \n💰 Послуга: *{costomerModel[chatId].Service}*\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами послуги повідомить менеджер після підтвердження.\n \n📲 Контакт: *Ярослав*, @yarius13\n \nСтатус заявки: *Скасована* ❌",
                    parseMode: ParseMode.Markdown
                );
            }
        }
    }
}