using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rota.Domain.Entities
{
    public abstract class EntityBase
    {
        public int Id { get; protected set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedAt { get; private set; }
        
        public void SoftDelete()
        {
            if (IsDeleted) return;
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
        public void Restore()
        {
            IsDeleted = false;
            DeletedAt = null;
        }
        
    }
}