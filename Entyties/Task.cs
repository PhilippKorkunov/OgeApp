using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgeApp.Entyties
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDone { get; set; }
        public string Text { get; set; }
        public string? UserAnswer { get; set; }
        public string RightAnswer { get; set; }
        public Topic Topic { get; set; }
        public int TopicId { get; set; } 
        public Picture Picture { get; set; }
        public int? PictureId { get; set; }
    }
}
