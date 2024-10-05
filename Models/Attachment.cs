namespace TeamIt.Models;
public class Attachment
{
    public int Id { get; set; }
    public int MessageId { get; set; } // Foreign key to Message
    public Message Message { get; set; }

    public string FilePath { get; set; }
    public string FileType { get; set; } // e.g., image, video
    public DateTime UploadedAt { get; set; }
}