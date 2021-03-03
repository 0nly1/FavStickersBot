using Telegram.Bot;

namespace FavStickersBot.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}