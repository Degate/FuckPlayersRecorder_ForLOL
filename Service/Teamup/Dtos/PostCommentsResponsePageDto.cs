namespace FuckPlayersRecorder_ForLOL.Service.Teamup.Dtos
{
    public class PostCommentsResponsePageDto
    {
        public int Count { get; set; }
        public IEnumerable<PostCommentDto> Data { get; set; }
    }
}
