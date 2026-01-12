public record CommandResponseDto
{
    public bool Status { get; set; }
    public string StatusMessage { get; set; }
    public int RecordId { get; set; }
}