namespace FuckPlayersRecorder_ForLOL.Service.Teamup.Dtos
{
    public class PostResponsePageDto
    {
        public int Count { get; set; }
        public IEnumerable<PostResponseDto> Data { get; set; }
    }
}
