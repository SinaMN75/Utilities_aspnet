using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities_aspnet.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ChatControllerV2 : BaseApiController
    {
        private readonly DbContext _context;
        private readonly IChatroomRepository _chatroomRepository;
        private readonly IMessageRepository _messagerepository;

        public ChatControllerV2(DbContext context, IChatroomRepository chatroomRepository, IMessageRepository messageRepository)
        {
            _context = context;
            _chatroomRepository = chatroomRepository;
            _messagerepository = messageRepository;
        }

        #region One to One Messaging
        [HttpPost("SendPrivateMessage")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ClaimRequirement]
        public async Task<ActionResult<GenericResponse>> SendPrivateMessage(ChatMessageInputDto message)
        {
            var receiver = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == message.ToUserId);
            if (receiver == null)
                return BadRequest();

            var result = await _messagerepository.AddPrivateMessage(message);

            return Ok(result);
        }

        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [ClaimRequirement]
        // [HttpGet("GetOneToOneMessages/{firstUserId}/{secondUserId}")]        
        // public async Task<ActionResult<GenericResponse<List<ChatMessage>>>> GetPrivateMessages(string firstUserId, string secondUserId)
        // {
        //     var messages = await _messagerepository.GetPrivateMessages(firstUserId, secondUserId);
        //     if (messages == null)
        //         return NoContent();
        //
        //     return Result(messages);
        // }

        [HttpPut("EditOneToOneMessages/{senderId}/{receiverId}/{messageText}/{messageId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ClaimRequirement]
        public async Task<ActionResult<GenericResponse>> EditPrivateMessages(string senderId, string receiverId, string messageText, Guid messageId)
            => Result(await _messagerepository.EditPrivateMessages(messageText, messageId));

        [HttpDelete("DeleteOneToOneMessages/{senderId}/{receiverId}/{messageId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ClaimRequirement]
        public async Task<ActionResult<GenericResponse>> DeletePrivateMessages(string senderId, string receiverId, Guid messageId)
            => Result(await _messagerepository.DeletePrivateMessage(messageId));

        #endregion

        #region Group Messaging
        [HttpPost("SendGroupMessage/{roomId}")]        
        public async Task<ActionResult<GenericResponse>> SendGroupMessage(ChatMessageInputDto message, Guid roomId)
        {
            var result = await _chatroomRepository.AddMessageToChatroom(roomId, message);

            return Ok(result);
        }

        [HttpGet("GetGroupMessage/{roomId}")]
        public async Task<ActionResult> GetGroupMessage(Guid roomId)
        {
            var roomMessages = await _chatroomRepository.GetChatroomMessages(roomId);

            return Ok(roomMessages);
        }

        [HttpPut("EditGroupMessage/{roomId}")]       
        public async Task<ActionResult> EditGroupMessage(Guid roomId, ChatMessageEditDto message)
        {
            await _chatroomRepository.EditGroupMessage(roomId, message);

            return Ok();
        }

        [HttpDelete("DeleteGroupMessage/{roomId}")]        
        public async Task<ActionResult> DeleteGroupMessage(Guid roomId, ChatMessageDeleteDto message)
        {
            await _chatroomRepository.DeleteGroupMessage(roomId, message);

            return Ok();
        }
        #endregion

        #region Seen/Online/Reaction
        //Todo: change method parameters to a dto
        [HttpPut("ModifyOnlineStatus/{userId}/{status}")]
        public async Task<ActionResult> ModifyOnlineStatus(string userId, bool status)
        {
            //Todo: change IsLoggedIn to IsOnline
            var user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return BadRequest();

            user.IsLoggedIn = status;
            await _context.SaveChangesAsync();
            return Ok();

        }

        //Todo: change method parameters to a dto
        [HttpPut("SeenMessage/{messageId}/receiverUser/senderUser")]
        public async Task<ActionResult> HasSeenPrivateMessage(Guid messageId, Guid receiverUser, string senderUser)
        {
            var isPrivateMessage = await _messagerepository.SeenMessage(messageId, receiverUser);

            if (isPrivateMessage)
            {
                var user = await _context.Set<UserEntity>().FirstOrDefaultAsync(x => x.Id == senderUser);
                if (user == null)
                    return BadRequest();

            }

            return Ok();

        }

        [HttpPut("AddEmoji/{emoji}/{messageId}/{userId}")]
        public async Task<ActionResult> AddEmojiToMessage(Reaction emoji, Guid messageId, Guid userId)
        {
            await _messagerepository.AddEmojiToMessage(emoji, messageId, userId);
            return Ok();

        }

        #endregion

        #region Chatroom
        [HttpPost("PostChatRoom/{userId}/{chatroomName}")]        
        public async Task<ActionResult> CreateChatroom(string chatroomName, string userId)
        {
            if (string.IsNullOrEmpty(chatroomName))
                return BadRequest();

            await _chatroomRepository.CreateChatroom(chatroomName, userId);
            return Ok();
        }

        [HttpPut("EditChatRoom/{userId}/{chatroomName}")]
        public async Task<ActionResult> EditChatroom(string chatroomName, string userId)
        {
            if (string.IsNullOrEmpty(chatroomName) || string.IsNullOrEmpty(userId.ToString()))
                return BadRequest();

            await _chatroomRepository.EditChatroom(chatroomName, userId);
            return Ok();
        }

        [HttpDelete("DeleteChatRoom/{userId}/{chatroomId}")]        
        public async Task<ActionResult> DeleteChatroom(Guid chatroomId, string userId)
        {
            if (string.IsNullOrEmpty(chatroomId.ToString()) || string.IsNullOrEmpty(userId.ToString()))
                return BadRequest();

            await _chatroomRepository.DeleteChatroom(chatroomId, userId);
            return Ok();
        }

        [HttpGet("GetChatRoom/{chatroomName}")]        
        public async Task<ActionResult> GetChatroomsByName(string chatroomName)
        {
            if (string.IsNullOrEmpty(chatroomName))
                return BadRequest();

            var result = await _chatroomRepository.GetChatroomsByName(chatroomName);
            return Ok(result);
        }

        [HttpGet("GetChatRoom/{chatroomId}")]        
        public async Task<ActionResult> GetChatroomsById(string chatroomId)
        {
            if (string.IsNullOrEmpty(chatroomId))
                return BadRequest();

            var result = await _chatroomRepository.GetChatroomsById(chatroomId);
            return Ok(result);
        }

        [HttpPut("AddUserToChatRoom/{chatroomId}")]        
        public async Task<ActionResult> AddUserToChatroom(Guid chatroomId, string userId)
        {
            await _chatroomRepository.AddUserToChatroom(chatroomId, userId);
            return Ok();
        }
        #endregion

        [HttpGet("GetLatestMessages/{userId}")]        
        public async Task<ActionResult> GetLatestMessages(string userId)
        {
            var result = await _messagerepository.GetLatestMessage(userId);

            return Ok(result);

        }

    }
}
