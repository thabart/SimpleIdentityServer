using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.ResourceManager.API.Host.DTOs
{
    internal enum ElFinderCommands
    {
        Open,
        Parents,
        Mkdir
    }
    
    internal sealed class DeserializedElFinderParameter
    {
        public DeserializedElFinderParameter(ErrorResponse errorResponse)
        {
            ErrorResponse = errorResponse;
        }
        
        public DeserializedElFinderParameter(ElFinderParameter elFinderParameter)
        {
            ElFinderParameter = elFinderParameter;
        }

        public ElFinderParameter ElFinderParameter { get; private set; }
        public ErrorResponse ErrorResponse { get; private set; }
    }

    public class ElFinderParam
    {
        public string Command { get; private set; }
        public string Target { get; private set; }
        public int Tree { get; private set; }
        public bool Init { get; private set; }
    }

    internal sealed class ElFinderParameter
    {
        private static Dictionary<string, ElFinderCommands> _mappingStrToEnumCmd = new Dictionary<string, ElFinderCommands>
        {
            { Constants.ElFinderCommands.Open, ElFinderCommands.Open },
            { Constants.ElFinderCommands.Parents, ElFinderCommands.Parents },
            { Constants.ElFinderCommands.Mkdir, ElFinderCommands.Mkdir }
        };

        private ElFinderParameter(ElFinderCommands command, string target, int tree, bool init, string name)
        {
            Command = command;
            Target = target;
            Tree = tree;
            Init = init;
            Name = name;
        }

        public ElFinderCommands Command { get; private set; }
        public string Target { get; private set; }
        public int Tree { get; private set; }
        public bool Init { get; private set; }
        public string Name { get; private set; }

        public static DeserializedElFinderParameter Deserialize(JObject json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            JToken jtCmd;
            if (!json.TryGetValue(Constants.ElFinderDtoNames.Cmd, out jtCmd))
            {
                return new DeserializedElFinderParameter(new ErrorResponse(Constants.ElFinderErrors.ErrUnknownCmd));
            }

            JToken jtTarget;
            if (!json.TryGetValue(Constants.ElFinderDtoNames.Target, out jtTarget))
            {
                return new DeserializedElFinderParameter(new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound));
            }

            JToken jtTree;
            int tree = 0;
            if (json.TryGetValue(Constants.ElFinderDtoNames.Tree, out jtTree))
            {

                var treeStr = jtTree.ToString();
                if (!int.TryParse(treeStr, out tree))
                {
                    return new DeserializedElFinderParameter(new ErrorResponse(string.Format(Constants.Errors.ErrParamNotValidInt, Constants.ElFinderDtoNames.Tree)));
                }
            }

            JToken jtName;
            string name = null;
            if (json.TryGetValue(Constants.ElFinderDtoNames.Name, out jtName))
            {
                name = jtName.ToString();
            }

            bool init = false;
            JToken jtInit;
            if (json.TryGetValue(Constants.ElFinderDtoNames.Init, out jtInit))
            {
                bool.TryParse(jtInit.ToString(), out init);
            }

            var cmdStr = jtCmd.ToString();
            if (!_mappingStrToEnumCmd.ContainsKey(cmdStr))
            {
                return new DeserializedElFinderParameter(new ErrorResponse(Constants.ElFinderErrors.ErrUnknownCmd));
            }

            return new DeserializedElFinderParameter(new ElFinderParameter(_mappingStrToEnumCmd[cmdStr], jtTarget.ToString(), tree, init, name));
        }
    }
}
