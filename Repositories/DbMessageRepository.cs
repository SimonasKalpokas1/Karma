using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karma.data.messages;
using Karma.Models;
using Microsoft.EntityFrameworkCore;

namespace Karma.Repositories
{
    public class DbMessageRepository : IMessageRepository
    {
        public readonly BaseDbContext _context;

        public DbSet<Message> entities { get; set; }

        public DbMessageRepository(BaseDbContext context)
        {
            _context = context;
            entities = _context.Messages;
        }

        public IEnumerable<Message> GetAllByGroup(string groupId)
        {
            var messages =  from m in entities
                            where m.GroupId == groupId
                            select m;
            return messages;
        }

        public IEnumerable<Conversation> GetConversations(int userId)
        {
            var conversations = from c in _context.Conversations
                                where c.UserOneId == userId || c.UserTwoId == userId
                                select c; 
            return conversations;
        }

        public IEnumerable<Message> GetByLimit(string groupId, int limit)
        {
            var messages =  (from m in entities
                            orderby m.DateSent descending
                            select m).Take(limit).Reverse();
            return messages;
        }

        public IEnumerable<Message> GetByLimit(string groupId, int limit, string lastMessageId)
        {
            var query = from m in entities
                        orderby m.DateSent descending
                        select m;
            var offset = query.ToList().FindIndex(x => x.Id.ToString() == lastMessageId) + 1;
            var messages = query.Skip(offset).Take(limit).Reverse();
            return messages;
        }

        public void Add(List<Message> newMessages, string groupId)
        {
            entities.AddRange(newMessages);
            _context.SaveChanges();
        }

        public void DeleteById(string messageId, string groupId)
        {
            entities.Remove(entities.Find(Int32.TryParse(messageId, out int x))); //Parse messageId to int in MessageService
            _context.SaveChanges();
        }
    }
}