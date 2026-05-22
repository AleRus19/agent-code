using System.Text.Json;
using OpenAI.Chat;

namespace Agent;

public static class Tools {
    public static class ReadTool {
        public const string Name = "read";
        const string Description = "Read and return the contents of a file";
        const string Param =
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
                functionName: Name,
                functionDescription: Description,
                functionParameters: BinaryData.FromString(Param)
            );
    }
}