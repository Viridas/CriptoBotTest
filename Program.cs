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
    private static List<string> currencies = new List<string>();
    private static Dictionary<long, CostomerModel> costomerModel = new Dictionary<long, CostomerModel>();
    private static Dictionary<long, bool> HowMuchGet = new Dictionary<long, bool>();
    private static long adminChatId;
    private static HttpClient httpClient = new HttpClient();
    private static BinanceAPIService binanceService = new BinanceAPIService(httpClient);
    private static AnotherCryptoService anotherCryptoService = new AnotherCryptoService(httpClient);
    private static Dictionary<long, bool> ifInshaHotivkaTaken = new Dictionary<long, bool>();
    private static Dictionary<long, bool> ifNotBankingTaken = new Dictionary<long, bool>();
    private static Dictionary<long, bool> ifCheckNumber = new Dictionary<long, bool>();
    private static Dictionary<long, bool> inshe = new Dictionary<long, bool>();
    private static Dictionary<long, bool> ifTRC20Taken = new Dictionary<long, bool>();
    static async Task Main(string[] args)
    {
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

    static async Task OnAddNumber(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var chatId = update.Message.Chat.Id;
        costomerModel[chatId].Phone = update.Message.Contact.PhoneNumber;
        costomerModel[chatId].FirstName = update.Message.Contact.FirstName;
        costomerModel[chatId].LastName = update.Message.Contact.LastName;

        var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new[]
                    {
                        new KeyboardButton("Нова заявка"),
                        new KeyboardButton("Про нас, умови та графік роботи")
                    },
                    new[]
                    {
                        new KeyboardButton("Ваші відгуки"),
                        new KeyboardButton("Наша спільнота")
                    }
                })
        {
            OneTimeKeyboard = false,
            ResizeKeyboard = true
        };

        lastMessage[chatId] = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Контакт отримано ✅ Тепер ми на зв'язку.",
                replyMarkup: keyboard,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );

        int min = 0;
        int max = 0;
        if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[2] || costomerModel[chatId].CurrencyGet == currencies[3]))
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки я отримаю", "howManyGet"),
                }
            });

            min = 100;
            max = 20000;
            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Введіть суму Tether, USDT, яку віддаєте ➡️ (ліміт: {min} USDT - {max} USDT):",
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else if (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3]))
        {
            min = 4000;
            max = 900000;

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки я отримаю", "howManyGet"),
                }
            });

            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Введіть скільки ви віддаєте. Мінімум: {min}, Максимум: {max}",
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8]))
        {
            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Зазначне суму USDT, яку віддаєте",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8])
        {
            if (costomerModel[chatId].CurrencyCell == currencies[7])
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки я отримаю", "howManyGet"),
                                }
                            });

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Зазначне суму {currencies[7]}, яку віддаєте",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (costomerModel[chatId].CurrencyCell == currencies[8])
            {
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки я отримаю", "howManyGet"),
                                }
                            });

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Зазначне суму {currencies[8]}, яку віддаєте",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
        }
        else if (inshe.ContainsKey(chatId) && inshe[chatId] == true)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                }
            });

            Random random = new Random();
            int randomNumber = random.Next(1, 1001);

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"📥 Заявка ID: *{randomNumber}*\n \n💰 Послуга: {costomerModel[chatId].Service}\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами послуги повідомить менеджер після підтвердження.\n \n📲 Контакт: Ярослав, @yarius13\n \nСтатус заявки: Не підтверджена ⚠️",
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"З вмами скоро зв'яжеться адміністратор",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }


        // lastMessage[chatId] = await botClient.SendTextMessageAsync(
        //     chatId: adminChatId,
        //     text: $"Нова заявка!\n Ім'я: {costomerModel[chatId].FirstName}\n Фамілія: {costomerModel[chatId].LastName}\n Номер телефону: {costomerModel[chatId].Phone}\n Яку валюту віддає: {costomerModel[chatId].CurrencyCell} \n Скільки віддає: {costomerModel[chatId].HowMuchGives} \n Яку валюту отримує: {costomerModel[chatId].CurrencyGet} \n Номер карти: {costomerModel[chatId].CardNumber}",
        //     parseMode: ParseMode.Markdown,
        //     cancellationToken: cancellationToken
        // );
    }
    static async Task OnMessage(ITelegramBotClient botClient, Message msg, CancellationToken cancellationToken)
    {
        var chatId = msg.Chat.Id;

        try
        {
            if (msg.Text == "/start")
            {
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

                if (!costomerModel.ContainsKey(chatId))
                {
                    costomerModel.Add(chatId, new CostomerModel() { Username = msg.From.Username });
                }
                else
                {
                    costomerModel[chatId].Username = msg.From.Username;
                }

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Вітаю!",
                    replyMarkup: keyboard,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (msg.Text == "Нова заявка 📥")
            {
                inshe[chatId] = false;
                ifCheckNumber[chatId] = false;
                HowMuchGet[chatId] = false;
                ifInshaHotivkaTaken[chatId] = false;
                ifNotBankingTaken[chatId] = false;
                ifTRC20Taken[chatId] = false;
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
                });

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
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

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
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

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
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
                        InlineKeyboardButton.WithUrl("Приєднатись↗️", "https://t.me/reviews_13exchanger"),
                    }
                });

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Наша спільнота 📣",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (decimal.TryParse(msg.Text, out decimal count) && count.ToString().Length != 16)
            {
                if (costomerModel.ContainsKey(chatId))
                {
                    if (HowMuchGet.ContainsKey(chatId))
                    {
                        if (HowMuchGet[chatId] == true)
                        {
                            if (costomerModel[chatId].CurrencyCell == currencies[0])
                            {
                                if (count < 100 || count > 20000)
                                {
                                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "Введіть суму відповідно до визначених лімітів (min. 100, max. 20000)❗️",
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                    return;
                                }
                                if (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[3])
                                {
                                    costomerModel[chatId].HowMuchGives = Math.Round(count / await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoLeftSellRequest.json"), 2);
                                    costomerModel[chatId].HowMuchGet = count;

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                    }
                                });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nСума переказу: {costomerModel[chatId].HowMuchGives} одиниць\nОтримаєте: {costomerModel[chatId].HowMuchGet} гривень",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                                else if (costomerModel[chatId].CurrencyGet == currencies[2])
                                {
                                    costomerModel[chatId].HowMuchGives = Math.Round(count / await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\pryvatLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\pryvatLeftSellRequest.json"), 2);
                                    costomerModel[chatId].HowMuchGet = count;

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                        }
                                    });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nСума переказу: {costomerModel[chatId].HowMuchGives} одиниць\nОтримаєте: {costomerModel[chatId].HowMuchGet} гривень",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                                else if (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8])
                                {
                                    costomerModel[chatId].HowMuchGet = count;

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                        }
                                    });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nОтримаєте: {costomerModel[chatId].HowMuchGet} {costomerModel[chatId].CurrencyGet}",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                            }
                            else if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3] || costomerModel[chatId].CurrencyCell == currencies[2])
                            {
                                if (count < 100 || count > 20000)
                                {
                                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "Введіть суму відповідно до визначених лімітів (min. 100, max. 20000)❗️",
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                    return;
                                }
                                if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3])
                                {
                                    costomerModel[chatId].HowMuchGives = count * await binanceService.CountRightProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoRightBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoRightSellRequest.json");
                                    costomerModel[chatId].HowMuchGet = count;

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                        }
                                    });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nСума переказу: {costomerModel[chatId].HowMuchGives} гривень\nОтримаєте: {costomerModel[chatId].HowMuchGet} одиниць",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                                else if (costomerModel[chatId].CurrencyCell == currencies[2])
                                {
                                    costomerModel[chatId].HowMuchGives = count * await binanceService.CountRightProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\pryvatRightBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\pryvatRightSellRequest.json");
                                    costomerModel[chatId].HowMuchGet = count;

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                        }
                                    });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nСума переказу: {costomerModel[chatId].HowMuchGives} гривень\nОтримаєте: {costomerModel[chatId].HowMuchGet} одиниць",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                            }
                            else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8])
                            {
                                costomerModel[chatId].HowMuchGet = count;

                                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                        }
                                    });

                                Random random = new Random();
                                int randomNumber = random.Next(1, 1001);

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nОтримаєте: {costomerModel[chatId].HowMuchGet} одиниць",
                                    replyMarkup: inlineKeyboard,
                                    parseMode: ParseMode.Markdown,
                                    cancellationToken: cancellationToken
                                );
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
                                        lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: "Введіть суму відповідно до визначених лімітів (min. 100, max. 20000)❗️",
                                            parseMode: ParseMode.Markdown,
                                            cancellationToken: cancellationToken
                                        );
                                        return;
                                    }

                                    var course = await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoLeftSellRequest.json");
                                    costomerModel[chatId].HowMuchGives = count;
                                    costomerModel[chatId].HowMuchGet = count * course;

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                                    }
                                });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);
                                    var getCurr = costomerModel[chatId].CurrencyGet == currencies[1] ? "UAH" : "";
                                    var order = costomerModel[chatId].Order == true ? "Так, через ордер" : "Ні, без ордера";
                                    var confirm = costomerModel[chatId].Confirm == true ? "Заявку підтверджено ✅" : "Не підтверджена ⚠️";

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"📥 Заявка ID:: *{randomNumber}*\n \n➡️ Віддаєте: *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: *1:{course}*\n \n💸 Сума, яку потрібно надіслати: *{costomerModel[chatId].HowMuchGives} USDT*\n💰 Сума, яку отримаєте: {costomerModel[chatId].HowMuchGet} {getCurr}\n \n🔐 P2P-ордер: {order}\n💳 Номер карти: {costomerModel[chatId].CardNumber}\n📲 Контакт: {costomerModel[chatId].FirstName} @{costomerModel[chatId].Username}",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                                else if (costomerModel[chatId].CurrencyGet == currencies[2])
                                {
                                    if (count < 100 || count > 20000)
                                    {
                                        lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: "Введіть суму відповідно до визначених лімітів (min. 100, max. 20000)❗️",
                                            parseMode: ParseMode.Markdown,
                                            cancellationToken: cancellationToken
                                        );
                                        return;
                                    }

                                    costomerModel[chatId].HowMuchGives = count;
                                    costomerModel[chatId].HowMuchGet = count * await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\pryvatLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\pryvatLeftSellRequest.json");

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                    }
                                });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nСума переказу: {costomerModel[chatId].HowMuchGives} одиниць\nОтримаєте: {costomerModel[chatId].HowMuchGet} гривень",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                                else if (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8])
                                {
                                    costomerModel[chatId].HowMuchGives = count;

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                        }
                                    });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nСума переказу: {costomerModel[chatId].HowMuchGives} одиниць",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                            }
                            else if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3] || costomerModel[chatId].CurrencyCell == currencies[2])
                            {
                                if (count < 4000 || count > 900000)
                                {
                                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "Введіть суму відповідно до визначених лімітів (min. 4000, max. 900000)❗️",
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                    return;
                                }
                                if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3])
                                {
                                    costomerModel[chatId].HowMuchGives = count;
                                    costomerModel[chatId].HowMuchGet = Math.Round(count / await binanceService.CountRightProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoRightBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoRightSellRequest.json"), 2);

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                        }
                                    });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nСума переказу: {costomerModel[chatId].HowMuchGives} гривень\nОтримаєте: {costomerModel[chatId].HowMuchGet} одиниць",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                                else if (costomerModel[chatId].CurrencyCell == currencies[2])
                                {
                                    costomerModel[chatId].HowMuchGives = count;
                                    costomerModel[chatId].HowMuchGet = Math.Round(count / await binanceService.CountRightProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\pryvatRightBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\pryvatRightSellRequest.json"), 2);

                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                        }
                                    });

                                    Random random = new Random();
                                    int randomNumber = random.Next(1, 1001);

                                    await botClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nСума переказу: {costomerModel[chatId].HowMuchGives} гривень\nОтримаєте: {costomerModel[chatId].HowMuchGet} одиниць",
                                        replyMarkup: inlineKeyboard,
                                        parseMode: ParseMode.Markdown,
                                        cancellationToken: cancellationToken
                                    );
                                }
                            }
                            else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8])
                            {
                                costomerModel[chatId].HowMuchGives = count;

                                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                        {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
                                        }
                                    });

                                Random random = new Random();
                                int randomNumber = random.Next(1, 1001);

                                await botClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: $"Заявка на обмін ID: {randomNumber}\nВідправляєте: {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nСума переказу: {costomerModel[chatId].HowMuchGives} {costomerModel[chatId].CurrencyCell}",
                                    replyMarkup: inlineKeyboard,
                                    parseMode: ParseMode.Markdown,
                                    cancellationToken: cancellationToken
                                );
                            }
                        }
                    }
                }
            }
            else if (long.TryParse(msg.Text, out long cardNumber) && cardNumber.ToString().Length == 16 && ifCheckNumber.ContainsKey(chatId))
            {
                if (!ifCheckNumber[chatId])
                {
                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❗ Некоректна форма введення ❗️",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                    );

                    return;
                }
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
                            text: "Надішліть ваш контакт Telegram, щоб менеджер👨🏻‍💻 міг з вами зв'язатись.",
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
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Введіть пароль",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (msg.Text == "/vsbhupw383e2asnx390g")
            {
                adminChatId = chatId;


                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Цей чат тепер для адмінів",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (ifInshaHotivkaTaken.ContainsKey(chatId) && ifTRC20Taken.ContainsKey(chatId))
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
                            KeyboardButton.WithRequestContact("Надіслати контакт")
                        }
                    })
                        {
                            OneTimeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Натисніть кнопку, щоб надіслати свої контактні дані",
                            replyMarkup: keyboard);
                    }
                    else
                    {
                        lastMessage[chatId] = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Дякуємо за довіру, очікуйте декілька хвилин з вами зв'яжеться менеджер для здійснення угоди.",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );

                        // await botClient.SendTextMessageAsync(
                        //     chatId: adminChatId,
                        //     text: $"Нова заявка!\n Ім'я: {costomerModel[chatId].FirstName}\n Фамілія: {costomerModel[chatId].LastName}\n Номер телефону: {costomerModel[chatId].Phone}\n Яку валюту віддає: {costomerModel[chatId].CurrencyCell} \n Скільки віддає: {costomerModel[chatId].HowMuchGives} \n Яку валюту отримує: {costomerModel[chatId].CurrencyGet} \n Номер карти: {costomerModel[chatId].CardNumber}",
                        //     parseMode: ParseMode.Markdown,
                        //     cancellationToken: cancellationToken
                        // );
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
                            text: "Адреса повинна складатись лише з латинських букв(a-Z) і цифр(0-9). Максимум 100 знаків"
                        );
                        return;
                    }
                    if (msg.Text.Length > 100)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Максимум 100 знаків"
                        );
                        return;
                    }

                    costomerModel[chatId].CardNumber = msg.Text;

                    if (costomerModel[chatId].Phone == null)
                    {
                        var keyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new[]
                            {
                            KeyboardButton.WithRequestContact("Надіслати контакт")
                            }
                        })
                        {
                            OneTimeKeyboard = true
                        };

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Натисніть кнопку, щоб надіслати свої контактні дані",
                            replyMarkup: keyboard);
                    }
                    else
                    {
                        int min = 0;
                        int max = 0;
                        if (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3]))
                        {
                            min = 4000;
                            max = 900000;

                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки я отримаю", "howManyGet"),
                                }
                            });

                            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Введіть скільки ви віддаєте. Мінімум: {min}, Максимум: {max}",
                                replyMarkup: inlineKeyboard,
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8])
                        {
                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки я отримаю", "howManyGet"),
                                }
                            });

                            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Введіть скільки ви віддаєте.",
                                replyMarkup: inlineKeyboard,
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                }
            }

            else
            {
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "❗ Некоректна форма введення ❗️",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (ApiRequestException apiEx) when (apiEx.ErrorCode == 403)
        {
            return;
        }
    }

    static async Task OnCallbackQuery(ITelegramBotClient botClient, CallbackQuery query, CancellationToken cancellationToken)
    {
        var chatId = query.Message.Chat.Id;
        if (query.Data != null)
        {
            if (currencies.Any(x => x.ToLower() == query.Data || currencies.Any(x => x.ToLower() == query.Data.Substring(0, query.Data.Length - 1))))
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

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Будь ласка, оберіть валюту яку віддаєте",
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
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(currencies[7], currencies[7].ToLower()),
                    InlineKeyboardButton.WithCallbackData(currencies[8], currencies[8].ToLower()),
                },
            });

                ifNotBankingTaken[chatId] = true;

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Будь ласка, оберіть валюту яку віддаєте",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (query.Data == "course")
            {
                await botClient.DeleteMessageAsync(
                        chatId: chatId,
                        messageId: lastMessage[chatId].MessageId,
                        cancellationToken: cancellationToken
                    );
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Монобанк продаж: ₴ {await binanceService.CountRightProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoRightBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoRightSellRequest.json")}\nМонобанк купівля: ₴ {await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoLeftSellRequest.json")}\nПриват продаж: ₴ {await binanceService.CountRightProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\pryvatRightBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\pryvatRightSellRequest.json")}\nПриват купівля: ₴ {await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\pryvatLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\pryvatLeftSellRequest.json")}\nBitcoin: {await anotherCryptoService.GetBitcoinPrice()}\nEthereum: {await anotherCryptoService.GetEthereumPrice()}",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
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
                        if (costomerModel[chatId].CurrencyGet == currencies[1] || costomerModel[chatId].CurrencyGet == currencies[3])
                        {
                            var course = await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoLeftSellRequest.json");
                            var min = 100 * course;
                            var max = 20000 * course;
                            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Введіть суму *{costomerModel[chatId].CurrencyGet}*, яку отримаєте ⬅️ (ліміт: {min} UAH - {max} UAH):",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (costomerModel[chatId].CurrencyGet == currencies[2])
                        {
                            var course = await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\pryvatLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\pryvatLeftSellRequest.json");
                            var min = 100 * course;
                            var max = 20000 * course;
                            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Введіть суму *{costomerModel[chatId].CurrencyGet}*, яку отримаєте ⬅️ (ліміт: {min} UAH - {max} UAH):",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                    else if (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3]))
                    {
                        if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3])
                        {
                            var min = 100;
                            var max = 20000;
                            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Введіть скільки ви отримаєте. Мінімум: {min}, Максимум: {max}",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                        else if (costomerModel[chatId].CurrencyCell == currencies[2])
                        {
                            var min = 100;
                            var max = 20000;
                            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Введіть скільки ви отримаєте. Мінімум: {min}, Максимум: {max}",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                    }
                    else if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8]))
                    {
                        lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Введіть скільки ви отримаєте готівки",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                    }
                    else if (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8]))
                    {
                        lastMessage[chatId] = await botClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"Введіть скільки ви отримаєте криптовалюти",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                    }
                }
            }
            else if (query.Data == "howMuchGive")
            {
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

                ifCheckNumber[chatId] = true;

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Інший обмін та послуги з криптовалютами🧾",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken

                );
            }
            else if (query.Data == "w1" || query.Data == "w2" || query.Data == "w3" || query.Data == "w4" || query.Data == "w5" || query.Data == "w6" || query.Data == "w7" || query.Data == "w8")
            {
                if (ifCheckNumber.ContainsKey(chatId))
                {
                    if (!ifCheckNumber[chatId])
                    {
                        lastMessage[chatId] = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "❗ Некоректна форма введення ❗️",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                        );

                        return;
                    }
                }
                if (costomerModel.ContainsKey(chatId))
                {
                    switch (query.Data)
                    {
                        case "w1":
                            Check("Перестановка готівки по світу ($/€)");
                            break;
                        case "w2":
                            Check("Оплата і прийом безготівки ($/€)");
                            break;
                        case "w3":
                            Check("Оплата безготівки на фіз. обличчя");
                            break;
                        case "w4":
                            Check("Оплата будь-яких сум на ФОП");
                            break;
                        case "w5":
                            Check("Оплата юаня на карти фіз. облич");
                            break;
                        case "w6":
                            Check("Обмін з електронних платіжних систем");
                            break;
                        case "w7":
                            Check("Виплата на картки Європи");
                            break;
                        case "w8":
                            Check("Інше");
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

                        inshe[chatId] = true;

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Надішліть ваш контакт Telegram, щоб менеджер👨🏻‍💻 міг з вами зв'язатись.",
                            replyMarkup: keyboard);
                    }
                    else
                    {
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Підтвердити заявку ✅", "accses"),
                            }
                        });

                        Random random = new Random();
                        int randomNumber = random.Next(1, 1001);

                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"📥 Заявка ID: *{randomNumber}*\n \n💰 Послуга: {costomerModel[chatId].Service}\n📈 Актуальні курси на момент створення заявки та детальну інформацію щодо обраної вами послуги повідомить менеджер після підтвердження.\n \n📲 Контакт: Ярослав, @yarius13\n \nСтатус заявки: Не підтверджена ⚠️",
                            replyMarkup: inlineKeyboard,
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                    }
                }
            }
            else if (query.Data == "accses")
            {
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Заявку підтверджено ✅ Через декілька хвилин з вами зв'яжеться менеджер👨🏻‍💻 для здійснення угоди. Дякуємо за довіру ❤️",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.DeleteMessageAsync(
                        chatId: chatId,
                        messageId: lastMessage[chatId].MessageId,
                        cancellationToken: cancellationToken
                    );
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
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
                    InlineKeyboardButton.WithCallbackData("Підтверджую", "accses"),
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
                            }
                        });

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Віддаєте {currency}. Оберіть валюту яку отримаєте",
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

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
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

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Віддаєте {currency}. Оберіть валюту яку отримаєте",
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

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
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
                else if (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8])
                {
                    ProcessGetValue(botClient, chatId, cancellationToken, 8);
                }
            }
            else
            {
                ProcessGetValue(botClient, chatId, cancellationToken, 0);
            }

            // lastMessage[chatId] = await botClient.SendTextMessageAsync(
            //     chatId: chatId,
            //     text: $"Введіть суму яку віддаєте",
            //     parseMode: ParseMode.Markdown,
            //     cancellationToken: cancellationToken
            // );
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
    static async Task ProcessGetValue(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken, int bank)
    {
        if (costomerModel[chatId].CurrencyCell == currencies[0])
        {
            switch (bank)
            {
                case 1:

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"➡️ Віддаєте *{costomerModel[chatId].CurrencyCell}*\n⬅️ Отримуєте: *{costomerModel[chatId].CurrencyGet}*\n📈 Курс: 1 : {await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoLeftSellRequest.json")}",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );

                    if (!ifCheckNumber.ContainsKey(chatId))
                    {
                        ifCheckNumber.Add(chatId, true);
                    }
                    else
                    {
                        ifCheckNumber[chatId] = true;
                    }

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Введіть номер карти 💳, куди бажаєте отримати кошти (16 цифр). Наприклад: <i>1111333311113333</i>",
                    parseMode: ParseMode.Html);
                    break;
                case 2:
                    // if (HowMuchGivesNow[chatId] < 100 || HowMuchGivesNow[chatId] > 20000)
                    // {
                    //     await printExeptionValue1(botClient, chatId, cancellationToken);
                    //     return;
                    // }

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Віддаєте {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nТеперішній курс 1:{await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\pryvatLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\pryvatLeftSellRequest.json")}",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );

                    if (!ifCheckNumber.ContainsKey(chatId))
                    {
                        ifCheckNumber.Add(chatId, true);
                    }
                    else
                    {
                        ifCheckNumber[chatId] = true;
                    }

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Будь ласка, введіть свій номер карти(16 цифр).\nПриклад: 0000 0000 0000 0000");
                    break;
                case 3:
                    // if (HowMuchGivesNow[chatId] < 100 || HowMuchGivesNow[chatId] > 20000)
                    // {
                    //     await printExeptionValue1(botClient, chatId, cancellationToken);
                    //     return;
                    // }

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Віддаєте {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nТеперішній курс 1:{await binanceService.CountLeftProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoLeftBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoLeftSellRequest.json")}",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );

                    if (!ifCheckNumber.ContainsKey(chatId))
                    {
                        ifCheckNumber.Add(chatId, true);
                    }
                    else
                    {
                        ifCheckNumber[chatId] = true;
                    }

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Будь ласка, введіть свій номер карти(16 цифр).\nПриклад: 0000 0000 0000 0000");
                    break;
                case 8:
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки я отримаю", "howManyGet"),
                                }
                            });

                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: $"Введіть скільки ви віддаєте.",
                        replyMarkup: inlineKeyboard,
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                    break;
            }
        }
        else if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3])
        {
            // if (HowMuchGivesNow[chatId] < 4000 || HowMuchGivesNow[chatId] > 900000)
            // {
            //     await printExeptionValue2(botClient, chatId, cancellationToken);
            //     return;
            // }
            ifTRC20Taken[chatId] = true;

            if (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[3])
            {
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Віддаєте {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nТеперішній курс 1:{await binanceService.CountRightProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\monoRightBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\monoRightSellRequest.json")}",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вкажіть адресу гаманця TRC20, куди бажаєте отримати кошти");
            }
            else if (costomerModel[chatId].CurrencyCell == currencies[2])
            {
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Віддаєте {costomerModel[chatId].CurrencyCell}\nОтримуєте: {costomerModel[chatId].CurrencyGet}\nТеперішній курс 1:{await binanceService.CountRightProcentPriceAsync(@"D:\Progects\CryptoBot\CryptoBot\pryvatRightBuyRequest.json", @"D:\Progects\CryptoBot\CryptoBot\pryvatRightSellRequest.json")}",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вкажіть адресу гаманця TRC20, куди бажаєте отримати кошти");
            }
        }
        else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8])
        {
            ifTRC20Taken[chatId] = true;

            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вкажіть адресу гаманця TRC20, куди бажаєте отримати кошти");
        }


        async Task printExeptionValue1(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {

            lastMessage[chatId] = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Мінімальна сума = 100 одиниць\nМаксимальна сума = 20000 одиниць",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
            return;
        }

        async Task printExeptionValue2(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {

            lastMessage[chatId] = await botClient.SendTextMessageAsync(
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
            int min = 0;
            int max = 0;
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
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Введіть суму Tether, USDT, яку віддаєте ➡️ (ліміт: {min} USDT - {max} USDT):",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (costomerModel[chatId].CurrencyGet == currencies[0] && (costomerModel[chatId].CurrencyCell == currencies[1] || costomerModel[chatId].CurrencyCell == currencies[2] || costomerModel[chatId].CurrencyCell == currencies[3]))
            {
                min = 4000;
                max = 900000;

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Я хочу вказати, скільки отримаю ⬅️", "howManyGet"),
                    }
                });

                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Введіть скільки ви віддаєте. Мінімум: {min}, Максимум: {max}",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (costomerModel[chatId].CurrencyCell == currencies[0] && (costomerModel[chatId].CurrencyGet == currencies[7] || costomerModel[chatId].CurrencyGet == currencies[8]))
            {
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Зазначне суму USDT, яку віддаєте",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (costomerModel[chatId].CurrencyCell == currencies[7] || costomerModel[chatId].CurrencyCell == currencies[8])
            {
                if (costomerModel[chatId].CurrencyCell == currencies[7])
                {
                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Зазначне суму {currencies[7]}, яку віддаєте",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
                }
                else if (costomerModel[chatId].CurrencyCell == currencies[8])
                {
                    lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Зазначне суму {currencies[8]}, яку віддаєте",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
                }
            }
            else
            {
                lastMessage[chatId] = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"З вмами скоро зв'яжеться адміністратор",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }


            // await botClient.SendTextMessageAsync(
            //     chatId: adminChatId,
            //     text: $"Нова заявка!\n Ім'я: {costomerModel[chatId].FirstName}\n Фамілія: {costomerModel[chatId].LastName}\n Номер телефону: {costomerModel[chatId].Phone}\n Яку валюту віддає: {costomerModel[chatId].CurrencyCell} \n Скільки віддає: {costomerModel[chatId].HowMuchGives} \n Яку валюту отримує: {costomerModel[chatId].CurrencyGet} \n Номер карти: {costomerModel[chatId].CardNumber}",
            //     parseMode: ParseMode.Markdown,
            //     cancellationToken: cancellationToken
            // );
        }
    }
}



