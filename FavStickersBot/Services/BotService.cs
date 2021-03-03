using FavStickersBot.Models;
using Telegram.Bot;

namespace FavStickersBot.Services
{
    public class BotService : IBotService
    {
        public BotService()
        {
            Client = new TelegramBotClient(Settings.Token);
        }
        
        public TelegramBotClient Client { get; }
    }
}