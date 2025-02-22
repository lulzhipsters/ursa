namespace Ursa;

public class Settings
{
    [ConfigurationKeyName("URSA_DATABASE_CONNECTION")]
    public string DatabaseConnection { get; set; } = "";
}