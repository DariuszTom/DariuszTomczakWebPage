namespace DariuszTomczakWebpage.Shared.AI
{
    public class AnthropicSettings
    {
        /// <summary>
        /// Base URL for Anthropic API. Default is the public Anthropic endpoint.
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.anthropic.com";

        /// <summary>
        /// Model to use (e.g. "claude-opus-4-latest", "claude-opus-4-20250514"). Match to what you have access to.
        /// </summary>
        public string Model { get; set; } = "claude-opus-5-5";

        /// <summary>
        /// Maximum model tokens to request (model-specific limits apply). Claude Opus 4 supports up to 200k output tokens.
        /// </summary>
        public int MaxTokens { get; set; } = 50000;

        /// <summary>
        /// System prompt that defines AI assistant's behavior and constraints. 
        /// Placeholder {UserName} should be replaced at runtime with the actual user's name.
        /// </summary>
        public string SystemPrompt { get; set; } = string.Empty;

    }
}
