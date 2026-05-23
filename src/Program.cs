using System.ClientModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using OpenAI;
using OpenAI.Assistants;
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
    Tools = { ReadTool.GetChatTool(), WriteTool.GetChatTool(), BashTool.GetChatTool() }
};

List<ChatMessage> messages = [new UserChatMessage(prompt)];

while (true) {

    ChatCompletion response = client.CompleteChat(
        messages,
        options: options
    );

    if (response.FinishReason == ChatFinishReason.Stop) {
        messages.Add(new AssistantChatMessage(response));
        Console.Write(response.Content[0].Text);
        return;
    }

    if (response.FinishReason == ChatFinishReason.ToolCalls) {
        foreach (var toolCallRequest in response.ToolCalls) {
            messages.Add(new AssistantChatMessage(response));
            var param = toolCallRequest.FunctionArguments.ToObjectFromJson<Dictionary<string, string>>() ?? [];

            switch (toolCallRequest.FunctionName) {
                case ReadTool.Name:
                    if (param.TryGetValue("file_path", out var p)) {
                        messages.Add(new ToolChatMessage(toolCallRequest.Id, await File.ReadAllTextAsync(p)));
                    }
                    break;
                case WriteTool.Name:
                    var content = param["content"];
                    var path = param["file_path"];

                    await File.WriteAllTextAsync(path, content);
                    messages.Add(new ToolChatMessage(toolCallRequest.Id, $"write successful"));

                    break;
                case BashTool.Name:
                    var command = param["command"];

                    var processStartInfo = new ProcessStartInfo {
                        FileName = command,
                        RedirectStandardOutput = true,
                        UseShellExecute = true
                    };
                    var process = Process.Start(processStartInfo);
                    var output = process!.StandardOutput.ReadToEnd();
                    var err = process!.StandardError.ReadToEnd();

                    await process.WaitForExitAsync();
                    if (!string.IsNullOrEmpty(err)) {
                        messages.Add(new ToolChatMessage(toolCallRequest.Id, $"result: {err}"));
                    }
                    else {
                        messages.Add(new ToolChatMessage(toolCallRequest.Id, $"result: {output}"));
                    }
                    break;
            }
        }
    }
}
