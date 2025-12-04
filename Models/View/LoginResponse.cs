namespace FashionApi.Models.View
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public NguoiDungView User { get; set; }
    }
}
