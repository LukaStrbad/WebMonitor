using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebMonitor.Model.Db;

public class User
{
    [Key] [JsonIgnore] public int Id { get; set; }

    [Required]
    public required string Username { get; set; }

    [Required] [JsonIgnore] public string Password { get; set; } = "";

    public string DisplayName { get; set; } = "";

    public bool IsAdmin { get; set; } = false;

    [ForeignKey(nameof(AllowedFeatures))]
    [JsonIgnore]
    public int AllowedFeaturesId { get; set; }

    [Required]
    public virtual AllowedFeatures AllowedFeatures { get; set; } = new();
}
