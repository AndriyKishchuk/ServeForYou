namespace DataBase.Users
{
    public class TaskFile
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public Tasks Task { get; set; } = null!;

        public int UploadedByUserId { get; set; }
        public User UploadedBy { get; set; } = null!;

        public string FileName { get; set; } = string.Empty;        // оригінальна назва
        public string StoredFileName { get; set; } = string.Empty;  // guid + extension
        public string FilePath { get; set; } = string.Empty;        // відносний шлях
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public FileCategory Category { get; set; }
    }

    public enum FileCategory
    {
        Attachment = 1,  // Manager
        Result = 2       // Employee
    }
}
