using System.ComponentModel.DataAnnotations;

namespace ScoreService.DtOs;

public class UpdateScoreRequest
{
    [Range(0.0, 10.0, ErrorMessage = "Điểm số phải từ 0.0 đến 10.0")]
    public decimal ScoreValue { get; set; }
}
