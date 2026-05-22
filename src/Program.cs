using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using static Agent.Tools;

if (args.Length < 2 || args[0] != "-p") {
    throw new Exception("Usage: program -p <prompt>");
}

var prompt = args[1];

if (string.IsNullOrEmpty(prompt)) {
    throw new Exception("Prompt must not be empty");
}

var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
var baseUrl = Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ?? "https://openrouter.ai/api/v1";

if (string.IsNullOrEmpty(apiKey)) {
    throw new Exception("OPENROUTER_API_KEY is not set");
}

var client = new ChatClient(
    model: "anthropic/claude-haiku-4.5",
    credential: new ApiKeyCredential(apiKey),
    options: new OpenAIClientOptions { Endpoint = new Uri(baseUrl) }
);

var options = new ChatCompletionOptions {
    Tools = { ReadTool.GetChatTool() }
};

ChatCompletion response = client.CompleteChat(
    [new UserChatMessage(prompt)],
    options: options
);

if (response.FinishReason == ChatFinishReason.ToolCalls) {
    foreach (var toolCallRequest in response.ToolCalls) {
        if (toolCallRequest.FunctionName == ReadTool.Name) {
            var param = toolCallRequest.FunctionArguments.ToObjectFromJson<Dictionary<string, string>>() ?? [];
            if (param.TryGetValue("file_path", out var p)) {
                var content = await File.ReadAllTextAsync(p);
                Console.WriteLine(content);
                return;
            }
        }
    }
}

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.Error.WriteLine("Logs from your program will appear here!");

// TODO: Uncomment the line below to pass the first stage
Console.Write(response.Content[0].Text);
