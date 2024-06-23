using Core.Domains;

namespace Core.Utilities
{
    public class MinorWarning : Warning
    {
        public MinorWarning(string message) : base(message)
        {
        }
    }
    public class Warning : Exception
    {
        public BaseEntity Entity { get; set; }
        public Warning(string? message)
            : base(message)
        {

        }
        public Warning(string? message, Exception? inner)
            : base(message, inner)
        {

        }
        public Warning(string? message, BaseEntity entity)
            :base(message)
        {
            Entity = entity;
        }
    }
}
