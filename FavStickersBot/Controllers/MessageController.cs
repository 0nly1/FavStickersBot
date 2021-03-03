using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FavStickersBot.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace FavStickersBot.Controllers
{
    public class MessageController : Controller
    {
        // GET
        [HttpGet]
        public string Get()
        {
            return "Ok";
        }
        
        private readonly IBotService _botService;
        // TelegramKeyboard tk = new TelegramKeyboard();
        
        public MessageController(IBotService botService)
        {
            _botService = botService;
        }

        [HttpPost]
        public async Task<OkResult> Post([FromBody] Update update)
        {
            var client = _botService.Client;

            if (update == null)
            {
                return Ok();
            }
            
            long chatId = update.Message.Chat.Id;

            if (update.Type == UpdateType.Message)
            {
                if (update.Message.Sticker == null)
                {
                    await client.SendTextMessageAsync(chatId, "It's not a sticker");
                    return Ok();
                }
                
                var sticker = update.Message.Sticker;

                if (sticker.IsAnimated)
                {
                    await client.SendTextMessageAsync(chatId, "Sorry, I can't add animated stickers " +
                                                              "to your favorite set now.");
                    return Ok();
                }

                string fileId = sticker.FileId;
                
                string name = $"{update.Message.Chat.Username}_fav_by_fav_stickers_bot";
                StickerSet stickerSet = null;

                try
                {
                    stickerSet = await client.GetStickerSetAsync(name);
                }
                catch
                {
                    // ignore
                }

                var file = new InputOnlineFile(fileId);
                int userId = update.Message.From.Id;
                
                // check if this stickerpack exists
                if (stickerSet == null)
                {
                    await client.SendTextMessageAsync(chatId, "I couldn't find your fav sticker set. " +
                                                              "Creating a new one...");
                    
                    await client.CreateNewStickerSetAsync(userId, name, "Your Fav @fav_stickers_bot",
                        file, sticker.Emoji);

                    await client.SendTextMessageAsync(chatId, "I've created a new stickerpack for " +
                                                              $"you: t.me/addstickers/{name}.\n" +
                                                              "Now all the stickers that you send to me will appear there.");
                    return Ok();
                }

                if (stickerSet.Stickers.FirstOrDefault(x => x.FileUniqueId == sticker.FileUniqueId) != null)
                {
                    await client.DeleteStickerFromSetAsync(sticker.FileId);
                    await client.SendTextMessageAsync(chatId, "I removed it!");
                    return Ok();
                }
                
                await client.AddStickerToSetAsync(userId, name, file, sticker.Emoji);
                await client.SendTextMessageAsync(chatId, "I saved it!");
            }

            return Ok();
        }
    }
}