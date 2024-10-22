using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using System.Diagnostics;

namespace Telega_CidO_bot
{
    class Program
    {
        //static TelegramBotClient bot;

        /// <summary>
        /// Метод инициализирует список комманд для бота
        /// </summary>
        /// <param name="bot">Бот</param>
        static void InitializeBotCommands(TelegramBotClient bot)
        {
            Telegram.Bot.Types.BotCommand bc_start = new Telegram.Bot.Types.BotCommand();
            bc_start.Command = "start"; bc_start.Description = "starting bot";

            Telegram.Bot.Types.BotCommand bc_calculate = new Telegram.Bot.Types.BotCommand();
            bc_calculate.Command = "calculate";
            bc_calculate.Description = "Простой калькулятор. (прим. /calculate 2+2*2 или /c)";

            Telegram.Bot.Types.BotCommand bc_time = new Telegram.Bot.Types.BotCommand();
            bc_time.Command = "time"; bc_time.Description = "Текущее время";

            //Telegram.Bot.Types.BotCommand bc_files = new Telegram.Bot.Types.BotCommand();
            //bc_files.Command = "files"; bc_files.Description = "Список сохраненных файлов";

            Telegram.Bot.Types.BotCommand bc_olo = new Telegram.Bot.Types.BotCommand();
            bc_olo.Command = "olo"; bc_olo.Description = "Ну давай, чел";

            Telegram.Bot.Types.BotCommand bc_cheDelat = new Telegram.Bot.Types.BotCommand();
            bc_cheDelat.Command = "cheDelat'"; bc_cheDelat.Description = "Если не заешь чем заняться";

            List<Telegram.Bot.Types.BotCommand> botCommands = new List<Telegram.Bot.Types.BotCommand>();
            botCommands.Add(bc_start);
            botCommands.Add(bc_calculate);
            botCommands.Add(bc_time);
            //botCommands.Add(bc_files);
            botCommands.Add(bc_olo);
            botCommands.Add(bc_cheDelat);
            bot.SetMyCommandsAsync(botCommands);
            Console.WriteLine(bot.GetMyCommandsAsync());
        }

        /// <summary>
        /// Метод для комманды бота /calculate
        /// </summary>
        /// <param name="bot">Бот</param>
        /// <param name="e">Сообщение в TG</param>
        static void BotCalculation(TelegramBotClient bot, Update e)
        {

            DataTable dummy = new DataTable();
            string result, botAnswer;
            var text = e.Message.Text;
            try
            {
                text = text.Split(' ')[1];
                result = dummy.Compute(text, string.Empty).ToString();
                botAnswer = $"Результат вычисления: {result}";
            }
            catch
            {
                botAnswer = "Ошибка синтаксиса";
            }
            bot.SendTextMessageAsync(e.Message.Chat.Id, botAnswer);

        }


        static List<CidSticker> LoadStickers(string name)
        {
            if (!System.IO.File.Exists($"{name}.json"))
            {
                return null;
            }
            string json = System.IO.File.ReadAllText($"{name}.json");

            List<CidSticker> pasta = JsonConvert.DeserializeObject<List<CidSticker>>(json);
            return pasta;
        }

        static void SaveStickers(List<CidSticker> sticks, string name)
        {
            string json = JsonConvert.SerializeObject(sticks);
            System.IO.File.WriteAllText($"{name}.json", json);
        }

        static List<Pasta> LoadPastas()
        {
            if (!System.IO.File.Exists($"pastas.json"))
            {
                return null;
            }
            string json = System.IO.File.ReadAllText($"pastas.json");

            List<Pasta> pasta = JsonConvert.DeserializeObject<List<Pasta>>(json);
            return pasta;
        }

        static void SavePastas(List<Pasta> pastas)
        {
            string json = JsonConvert.SerializeObject(pastas);
            System.IO.File.WriteAllText($"pastas.json", json);
        }

        static int DiceRoll(int sides)
        {
            Random random = new Random();
            int roll = random.Next(1, sides + 1);
            return roll;
        }

        /// <summary>
        /// Добавить имя для стикера [/addname ,sticker Unique Name,new name]
        /// </summary>
        /// <param name="sticks"></param>
        /// <param name="text"></param>
        /// <param name="bot"></param>
        /// <param name="chat"></param>
        static void AddNameToSticker(List<CidSticker> sticks, string text, TelegramBotClient bot, long chat, string packName)
        {
            var splitedMessage = text.Split(',');
            string reply = "Стикер с таким именем не найден";
            if (splitedMessage.Length != 3)
            {
                reply = "Имя не добавлено. [/addname ,sticker Unique Name,new name]";
                bot.SendTextMessageAsync(chat, reply);
                return;
            }
            //CidSticker st = new CidSticker();
            foreach (var stick in sticks)
            {
                if (stick.uniqueName == splitedMessage[1])
                {
                    stick.names.Add(splitedMessage[2]);
                    SaveStickers(sticks, packName);
                    reply = "Для стикера " + splitedMessage[1] + " добавлено имя: " + splitedMessage[2];
                    bot.SendTextMessageAsync(chat, reply);
                    return;
                }
            }
            bot.SendTextMessageAsync(chat, reply);
        }


        /// <summary>
        /// Записывает List_string в string, разделенные запятой
        /// </summary>
        /// <param name="namesListed"></param>
        /// <returns></returns>
        static string GetAllNames(List<string> namesListed)
        {
            string namesString = "";
            for (int i = 0; i < namesListed.Count; i++)
            {
                namesString = namesString + $" {namesListed[i]},";
            }
            namesString = namesString.TrimEnd(",".ToCharArray());

            return namesString;
        }

        /// <summary>
        /// Изменить имя стикера
        /// </summary>
        /// <param name="sticks"></param>
        /// <param name="text"></param>
        /// <param name="bot"></param>
        /// <param name="chat"></param>
        static void ChangeStickerName(List<CidSticker> sticks, string text, TelegramBotClient bot, long chat, string packName)
        {
            var splitedText = text.Split(' ');
            string reply = "";
            if (splitedText.Length != 3)
            {
                reply = "Ошибка синтаксиса. [/command {name} {new name}]";
                bot.SendTextMessageAsync(chat, reply);
                return;
            }
            foreach (var st in sticks)
            {
                if (splitedText[1] == st.uniqueName)
                {
                    st.uniqueName = splitedText[2];
                    st.names[0] = splitedText[2];
                    SaveStickers(sticks, packName);
                    reply = $"Имя {splitedText[1]} заменено на {splitedText[2]}";
                    bot.SendTextMessageAsync(chat, reply);
                    return;
                }
            }
            reply = $"Стикер {splitedText[1]} не найден";
            bot.SendTextMessageAsync(chat, reply);
        }

        /// <summary>
        /// Удаляет стикер из набора. Не обновляет текущий набор.
        /// </summary>
        /// <param name="sticks"></param>
        /// <param name="text"></param>
        /// <param name="bot"></param>
        /// <param name="chat"></param>
        /// <param name="packName"></param>
        static void DeleteSticker(List<CidSticker> sticks, string text, TelegramBotClient bot, long chat, string packName)
        {
            var splitedText = text.Split(' ');
            string reply = "";
            if (splitedText.Length != 2)
            {
                reply = "Ошибка синтаксиса. [/command {name}]";
                bot.SendTextMessageAsync(chat, reply);
                return;
            }
            foreach (var st in sticks)
            {
                if (splitedText[1] == st.uniqueName)
                {
                    sticks.Remove(st);
                    SaveStickers(sticks, packName);
                    reply = $"Стикер {splitedText[1]} удалён";
                    bot.SendTextMessageAsync(chat, reply);
                    return;
                }
            }
            reply = $"Стикер {splitedText[1]} не найден";
            bot.SendTextMessageAsync(chat, reply);
        }

        /// <summary>
        /// Добавляет пасту
        /// </summary>
        /// <param name="pastas"></param>
        /// <param name="text"></param>
        /// <param name="bot"></param>
        /// <param name="chat"></param>
        /// <param name="messageToReply"></param>
        static void AddPasta(List<Pasta> pastas, string text, TelegramBotClient bot, long chat, int messageToReply)
        {
            var splitedText = text.Split(" ; ".ToCharArray());
            string reply = "";
            if (splitedText.Length != 3)
            {
                reply = "Ошибка синтаксиса. Новая паста должа быть вида\n[/addpasta ; Имя пасты ; Содержание пасты]. (слева и справа от \";\" обязательно должен быть пробел)";
                bot.SendTextMessageAsync(chat, reply, disableNotification: true, replyToMessageId: messageToReply);
                return;
            }
            foreach (var e in pastas)
            {
                if (e.Name == splitedText[1])
                {
                    reply = "Паста с таким именем уже существует";
                    bot.SendTextMessageAsync(chat, reply, disableNotification: true, replyToMessageId: messageToReply);
                    return;
                }
            }
            pastas.Add(new Pasta(splitedText[1], splitedText[2]));
            SavePastas(pastas);
            reply = $"Паста !{splitedText[1]} добавлена:\n {splitedText[2]}";
            bot.SendTextMessageAsync(chat, reply, disableNotification: true, replyToMessageId:messageToReply);
        }

        /// <summary>
        /// Список всех паст
        /// </summary>
        /// <param name="pastas"></param>
        /// <returns></returns>
        static string GetAllPastasNames(List<Pasta> pastas)
        {
            string pastasNames = "";
            foreach (var ps in pastas)
            {
                pastasNames += "!" + ps.Name + "; ";
            }
            return pastasNames;
        }

        /// <summary>
        /// Изменить имя пасты
        /// </summary>
        /// <param name="pastas"></param>
        /// <param name="text"></param>
        /// <param name="bot"></param>
        /// <param name="chat"></param>
        static void RenamePasta(List<Pasta> pastas, string text, TelegramBotClient bot, long chat)
        {
            var splitedText = text.Split(" ; ".ToCharArray());
            string reply = "";
            if (splitedText.Length != 3)
            {
                reply = "Ошибка синтаксиса. Новое имя пасты должо быть вида\n[/addpasta ; Старое имя ; Новое имя]. (слева и справа от \";\" обязательно должен быть пробел)";
                bot.SendTextMessageAsync(chat, reply);
                return;
            }
            foreach (var e in pastas)
            {
                if (e.Name == splitedText[1])
                {
                    e.Name = splitedText[2];
                    SavePastas(pastas);
                    reply = $"Имя \"!{splitedText[1]}\" заменено на \"!{splitedText[2]}\"";
                    bot.SendTextMessageAsync(chat, reply);
                    return;
                }
            }
            reply = $"Паста !{splitedText[1]} не найдена";
            bot.SendTextMessageAsync(chat, reply);
        }






        static void NotMain(string[] args)
        {
            // Создать бота, позволяющего принимать разные типы файлов, 
            // *Научить бота отправлять выбранный файл в ответ
            // 
            // https://data.mos.ru/
            // https://apidata.mos.ru/



            string token = System.IO.File.ReadAllText(@"D:\bot_token.txt");
            //string token = "5505297146:AAGx8TyjpdynfOOZpUjIhID66TH5XxRNwgs";
            var bot = new TelegramBotClient(token);
            using var cts = new CancellationTokenSource();
            var recieverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            DateTime currentTime = DateTime.Now;
            DateTime timeCheck = DateTime.Now;


            long admin = 743376539;
            long userID = 0;
            string text = "";
            string fN = "";
            long id = 0;
            string mType = "";
            var rdm = new Random();
            var watch = new Stopwatch();
            string reply;
            int counter;


            string cidoEmotionsName = "CidO_Emotions";
            string pidorSticksName = "Pidor_Sticks";
            string currentPackName = "";
            CidSticker lastSticker = new CidSticker();
            List<CidSticker> cidoEmotions = new List<CidSticker>();
            List<CidSticker> pidorSticks = new List<CidSticker>();
            List<CidSticker> currentPack = new List<CidSticker>();

            Pasta tempPasta = new Pasta();
            List<Pasta> pastas = new List<Pasta>();

            bool wasLastSticker = false;
            bool stickAddMode = false;
            bool cidoEmotionsMode = false;
            bool pidorSticksMode = false;
            bool checker = false;


            cidoEmotions = LoadStickers(cidoEmotionsName);
            pidorSticks = LoadStickers(pidorSticksName);
            pastas = LoadPastas();
            if (cidoEmotions == null) cidoEmotions = new List<CidSticker>();
            if (pidorSticks == null) pidorSticks = new List<CidSticker>();
            if (pastas == null) pastas = new List<Pasta>();

            bot.StartReceiving(
                ResponseAction,
                HandleErrorsAsync,
                recieverOptions,
                cancellationToken: cts.Token);

            //bot.StartReceiving(
            //    HandleUpdatesAsync,
            //    HandleErrorsAsync,
            //    recieverOptions,
            //    cancellationToken: cts.Token);


            Console.ReadLine();

            cts.Cancel();


            Task HandleErrorsAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
            {
                var errorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Ошибка телеги \n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                    _ => "У Jolly551 кривые руки\n" + exception.ToString()
                };
                Console.WriteLine(errorMessage);
                return Task.CompletedTask;
            }


            async Task ResponseAction(ITelegramBotClient botClient, Update e, CancellationToken cancellationToken)
            {
                timeCheck = DateTime.Now;

                if (e.Message != null)
                {
                    if (timeCheck.AddSeconds(-20).AddHours(-3) >= e.Message.Date)
                    {
                        Console.WriteLine("Сообщение устарело " + e.Message.Date.AddHours(3));
                        return;
                    }
                    text = e.Message.Text;
                    fN = e.Message.Chat.FirstName;
                    id = e.Message.Chat.Id;
                    mType = e.Message.Type.ToString();
                }
                else
                {
                    return;
                }

                string[] splitedText = null;
                if (text != null)
                {
                    splitedText = text.Replace("\n", " ").Split(' ');
                }



                currentTime = DateTime.Now;
                int messageId = e.Message.MessageId;

                if (e.Message != null)
                {
                    string cName = e.Message.From.FirstName;
                    string cLName = e.Message.From.LastName;
                    userID = e.Message.From.Id;

                    Console.WriteLine("{5:hh:mm:ss} ID: {0} {1} {2} userID {3}. Says: {4}",

                    id,
                    cName,
                    "",
                    userID,
                    text,
                    currentTime);
                }
                else
                {
                    Console.WriteLine("Тип сообщения: {0} Отправитель {1} id {2} \"{3}\"",
                        mType,
                        fN,
                        id,
                        text);

                }


                // режим добавления стикеров
                if (stickAddMode)
                {
                    if (userID != admin)
                    {
                        goto skip;
                    }

                    if (cidoEmotionsMode)
                    {
                        currentPackName = cidoEmotionsName;
                        currentPack = cidoEmotions;
                    }
                    else if (pidorSticksMode)
                    {
                        currentPackName = pidorSticksName;
                        currentPack = pidorSticks;
                    }


                    if (e.Message.Type == MessageType.Sticker)
                    {
                        wasLastSticker = true;
                        await bot.SendTextMessageAsync(id, e.Message.Sticker.FileId);
                        await bot.SendTextMessageAsync(id, "Задайте имена стикеру. Первое будет основное");
                        lastSticker.id = e.Message.Sticker.FileId;
                    }

                    if (text != null && wasLastSticker)
                    {
                        wasLastSticker = false;

                        //splitedText = text.Split(" ");
                        lastSticker.uniqueName = splitedText[0];
                        lastSticker.names = new List<string>();

                        for (int i = 0; i <= splitedText.Length - 1; i++)
                        {
                            lastSticker.names.Add(splitedText[i]);
                        }


                        currentPack.Add(lastSticker);
                        SaveStickers(currentPack, currentPackName);
                        await bot.SendTextMessageAsync(id, "Стикер добавлен");
                        lastSticker = null;
                        lastSticker = new CidSticker();
                    }
                }
            skip:


                // MAIN
                if (text != null)
                {


                    //Отправка эмоций CidO_bot
                    string lastWord = splitedText[splitedText.Length - 1].ToLower();

                    //peepoWow
                    if (text.ToLower().StartsWith("вот бы "))
                    {
                        await bot.SendStickerAsync(id, InputFile.FromFileId(cidoEmotions[27].id),
                            disableNotification: true, replyToMessageId: messageId);
                    }


                    //cidoEmotions.
                    foreach (var st in cidoEmotions)
                    {
                        if (st.names.Contains(lastWord))
                        {
                            await bot.SendStickerAsync(id, InputFile.FromFileId(st.id),
                                disableNotification: true, replyToMessageId: messageId);
                            break;
                        }
                    }
                    

                    // ПАСТЫ
                    if (text.StartsWith("!"))
                    {
                        text = text.TrimStart("!".ToCharArray());
                        foreach (var ps in pastas)
                        {
                            if (ps.Name.ToLower() == text.ToLower())
                            {
                                await bot.SendTextMessageAsync(id, ps.Content, disableNotification: true, replyToMessageId: messageId);
                                break;
                            }
                        }
                    }

                    while (text.EndsWith(".") || text.EndsWith("!") || text.EndsWith("?"))
                    {
                        text = text.Remove(text.Length - 1);
                    }

                    // PIDOR PACK
                    if (text.ToLower() == "я пидор" || text.ToLower() == "я милашка" || text.ToLower() == "я няшка")
                    {
                        await bot.SendStickerAsync(id, InputFile.FromFileId(pidorSticks[rdm.Next(0, pidorSticks.Count)].id),
                            disableNotification: true, replyToMessageId: messageId);
                        return;
                    }



                    if (text.ToLower().EndsWith(" ало") ||
                        text.ToLower().EndsWith(" оло") ||
                        text.ToLower().EndsWith(" але") ||
                        text.ToLower().EndsWith(" alo") ||
                        text.ToLower().EndsWith(" olo") ||
                        text.ToLower().EndsWith(" ale"))
                    {
                        await bot.SendTextMessageAsync(id, "Хуем по лбу не дало?", disableNotification: true, replyToMessageId: messageId);
                    }
                    else if (text.ToLower() == "ало" ||
                        text.ToLower() == "оло" ||
                        text.ToLower() == "але" ||
                        text.ToLower() == "alo" ||
                        text.ToLower() == "olo" ||
                        text.ToLower() == "ale")
                    {
                        await bot.SendTextMessageAsync(id, "Хуем по лбу не дало?", disableNotification: true, replyToMessageId: messageId);
                    }

                    if (text.ToLower() == "когда")
                    {
                        await bot.SendTextMessageAsync(id, "Завтра в 3", disableNotification: true, replyToMessageId: messageId);
                    }

                    if (text.ToLower() == "че делать" ||
                        text.ToLower() == "что делать")
                    {
                        await bot.SendTextMessageAsync(id, "Муравью хуй приделать", disableNotification: true, replyToMessageId: messageId);
                    }

                    if (text.ToLower() == "джоли лох" ||
                        text.ToLower() == "джолли лох")
                    {
                        await bot.SendTextMessageAsync(id, "Согласен", disableNotification: true, replyToMessageId: messageId);
                    }
                    if (text.ToLower() == "бот вонючка")
                    {
                        var sticker = InputFile.FromFileId(("CAACAgIAAxkBAAEE6n9inHHcau5PZyhTmuClL0Gai40eowACRSIAAj2loUg57CRc1AHg_iQE"));
                        await bot.SendStickerAsync(id, sticker, disableNotification: true, replyToMessageId: messageId);
                    }

                    if (text.ToLower().EndsWith("да") && rdm.Next(1, 101) <= 5)
                    {
                        if (text.ToLower().EndsWith(" да") && text.ToLower().Split(' ').Length == 2)
                        {

                            await bot.SendTextMessageAsync(id, text.Split(' ')[0] + " пизда 😎", disableNotification: true, replyToMessageId: messageId);
                            return;
                        }
                        if (text.ToLower().EndsWith(" да") || text.ToLower() == "да")
                        {
                            await bot.SendTextMessageAsync(id, "Пизда 😎", replyToMessageId: e.Message.MessageId);
                        }
                    }

                    if ((text.ToLower().EndsWith("где") ||
                        text.ToLower().EndsWith("где?")) &&
                        rdm.Next(1, 101) <= 25)
                    {
                        await bot.SendTextMessageAsync(id, "В пизде 😎", disableNotification: true, replyToMessageId: messageId);
                    }

                    if (text.Contains("Cidavin_bot"))
                    {
                        await bot.SendStickerAsync(id, InputFile.FromFileId(cidoEmotions[72].id), disableNotification: true, replyToMessageId: messageId);
                    }
                }



                if (mType == "Text" && text.StartsWith("/"))
                {

                    if (text.StartsWith("/d "))
                    {
                        string num = text.Split(' ')[1];

                    }
                    string textCommand = splitedText[0];
                    switch (textCommand)
                    {
                        case "/start": await bot.SendTextMessageAsync(id, "Йоу"); break;
                        case "/help":
                            reply = $"Бот @Cidavin_bot\n Список дооступных комманд:\n" +
                                $"Общие:\n" +
                                $"/time - текущее время\n" +
                                $"/calculate (/c) - калькулятор\n" +
                                $"/d20 - кидает куб d20\n" +
                                $"/pastas - список паст\n" +
                                $"/addpasta - добавить пасту\n" +
                                $"/delpasta - удалить пасту\n" +
                                $"/renamepasta - изменить имя пасты\n" +
                                $"Admin:\n" +
                                $"/mode - режим добавления стикеров\n" +
                                $"/addname - [command ,sticker_Unique_Name,new_name] - добавляет новое имя стикера\n" +
                                $"/getstick - информация по стикеру\n" +
                                $"/changename - [command sticker_Unique_Name new_name]\n" +
                                $"/delstick - удалить стикер [command sticker_Unique_Name]";
                            await bot.SendTextMessageAsync(id, reply, disableNotification: true, replyToMessageId: messageId);
                            break;
                        case "/time": await bot.SendTextMessageAsync(id, DateTime.Now.ToString()); break;
                        case "/calculate": BotCalculation(bot, e); break;
                        case "/c": BotCalculation(bot, e); break;
                        case "/d20":
                            await bot.SendTextMessageAsync(id, DiceRoll(20).ToString());
                            break;
                        case "/olo": await bot.SendTextMessageAsync(id, "Хуем по лбу не дало?"); break;
                        case "/чеделать": await bot.SendTextMessageAsync(id, "Муравью хуй приделать"); break;
                        case "/addname":
                            if (id != admin)
                            {
                                await bot.SendTextMessageAsync(id, cidoEmotions[33].id, disableNotification: true, replyToMessageId: messageId);
                                break;
                            }
                            AddNameToSticker(cidoEmotions, text, bot, id, cidoEmotionsName); break;
                        case "/delstick":
                            if (userID != admin) break;
                            DeleteSticker(cidoEmotions, text, bot, id, cidoEmotionsName);
                            cidoEmotions = LoadStickers(cidoEmotionsName);
                            break;
                        case "/mode":
                            if (userID != admin) break;
                            if (splitedText.Length != 2) break;

                            switch (splitedText[1])
                            {
                                case "0":
                                    stickAddMode = false; cidoEmotionsMode = false; pidorSticksMode = false;
                                    reply = $"Режим добавления стикеров {stickAddMode}"; break;
                                case "1":
                                    stickAddMode = true; cidoEmotionsMode = true; pidorSticksMode = false;
                                    reply = $"Режим добавления стикеров {cidoEmotionsName}"; break;
                                case "2":
                                    stickAddMode = true; cidoEmotionsMode = false; pidorSticksMode = true;
                                    reply = $"Режим добавления стикеров {pidorSticksName}"; break;
                                default:
                                    reply = "Режим не найден";
                                    break;
                            }
                            await bot.SendTextMessageAsync(id, reply);
                            break;

                        case "/getstick":
                            if (splitedText.Length <= 1)
                            {
                                await bot.SendTextMessageAsync(id, "Введите имя стикера"); break;
                            }
                            counter = 0;
                            foreach (var stick in cidoEmotions)
                            {
                                if (stick.uniqueName == splitedText[1])
                                {
                                    reply = $"{stick.uniqueName} \nномер в массиве {counter} \nID {stick.id} \nвсе имена: ";
                                    reply = reply + GetAllNames(stick.names);
                                    await bot.SendTextMessageAsync(id, reply);
                                    break;
                                }
                                counter++;
                            }
                            break;
                        case "/changename":
                            if (id != admin)
                            {
                                await bot.SendTextMessageAsync(id, cidoEmotions[33].id, disableNotification: true, replyToMessageId: messageId);
                                break;
                            }
                            ChangeStickerName(cidoEmotions, text, bot, id, cidoEmotionsName);
                            break;
                        case "/addpasta": AddPasta(pastas, text, bot, id, messageId); break;
                        case "/pastas":
                            reply = $"Список сохраненных паст:\n\n" + GetAllPastasNames(pastas);
                            await bot.SendTextMessageAsync(id, reply);
                            break;
                        case "/renamepasta": RenamePasta(pastas, text, bot, id); break;

                        default:
                            //bot.SendTextMessageAsync(id, "Неизвестная комманда");
                            break;
                    }
                }



                //Console.WriteLine("Время " + timeCheck.Subtract(DateTime.Now));

            }







        }




    }
}
