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

    public static class WriteTool {
        public const string Name = "write";
        const string Description = "Write content to a file";
        const string Param =
        """
            {
                "type": "object",
                "required": ["file_path", "content"],
                "properties": {
                    "file_path": {
                        "type": "string",
                        "description": "The path of the file to write to"
                    },
                    "content": {
                        "type": "string",
                        "description": "The content to write to the file"
                    }
                }
            }
        """;

        public static ChatTool GetChatTool() =>
            ChatTool.CreateFunctionTool(
                functionName: Name,
                functionDescription: Description,
                functionParameters: BinaryData.FromString(Param)
            );
    }

    public static class BashTool {
        public const string Name = "bash";
        const string Description = "Execute a shell command";
        const string Param =
        """
            {
                "type": "object",
                "required": ["command"],
                "properties": {
                    "command": {
                        "type": "string",
                        "description": "The command to execute"
                    }
                }
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