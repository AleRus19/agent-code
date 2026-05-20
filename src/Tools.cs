using System.Text.Json;
using OpenAI.Chat;

namespace Agent;

public static class Tools {
    public static class ReadTool {
        const string name = "Read";
        const string description = "Read and return the contents of a file";
        const string param =
        """
            {
                "type": "object",
                "properties": {
                    "file_path": {
                        "type": "string",
                        "description": "The path to the file to read"
                    }
                },
                "required": ["file_path"]
            }
        """;

        public static ChatTool GetChatTool() =>
            ChatTool.CreateFunctionTool(
                functionName: name,
                functionDescription: description,
                functionParameters: BinaryData.FromString(param)
            );
    }
}