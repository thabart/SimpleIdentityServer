using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.ResourceManager.API.Host.DTOs
{
    internal enum ElFinderCommands
    {
        Open,
        Parents,
        Mkdir,
        Rm,
        Rename,
        Mkfile,
        Tree,
        Duplicate,
        Paste,
        Ls,
        Search,
        Access,
        Perms,
        MkPerm,
        OpenIdClients
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
            { Constants.ElFinderCommands.Mkdir, ElFinderCommands.Mkdir },
            { Constants.ElFinderCommands.Rm, ElFinderCommands.Rm },
            { Constants.ElFinderCommands.Rename, ElFinderCommands.Rename },
            { Constants.ElFinderCommands.Mkfile, ElFinderCommands.Mkfile },
            { Constants.ElFinderCommands.Tree, ElFinderCommands.Tree },
            { Constants.ElFinderCommands.Duplicate, ElFinderCommands.Duplicate },
            { Constants.ElFinderCommands.Paste, ElFinderCommands.Paste },
            { Constants.ElFinderCommands.Ls, ElFinderCommands.Ls },
            { Constants.ElFinderCommands.Search, ElFinderCommands.Search },
            { Constants.ElFinderCommands.Access, ElFinderCommands.Access },
            { Constants.ElFinderCommands.Perms, ElFinderCommands.Perms },
            { Constants.ElFinderCommands.MkPerm, ElFinderCommands.MkPerm },
            { Constants.ElFinderCommands.OpenIdClients, ElFinderCommands.OpenIdClients }
        };

        private ElFinderParameter(ElFinderCommands command, string target, IEnumerable<string> targets, bool tree, 
            bool init, string name, string current, string source, string destination, bool cut, string q, JArray rules)
        {
            Command = command;
            Target = target;
            Targets = targets;
            Tree = tree;
            Init = init;
            Name = name;
            Source = source;
            Destination = destination;
            Cut = cut;
            Q = q;
        }

        public ElFinderCommands Command { get; private set; }
        public string Target { get; private set; }
        public IEnumerable<string> Targets { get; private set; }
        public bool Tree { get; private set; }
        public bool Init { get; private set; }
        public string Name { get; private set; }
        public string Current { get; private set; }
        public string Source { get; private set; }
        public string Destination { get; private set; }
        public bool Cut { get; private set; }
        public string Q { get; private set; }
        public JArray Rules { get; private set; }

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
            json.TryGetValue(Constants.ElFinderDtoNames.Target, out jtTarget);

            JToken jtTargets = null;
            json.TryGetValue(Constants.ElFinderDtoNames.Targets, out jtTargets);

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

            int init = 0;
            JToken jtInit;
            if (json.TryGetValue(Constants.ElFinderDtoNames.Init, out jtInit))
            {
                if (!int.TryParse(jtInit.ToString(), out init))
                {
                    return new DeserializedElFinderParameter(new ErrorResponse(string.Format(Constants.Errors.ErrParamNotValidInt, Constants.ElFinderDtoNames.Init)));
                }
            }

            JToken jtSource;
            json.TryGetValue(Constants.ElFinderDtoNames.Source, out jtSource);
            JToken jtDestination;
            json.TryGetValue(Constants.ElFinderDtoNames.Destination, out jtDestination);
            int cut = 0;
            JToken jtCut;
            if (json.TryGetValue(Constants.ElFinderDtoNames.Cut, out jtCut))
            {
                if (!int.TryParse(jtCut.ToString(), out cut))
                {
                    return new DeserializedElFinderParameter(new ErrorResponse(string.Format(Constants.Errors.ErrParamNotValidInt, Constants.ElFinderDtoNames.Cut)));
                }
            }

            JToken jtCurrent;
            json.TryGetValue(Constants.ElFinderDtoNames.Current, out jtCurrent);

            JToken jtQ;
            json.TryGetValue(Constants.ElFinderDtoNames.Q, out jtQ);

            var cmdStr = jtCmd.ToString();
            if (!_mappingStrToEnumCmd.ContainsKey(cmdStr))
            {
                return new DeserializedElFinderParameter(new ErrorResponse(Constants.ElFinderErrors.ErrUnknownCmd));
            }

            var targets = new List<string>();
            var jtTargetsArr = jtTargets as JArray;
            if (jtTargetsArr != null)
            {
                foreach (var r in jtTargetsArr)
                {
                    targets.Add(r.ToString());
                }
            }

            JToken jtRules;
            json.TryGetValue(Constants.ElFinderDtoNames.Rules, out jtRules);

            return new DeserializedElFinderParameter(new ElFinderParameter(_mappingStrToEnumCmd[cmdStr], jtTarget == null ? null : jtTarget.ToString(), targets,
                tree == 1, init == 1, name, jtCurrent == null ? null : jtCurrent.ToString(), jtSource == null ? null : jtSource.ToString(), jtDestination == null ? null : jtDestination.ToString(),
                cut == 1, jtQ == null ? null : jtQ.ToString(), jtRules == null ? null : jtRules as JArray));
        }
    }
}
