using Newtonsoft.Json;
using OgeApp.Entyties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Task = OgeApp.Entyties.Task;
using Topic = OgeApp.Entyties.Topic;

namespace OgeApp.TaskProcessing
{
    internal class TaskAdder
    {

        private static readonly string[] requiredKeys = new string[5]
        {
            "Topic",
            "TaskName",
            "TaskText",
            "TaskAnswer",
            "PictureLink",
        };

        private readonly EFDBContext _eFDBContext;
        private readonly string _path;

        public TaskAdder(string path, EFDBContext eFDBContext)
        {
            _eFDBContext = eFDBContext;
            _path = path;
        }

        public void Add()
        {
            var jsonList = ReadJson(_path);
            if (jsonList is null) { return; }
            for (int i = 0; i < jsonList.Count; i++)
            {
                var jsonDict = jsonList[i];
                if (jsonDict is not null)
                {
                    if (requiredKeys.Except(jsonDict.Keys).Any())
                    {
                        throw new Exception($"Json должен содержать только " +
                       $"следующие ключи : {string.Join(", ", requiredKeys)}");
                    }

                    Topic topic = GetTopic(jsonDict["Topic"]);

                    Task currentTask = new()
                    {
                        Name = jsonDict["TaskName"],
                        Text = jsonDict["TaskText"],
                        RightAnswer = jsonDict["TaskAnswer"],
                        TopicId = topic.Id,
                    };

                    if (!string.IsNullOrEmpty(jsonDict["PictureLink"]))
                    {
                        currentTask.PictureId = AddPictureAndGetId(jsonDict["PictureLink"]);
                    }

                    _eFDBContext.Add(currentTask);
                    topic.TaskNumber += 1;
                    _eFDBContext.Update(topic);
                    _eFDBContext.SaveChanges();
                }
            }
        }

        private Topic GetTopic(string topicName)
        {
            var topics = _eFDBContext.Topics.Local.ToObservableCollection().Where(topic => topic.Name == topicName);

            if (!topics.Any())
            {
                _eFDBContext.Add(new Topic() { Name = topicName });
                _eFDBContext.SaveChanges();
                return _eFDBContext.Topics.Local.ToObservableCollection().Where(topic => topic.Name == topicName).First();
            }
            else
            {
                return topics.First();

            }
        }

        private int AddPictureAndGetId(string path)
        {
            Picture picture = new()
            {
                Name = Path.GetFileName(path),
                PictureLink = Path.Combine(Directory.GetCurrentDirectory(), path)
            };
            _eFDBContext.Add(picture);
            _eFDBContext.SaveChanges();

            return _eFDBContext.Pictures.Local.ToObservableCollection().Where(picture => picture.PictureLink == Path.Combine(Directory.GetCurrentDirectory(), path)).First().Id;
        }

        private static List<Dictionary<string, string>>? ReadJson(string path) =>
            JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(File.ReadAllText(path));

    }
}
