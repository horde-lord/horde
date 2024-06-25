using Horde.Core.Interfaces.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Horde.Core.Domains.World.Entities
{
    public class UserConfiguration : BaseEntity
    {
        [NotMapped]
        public override ContextNames Context => ContextNames.Ecosystem;
        public string Name { get; set; }
        public string Value { get; set; }
        public string Remarks { get; set; }
        public int UserId { get; set; }
        [NotMapped]
        public User User { get; set; }
        private static JsonSerializerOptions _options = new JsonSerializerOptions() { ReferenceHandler = ReferenceHandler.Preserve };
        public static UserConfiguration CreateConfig<T>(int userId, string name, T val, string remarks = "")
        {
            return new UserConfiguration()
            {
                UserId = userId,
                Name = name,
                Remarks = remarks,
                Value = JsonSerializer.Serialize(val,options: _options)
            };
        }

        public static UserConfiguration CreateConfig<T>(int userId, T val) where T : class
        {
            return new UserConfiguration() { UserId = userId, Name = nameof(T), Value = JsonSerializer.Serialize(val, options: _options) };
        }

        public T GetConfigValue<T>()
        {
            return JsonSerializer.Deserialize<T>(Value, options: _options);
        }

        public void SetConfigValue<T>(T t)
        {
            Value = JsonSerializer.Serialize<T>(t, options: _options);
        }
    }


}
