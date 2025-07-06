using System.ComponentModel.DataAnnotations;

namespace FuckPlayersRecorder_ForLOL.Service.Teamup.Dtos
{
    public class CreatePostCommentDto
    {
        public long PostId { get; set; }
        public string Content { get; set; }
    }
}
