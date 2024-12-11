namespace TaskManagementApp.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; } // Low, Medium, High
        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; } 
        public string UserId { get; set; }
        public string UserName { get; set; }// To associate tasks with users
    }
    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Completed
    }
}
