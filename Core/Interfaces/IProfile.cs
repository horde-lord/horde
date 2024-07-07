namespace Horde.Core.Interfaces
{
    public interface IProfile : INamed
    {
        public string Description { get; set; }
        public string ImageUrl { get; set; }

    }
}

