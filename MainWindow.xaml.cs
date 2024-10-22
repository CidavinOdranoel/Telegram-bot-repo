using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Data;
using Telega_CidO_bot;
using Telegram.Bot.Polling;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Telegram_bot_WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public void SendMsg()
        {
            var concreteUser = Users[Users.IndexOf(userList.SelectedItem as TelegramUser)];
            string respondMes = $"CidO: {txtSendMes.Text}";
            concreteUser.Messages.Add(respondMes);
            CidSticker cidSticker = cidoEmotions.FirstOrDefault(x => x.uniqueName == txtSendMes.Text.ToLower());

            if ( cidSticker != null)
            {
                bot.SendStickerAsync(concreteUser.Id, FindSticker(cidSticker.uniqueName));
                return;
            }

            bot.SendTextMessageAsync(concreteUser.Id, txtSendMes.Text);

            txtSendMes.Text = String.Empty;
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

            return JsonConvert.DeserializeObject<List<CidSticker>>(json);
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
                bot.SendTextMessageAsync(chat, reply, replyToMessageId: messageToReply);
                return;
            }
            foreach (var e in pastas)
            {
                if (e.Name == splitedText[1])
                {
                    reply = "Паста с таким именем уже существует";
                    bot.SendTextMessageAsync(chat, reply, replyToMessageId: messageToReply);
                    return;
                }
            }
            pastas.Add(new Pasta(splitedText[1], splitedText[2]));
            SavePastas(pastas);
            reply = $"Паста !{splitedText[1]} добавлена:\n {splitedText[2]}";
            bot.SendTextMessageAsync(chat, reply, replyToMessageId: messageToReply);
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

        static string GetAllNamesInTheStickerpack(List<CidSticker> pack)
        {
            string result = string.Empty;

            foreach (var e in pack)
            {
                result += $"{e.uniqueName} (";
                result += String.Join(",", e.names) + ")\n";
            }

            return "Увы";
        }

        /// <summary>
        /// Находит стикер по уникальному имени
        /// </summary>
        /// <param name="uniqueName"></param>
        /// <returns></returns>
        static InputFileId FindSticker(string uniqueName)
        {
            return InputFile.FromFileId(cidoEmotions.Find(x => x.uniqueName == uniqueName).id);
        }

        ObservableCollection<TelegramUser> Users;

        //   BOT
        static string token = System.IO.File.ReadAllText(@"D:\bot_token.txt");
        //static string token = "5505297146:AAGx8TyjpdynfOOZpUjIhID66TH5XxRNwgs";
        static TelegramBotClient bot = new TelegramBotClient(token);

        static List<CidSticker> cidoEmotions = new List<CidSticker>();
        public MainWindow()
        {
            InitializeComponent();
            if (!System.IO.File.Exists("messages.json"))
            {
                Users = new ObservableCollection<TelegramUser>();
            }
            else
            {
                string json = System.IO.File.ReadAllText("messages.json");
                Users = JsonConvert.DeserializeObject<ObservableCollection<TelegramUser>>(json);
            }

            userList.ItemsSource = Users;

            #region OLD

            ////string token = System.IO.File.ReadAllText(@"D:\bot_token.txt");
            //string token = "5505297146:AAGx8TyjpdynfOOZpUjIhID66TH5XxRNwgs"; //test bot
            //var bot = new TelegramBotClient(token);
            //var cts = new CancellationTokenSource();
            //var recieverOptions = new ReceiverOptions
            //{
            //    AllowedUpdates = { }
            //};

            //bot.StartReceiving(
            //    HandleUpdatesAsync,
            //    HandleErrorsAsync,
            //    recieverOptions,
            //    cancellationToken: cts.Token);


            ////cts.Cancel();

            //async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            //{
            //    if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            //    {
            //        await HandleMessage(botClient, update.Message);
            //        return;
            //    }

            //    //var message = update.Message;
            //    //string msg = $"{DateTime.Now}: {message.Chat.FirstName} {message.Chat.Id} {message.Text}";

            //    //this.Dispatcher.Invoke(() =>
            //    //{
            //    //    var person = new TelegramUser(message.Chat.FirstName, message.Chat.Id);
            //    //    if (!Users.Contains(person)) Users.Add(person);
            //    //    Users[Users.IndexOf(person)].AddMessage($"{person.Name}: {message.Text}");
            //    //});

            //    if (update.Type == UpdateType.CallbackQuery)
            //    {
            //        await HandleCallbackQuery(botClient, update.CallbackQuery);
            //        return;
            //    }
            //}

            //Task HandleErrorsAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
            //{
            //    var errorMessage = exception switch
            //    {
            //        ApiRequestException apiRequestException
            //            => $"Ошибка телеги \n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
            //        _ => "Ошибка кода\n" + exception.ToString()
            //    };
            //    Console.WriteLine(errorMessage);
            //    return Task.CompletedTask;
            //}


            //async Task HandleMessage(ITelegramBotClient botClient, Message message)
            //{

            //    string msg = $"{DateTime.Now}: {message.Chat.FirstName} {message.Chat.Id} {message.Text}";

            //    this.Dispatcher.Invoke(() =>
            //    {
            //        var person = new TelegramUser(message.Chat.FirstName, message.Chat.Id);
            //        if (!Users.Contains(person)) Users.Add(person);
            //        Users[Users.IndexOf(person)].AddMessage($"{person.Name}: {message.Text}");
            //    });

            //    btnSendMes.Click += delegate { SendMsg(bot); };
            //    txtSendMes.KeyDown += (s, e) => { if (e.Key == Key.Return) { SendMsg(bot); } };


            //}         

            //async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
            //{

            //    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Ты выбрал: " + callbackQuery.Data);
            //}

            #endregion

            using var cts = new CancellationTokenSource();
            var recieverOptions = new Telegram.Bot.Extensions.Polling.ReceiverOptions
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
            // List<CidSticker> cidoEmotions = new List<CidSticker>();
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
                 HandleUpdatesAsync,
                 HandleErrorsAsync,
                 recieverOptions,
                 cancellationToken: cts.Token);

            async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {

                if (update.Message == null) return;
                await ResponseAction(botClient, update);
                var message = update.Message;
                string msg = $"{DateTime.Now}: {message.Chat.FirstName} {message.Chat.Id} {message.Text}";

                this.Dispatcher.Invoke(() =>
                {
                    var person = new TelegramUser(message.Chat.FirstName, message.Chat.Id);
                    if (!Users.Contains(person))
                    {
                        Users.Add(person);
                        string json = JsonConvert.SerializeObject(Users);
                        System.IO.File.WriteAllText($"messages.json", json);
                    }
                    Users[Users.IndexOf(person)].AddMessage($"{person.Name}: {message.Text}");
                });
            }

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


            async Task ResponseAction(ITelegramBotClient botClient, Update e)
            {
                timeCheck = DateTime.Now;

                // Пропуск устаревших сообщений
                if (timeCheck.AddSeconds(-20).AddHours(-3) >= e.Message.Date)
                    return;

                text = e.Message.Text != null ? e.Message.Text : e.Message.Caption; // текст
                fN = e.Message.Chat.FirstName;          // имя
                id = e.Message.Chat.Id;                 // ID чата
                int messageId = e.Message.MessageId;    // ID сообщения
                userID = e.Message.From.Id;             // ID юзера
                mType = e.Message.Type.ToString();      // Тип сообщения
                string[] splitedText = text != null ? text.Replace("\n", " ").Split(' ') : null; // слова в сообщении

                // ЛОГИ
                string logPath = $"Messages/{id}_.txt";
                string log = $"({e.Message.Type},{e.Message.MessageId}) {e.Message.From.Username}: {text} // {currentTime}\n";
                System.IO.File.AppendAllText(logPath, log);


                // режим добавления стикеров
                if (stickAddMode && userID == admin)
                {
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



                // MAIN
                if (text != null)
                {
                    if (text.ToLower().EndsWith("медисон база") ||
                        text.ToLower().EndsWith("мэдисон база") ||
                        text.ToLower().EndsWith("мэд база") ||
                        text.ToLower().EndsWith("мед база") ||
                        text.ToLower().EndsWith("мeд база") ||
                        text.ToLower().EndsWith("мєд база") ||
                        text.ToLower().EndsWith("м3д база") ||
                        text.ToLower().EndsWith("мзд база") ||
                        text.ToLower().EndsWith("медовый база"))
                    {
                        await bot.SendTextMessageAsync(id, "🤡", disableNotification: true, replyToMessageId: messageId);
                        return;
                    }

                    if (text.ToLower().EndsWith("ева база") ||
                        text.ToLower().EndsWith("евангелион база"))
                    {
                        await bot.SendStickerAsync(id, FindSticker("jokerge"), disableNotification: true, replyToMessageId: messageId);
                        return;
                    }

                    //Отправка эмоций CidO_bot
                    string lastWord = splitedText[splitedText.Length - 1].ToLower();

                    //peepoWow
                    if (text.ToLower().StartsWith("вот бы "))
                    {
                        await bot.SendStickerAsync(id, FindSticker("peepowow"),
                            disableNotification: true, replyToMessageId: messageId);
                    }


                    //cidoEmotions
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

                    // OTHER
                    if (text.ToLower().EndsWith("ева кал") ||
                        text.ToLower().EndsWith("евангелион кал"))
                    {
                        await bot.SendStickerAsync(id, FindSticker("based"), disableNotification: true, replyToMessageId: messageId);
                        return;
                    }

                    if ((splitedText.Contains("пон") || splitedText.Contains("pon")) && rdm.Next(1, 101) <= 1)
                    {
                        await bot.SendStickerAsync(id, FindSticker("idinahuipon"), disableNotification: true, replyToMessageId:messageId);
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

                    if (text.ToLower().Contains("cidavin_bot"))
                    {
                        await bot.SendStickerAsync(id, InputFile.FromFileId(cidoEmotions[72].id), disableNotification: true, replyToMessageId: messageId);
                    }
                }


                // Direct commands
                if (mType == "Text" && text.StartsWith("/"))
                {
                    reply = String.Empty;
                    CidSticker dummyStick;
                    string textCommand = splitedText[0];
                    switch (textCommand)
                    {
                        case "/start": reply = System.IO.File.ReadAllText("texts/start.txt"); break;
                        case "/help": reply = System.IO.File.ReadAllText("texts/help.txt"); break;
                        case "/admin": reply = System.IO.File.ReadAllText("texts/admin.txt"); break;
                        case "/time": reply = DateTime.Now.ToString(); break;
                        case "/calculate": BotCalculation(bot, e); break;
                        case "/c": BotCalculation(bot, e); break;
                        case "/d20": reply = DiceRoll(20).ToString(); break;
                        case "/olo": reply = "Хуем по лбу не дало?"; break;
                        case "/чеделать": reply = "Муравью хуй приделать"; break;
                        case "/addname":
                            if (id != admin)
                            {
                                await bot.SendStickerAsync(id, FindSticker("jokerge"), disableNotification: true, replyToMessageId: messageId);
                                break;
                            }
                            if (splitedText.Length != 3)
                            {
                                reply = $"Неверный сиснтаксис\n/addname [uniqueName] [add name]";
                                break;
                            }
                            dummyStick = cidoEmotions.Find(x => x.uniqueName == splitedText[1]);
                            if (dummyStick != null)
                            {
                                reply = $"Для стикера {dummyStick.uniqueName} добавлено имя {splitedText[2]}";
                                dummyStick.AddNameToSticker(splitedText[2]);
                                SaveStickers(cidoEmotions, cidoEmotionsName);
                            }
                            else
                            {
                                reply = $"Стикер {splitedText[1]} не найден";
                            }
                            break;
                        case "/delstick":
                            if (userID != admin) break;
                            if (splitedText.Length < 2) break;
                            if (cidoEmotions.Remove(cidoEmotions.Find(x => x.uniqueName == splitedText[1])))
                            {
                                reply = $"Стикер {splitedText[1]} удалён";
                                SaveStickers(cidoEmotions, cidoEmotionsName);
                            }
                            else
                            {
                                reply = $"Стикер {splitedText[1]} не найден\n" +
                                    "/delstick [uniqueName]";
                            }
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
                            break;
                        case "/getstick":
                            if (splitedText.Length != 2)
                            {
                                reply = "Неправильный синтаксис\n/getstick [uniqueName]";
                                break;
                            }
                            dummyStick = cidoEmotions.Find(x => x.uniqueName == splitedText[1]);
                            if (dummyStick != null)
                            {
                                reply = $"{dummyStick.uniqueName}\n" +
                                    $"ID: {dummyStick.id}\n" +
                                    $"Все имена:\n" +
                                    dummyStick.GetAllNames();
                            }
                            else
                            {
                                reply = $"Стикер {splitedText[1]} не найден";
                            }
                            break;
                        case "/allemotions":
                            reply = GetAllNamesInTheStickerpack(cidoEmotions);
                            break;
                        case "/changename":
                            if (id != admin)
                            {
                                await bot.SendStickerAsync(id, FindSticker("jokerge"), disableNotification: true, replyToMessageId: messageId);
                                break;
                            }
                            //ChangeStickerName(cidoEmotions, text, bot, id, cidoEmotionsName);
                            if (splitedText.Length != 3)
                            {
                                reply = "Неправильный синтаксис\n/changename [uniqueName] [new name]";
                                break;
                            }
                            dummyStick = cidoEmotions.Find(x => x.uniqueName == splitedText[1]);
                            if (dummyStick != null)
                            {
                                reply = $"Имя изменено с {dummyStick.uniqueName} на {splitedText[2]}";
                                dummyStick.ChangeUniqueName(splitedText[2]);
                                SaveStickers(cidoEmotions, cidoEmotionsName);
                            }
                            else
                            {
                                reply = $"Стикер {splitedText[1]} не найден";

                            }
                            break;
                        case "/addpasta": AddPasta(pastas, text, bot, id, messageId); break;
                        case "/pastas":
                            reply = $"Список сохраненных паст:\n\n" + GetAllPastasNames(pastas);
                            break;
                        case "/renamepasta": RenamePasta(pastas, text, bot, id); break;

                        default:
                            //bot.SendTextMessageAsync(id, "Неизвестная комманда");
                            break;

                    }
                    if (!String.IsNullOrEmpty(reply))
                        await bot.SendTextMessageAsync(id, reply);
                }



                //Console.WriteLine("Время " + timeCheck.Subtract(DateTime.Now));
            }


        }



        private void saveMessages_Click(object sender, RoutedEventArgs e)
        {

            string json = JsonConvert.SerializeObject(Users);
            System.IO.File.WriteAllText($"messages.json", json);

        }

        private void but2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSendMes_Click(object sender, RoutedEventArgs e)
        {
            SendMsg();
        }

        private void txtSendMes_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
