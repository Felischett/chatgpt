using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Messages
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int EmpfaengerId { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }

        [NotMapped]
        public string? TranslatedText { get; set; }

        // Das hat in deinem Code gefehlt:
        [NotMapped]
        public bool IsOwnMessage { get; set; }
    }
}