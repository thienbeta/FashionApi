using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FashionApi.Models.Create
{
    public class BatchDeleteImageRequest
    {
        [Required(ErrorMessage = "Danh sách mã hình ảnh không được trống")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 hình ảnh")]
        public List<int> MediaIds { get; set; } = new List<int>();

        public bool HardDelete { get; set; } = false;
    }
}
