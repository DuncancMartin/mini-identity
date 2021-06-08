namespace IdentityCore.Models.Settings
{
    public class Settings
    {
        public string SendGridApiKey { get; set; }
        public string SendGridEmailAddress { get; set; }
        public string SendGridEmailName { get; set; }
        public string SendGridForgotPasswordTemplate { get; set; }
        public string SendGridForgotPasswordUrl { get; set; }
        public string DockerBuild { get; set; }
        public string PortalUrl { get; set; }
    }
}
